using Cinderkeep.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

// 플레이어의 건축 입력을 담당하는 컴포넌트입니다.
// 실제 건축물 생성은 BuildingManager가 담당하고, 이 클래스는 "어디에 무엇을 지을지"만 요청합니다.
public sealed class PlayerBuild : MonoBehaviour
{
    [Header("Build Input")]
    [Tooltip("건축을 시도하는 입력 키입니다.")]
    [SerializeField] private KeyCode _buildKey = KeyCode.B;

    [Header("Build Detection")]
    [Tooltip("건축 지점을 감지하는 최대 거리입니다.")]
    [SerializeField] private float _buildDetectDistance = 5f;

    private const string BuildSpotLayerName = "BuildSpot";

    [Tooltip("건축 Raycast가 맞출 레이어입니다. 비어 있으면 BuildSpot 레이어를 자동으로 사용합니다.")]
    [SerializeField] private LayerMask _buildLayerMask;

    [Tooltip("상호작용 Ray가 시작되는 카메라 Transform입니다. 비어있으면 자식 카메라를 찾습니다.")]
    [SerializeField] private Transform _cameraTransform;

    [Header("Build Prefab")]
    [FormerlySerializedAs("Prefab_Fence")]
    [FormerlySerializedAs("GameObject_BuildingPrefab")]
    [Tooltip("생성할 건축물 프리팹입니다.")]
    [SerializeField] private GameObject _buildingPrefab;

    [Header("Connected Manager")]
    [Tooltip("실제 건축물 생성을 맡는 매니저입니다. 비어 있으면 GameManager를 통해 연결합니다.")]
    [SerializeField] private BuildingManager _buildingManager;

    private void Start()
    {
        InitializeBuildLayerMask();
        ConnectBuildingManager();
        ConnectCamera();
    }

    private void Update()
    {
        ReadBuildInput();
    }

    public void SetBuildingManager(BuildingManager buildingManager)
    {
        _buildingManager = buildingManager;
    }

    private void ReadBuildInput()
    {
        if (CinderkeepInput.WasKeyPressedThisFrame(_buildKey))
        {
            TryBuild();
        }
    }

    private void TryBuild()
    {
        // PlayerBuild는 입력과 BuildingSpot 감지만 담당합니다.
        // 실제 생성, 등록, 비용 처리는 BuildingManager와 데이터 스키마 쪽으로 넘깁니다.
        if (CanRequestBuild() == false)
        {
            return;
        }

        BuildingSpot buildingSpot = GetBuildingSpotFromRay();
        if (buildingSpot == null)
        {
            Debug.LogWarning("PlayerBuild: 바라보는 방향에 건축 가능한 BuildingSpot이 없습니다.");
            return;
        }

        bool isBuilt = _buildingManager.TryBuildAtSpot(buildingSpot, _buildingPrefab);
        if (isBuilt == true)
        {
            Debug.Log("PlayerBuild: BuildingSpot에 건축물을 설치했습니다.");
        }
    }

    private bool CanRequestBuild()
    {
        ConnectBuildingManager();

        if (_buildingPrefab == null)
        {
            Debug.LogWarning("PlayerBuild: 건축 프리팹이 비어 있습니다.");
            return false;
        }

        if (_buildingManager == null)
        {
            Debug.LogWarning("PlayerBuild: BuildingManager 연결이 필요합니다.");
            return false;
        }

        return true;
    }

    private BuildingSpot GetBuildingSpotFromRay()
    {
        // 건축 지점은 BuildSpot 레이어와 BuildingSpot 컴포넌트를 기준으로 찾습니다.
        // 맵 작업자는 건축 가능한 위치에 BuildingSpot을 배치하고 레이어를 맞춰야 합니다.
        ConnectCamera();
        if (_cameraTransform == null)
        {
            Debug.LogWarning("PlayerBuild: 카메라 Transform이 필요합니다.");
            return null;
        }

        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, _buildDetectDistance, _buildLayerMask) == false)
        {
            return null;
        }

        BuildingSpot buildingSpot = hitInfo.collider.GetComponentInParent<BuildingSpot>();
        if (buildingSpot == null)
        {
            return null;
        }

        if (buildingSpot.CanBuild() == false)
        {
            Debug.LogWarning("PlayerBuild: 이 BuildingSpot에는 이미 건축물이 있습니다.");
            return null;
        }

        return buildingSpot;
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
}
