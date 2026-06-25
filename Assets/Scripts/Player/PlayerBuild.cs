using Cinderkeep.Gameplay;
using UnityEngine;

// 1인칭 플레이어의 입력, 상태, 장착, 채집, 전투, 건축 중 한 흐름을 담당합니다.
// 입력 제어와 실제 효과를 분리해 퀵슬롯, 도구, 무기, 튜토리얼이 서로 얽히지 않게 합니다.
// 플레이어의 건축 입력과 E 홀드 진행을 담당하는 컴포넌트입니다.
// 실제 생성, 등록, 비용 처리는 BuildingManager가 담당합니다.
public sealed class PlayerBuild : MonoBehaviour
{
    private const string BuildSpotLayerName = "BuildSpot";

    [Header("Build Input")]
    [Tooltip("건축 지점을 바라본 채 누르고 있으면 건축 게이지가 차는 키입니다.")]
    [SerializeField] private KeyCode _holdBuildKey = KeyCode.E;
    [Tooltip("개발 테스트용 즉시 건축 키입니다. 실제 플레이 기준은 E 홀드입니다.")]
    [SerializeField] private KeyCode _debugInstantBuildKey = KeyCode.B;
    [Tooltip("건축이 완료될 때까지 E를 누르고 있어야 하는 시간입니다.")]
    [SerializeField] private float _buildHoldSeconds = 1.2f;

    [Header("Build Detection")]
    [Tooltip("건축 지점을 감지하는 최대 거리입니다.")]
    [SerializeField] private float _buildDetectDistance = 5f;
    [Tooltip("건축 Raycast가 맞출 레이어입니다. 비어 있으면 BuildSpot 레이어를 사용합니다.")]
    [SerializeField] private LayerMask _buildLayerMask;
    [Tooltip("상호작용 Ray가 시작되는 카메라 Transform입니다. 비어 있으면 자식 카메라를 찾습니다.")]
    [SerializeField] private Transform _cameraTransform;

    [Header("Connected Manager")]
    [Tooltip("실제 건축물 생성을 맡는 매니저입니다. 비어 있으면 GameManager를 통해 연결합니다.")]
    [SerializeField] private BuildingManager _buildingManager;

    [Header("Connected UI")]
    [Tooltip("건축 진행률을 표시하는 UI입니다. 비어 있으면 HUD Canvas 아래에 자동 생성합니다.")]
    [SerializeField] private BuildProgressUI _progressUI;

    private BuildingSpot _holdingSpot;
    private float _holdSeconds;
    private bool _isBuildCompletedWhileHolding;

    private void Start()
    {
        InitializeBuildLayerMask();
        ConnectBuildingManager();
        ConnectCamera();
        ConnectProgressUI();
    }

    private void Update()
    {
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            ResetHoldBuild();
            _isBuildCompletedWhileHolding = false;
            return;
        }

        ReadDebugBuildInput();
        UpdateHoldBuildInput();
    }

    public void SetBuildingManager(BuildingManager buildingManager)
    {
        _buildingManager = buildingManager;
    }

    private void ReadDebugBuildInput()
    {
        if (CinderkeepInput.WasKeyPressedThisFrame(_debugInstantBuildKey))
        {
            TryBuildInstant();
        }
    }

    private void UpdateHoldBuildInput()
    {
        if (CinderkeepInput.IsKeyPressed(_holdBuildKey) == false)
        {
            ResetHoldBuild();
            _isBuildCompletedWhileHolding = false;
            return;
        }

        if (_isBuildCompletedWhileHolding)
        {
            return;
        }

        if (CanRequestBuild() == false)
        {
            ResetHoldBuild();
            return;
        }

        BuildingSpot buildingSpot = GetBuildingSpotFromRay(false);
        if (buildingSpot == null)
        {
            ResetHoldBuild();
            return;
        }

        if (_holdingSpot != buildingSpot)
        {
            StartHoldBuild(buildingSpot);
        }

        _holdSeconds += Time.deltaTime;
        RefreshHoldProgress();

        if (_holdSeconds < _buildHoldSeconds)
        {
            return;
        }

        bool isBuilt = TryBuildAtSpot(_holdingSpot);
        _isBuildCompletedWhileHolding = true;
        ResetHoldBuild();

        if (isBuilt)
        {
            global::CinderkeepLog.Verbose("PlayerBuild: E 홀드 건축을 완료했습니다.");
        }
    }

    private void TryBuildInstant()
    {
        if (CanRequestBuild() == false)
        {
            return;
        }

        BuildingSpot buildingSpot = GetBuildingSpotFromRay(true);
        if (buildingSpot == null)
        {
            return;
        }

        if (TryBuildAtSpot(buildingSpot))
        {
            global::CinderkeepLog.Verbose("PlayerBuild: 디버그 즉시 건축을 완료했습니다.");
        }
    }

    private bool TryBuildAtSpot(BuildingSpot buildingSpot)
    {
        if (GameManager.Inst == null)
        {
            Debug.LogWarning("PlayerBuild: GameManager 연결이 필요합니다.");
            return false;
        }

        return _buildingManager.TryBuildAtSpot(
            buildingSpot,
            GameManager.Inst.PlayerModel,
            GameManager.Inst.GetGameDataManager());
    }

    private void StartHoldBuild(BuildingSpot buildingSpot)
    {
        _holdingSpot = buildingSpot;
        _holdSeconds = 0f;
        ConnectProgressUI();

        if (_progressUI != null)
        {
            _progressUI.Open(GetBuildingDisplayName(buildingSpot), _buildHoldSeconds);
        }
    }

    private void RefreshHoldProgress()
    {
        ConnectProgressUI();
        if (_progressUI == null)
        {
            return;
        }

        _progressUI.SetProgress(_holdSeconds / Mathf.Max(0.1f, _buildHoldSeconds));
    }

    private void ResetHoldBuild()
    {
        _holdingSpot = null;
        _holdSeconds = 0f;

        if (_progressUI != null)
        {
            _progressUI.Close();
        }
    }

    private bool CanRequestBuild()
    {
        ConnectBuildingManager();

        if (_buildingManager == null)
        {
            Debug.LogWarning("PlayerBuild: BuildingManager 연결이 필요합니다.");
            return false;
        }

        return true;
    }

    private BuildingSpot GetBuildingSpotFromRay(bool shouldLogWarning)
    {
        ConnectCamera();
        if (_cameraTransform == null)
        {
            if (shouldLogWarning)
            {
                Debug.LogWarning("PlayerBuild: 카메라 Transform이 필요합니다.");
            }

            return null;
        }

        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, _buildDetectDistance, _buildLayerMask) == false)
        {
            if (shouldLogWarning)
            {
                Debug.LogWarning("PlayerBuild: 바라보는 방향에 건축 가능한 BuildingSpot이 없습니다.");
            }

            return null;
        }

        BuildingSpot buildingSpot = hitInfo.collider.GetComponentInParent<BuildingSpot>();
        if (buildingSpot == null)
        {
            return null;
        }

        return buildingSpot;
    }

    private string GetBuildingDisplayName(BuildingSpot buildingSpot)
    {
        if (buildingSpot == null)
        {
            return string.Empty;
        }

        if (GameManager.Inst == null)
        {
            return buildingSpot.BuildingDataId;
        }

        GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
        BuildingData buildingData = GetDisplayBuildingData(buildingSpot, gameDataManager);
        if (buildingData != null && string.IsNullOrEmpty(buildingData.DisplayName) == false)
        {
            return buildingData.DisplayName;
        }

        return buildingSpot.BuildingDataId;
    }

    private BuildingData GetDisplayBuildingData(BuildingSpot buildingSpot, GameDataManager gameDataManager)
    {
        if (buildingSpot == null || gameDataManager == null)
        {
            return null;
        }

        if (buildingSpot.CanBuild())
        {
            return gameDataManager.GetBuilding(buildingSpot.BuildingDataId);
        }

        string fromBuildingId = string.IsNullOrEmpty(buildingSpot.CurrentBuildingDataId)
            ? buildingSpot.BuildingDataId
            : buildingSpot.CurrentBuildingDataId;

        int currentDay = GameManager.Inst == null || GameManager.Inst.GameRunModel == null
            ? GameRunModel.FirstDay
            : GameManager.Inst.GameRunModel.Day;

        foreach (BuildingUpgradeData upgradeData in gameDataManager.BuildingUpgradeDataList.Values)
        {
            if (upgradeData == null || upgradeData.RequiredDay > currentDay)
            {
                continue;
            }

            if (string.Equals(upgradeData.FromBuildingId, fromBuildingId, System.StringComparison.OrdinalIgnoreCase))
            {
                return gameDataManager.GetBuilding(upgradeData.ToBuildingId);
            }
        }

        return gameDataManager.GetBuilding(fromBuildingId);
    }

    private void ConnectCamera()
    {
        if (_cameraTransform != null)
        {
            return;
        }

        Camera camera = GetComponentInChildren<Camera>(true);
        if (camera != null)
        {
            _cameraTransform = camera.transform;
        }
    }

    private void InitializeBuildLayerMask()
    {
        if (_buildLayerMask != 0)
        {
            return;
        }

        _buildLayerMask = LayerMask.GetMask(BuildSpotLayerName);
    }

    private void ConnectBuildingManager()
    {
        if (_buildingManager != null)
        {
            return;
        }

        if (GameManager.Inst == null)
        {
            return;
        }

        _buildingManager = GameManager.Inst.GetBuildingManager();
    }

    private void ConnectProgressUI()
    {
        if (_progressUI != null)
        {
            return;
        }

        _progressUI = BuildProgressUI.EnsureSceneUI();
    }
}
