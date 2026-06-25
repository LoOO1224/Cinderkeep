using UnityEngine;
using UnityEngine.AI;

// 1인칭 플레이어의 입력, 상태, 장착, 채집, 전투, 건축 중 한 흐름을 담당합니다.
// 입력 제어와 실제 효과를 분리해 퀵슬롯, 도구, 무기, 튜토리얼이 서로 얽히지 않게 합니다.
// T 키 귀환 시전과 실제 위치 이동을 담당하는 플레이어 전용 컴포넌트입니다.
// 베이스 도착 지점과 진행 UI는 씬 또는 프리팹에서 연결하고, 피격 취소는 PlayerStatus 이벤트로 받습니다.
public sealed class PlayerBaseTeleport : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("베이스 귀환을 시작하는 키입니다. CinderkeepInput에 등록된 키만 동작합니다.")]
    [SerializeField] private KeyCode _teleportKey = KeyCode.T;

    [Header("Teleport Rule")]
    [Tooltip("귀환이 완료되기까지 걸리는 시간입니다. 나중에 데이터화가 필요하면 player_teleport.json 같은 별도 스키마로 분리하세요.")]
    [SerializeField] private float _teleportCastSeconds = 3f;
    [Tooltip("베이스에서 이 거리보다 가까우면 귀환을 시작하지 않습니다.")]
    [SerializeField] private float _minimumDistanceFromBase = 20f;
    [Tooltip("도착 위치를 NavMesh 위의 가까운 지점으로 보정할지 결정합니다.")]
    [SerializeField] private bool _snapDestinationToNavMesh = true;
    [Tooltip("NavMesh 도착 위치를 찾을 때 허용할 탐색 반경입니다.")]
    [SerializeField] private float _navMeshSampleRadius = 4f;

    [Header("Connected Components")]
    [Tooltip("귀환 완료 후 이동할 베이스 지점입니다. CinderHeart 근처 빈 Transform을 연결하는 것을 권장합니다.")]
    [SerializeField] private Transform _baseTeleportPoint;
    [Tooltip("플레이어 CharacterController입니다. 순간 이동할 때 잠시 비활성화했다가 복구합니다.")]
    [SerializeField] private CharacterController _characterController;
    [Tooltip("피격 취소를 받기 위한 PlayerStatus입니다.")]
    [SerializeField] private PlayerStatus _playerStatus;
    [Tooltip("귀환 진행률을 표시하는 UI 컴포넌트입니다.")]
    [SerializeField] private BaseTeleportProgressUI _progressUI;

    private bool _isTeleporting;
    private float _teleportTimer;

    public bool IsTeleporting
    {
        get
        {
            return _isTeleporting;
        }
    }

    private void Awake()
    {
        ConnectLocalComponents();
        CloseProgressUI();
    }

    private void OnEnable()
    {
        SubscribePlayerStatus();
    }

    private void OnDisable()
    {
        UnsubscribePlayerStatus();
        CancelTeleport();
    }

    private void Update()
    {
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            CancelTeleport();
            return;
        }

        if (_isTeleporting == true)
        {
            UpdateTeleportCast();
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(_teleportKey) == true)
        {
            TryStartTeleport();
        }
    }

    public void SetBaseTeleportPoint(Transform baseTeleportPoint)
    {
        _baseTeleportPoint = baseTeleportPoint;
    }

    public void SetProgressUI(BaseTeleportProgressUI progressUI)
    {
        _progressUI = progressUI;
        CloseProgressUI();
    }

    public void TryStartTeleport()
    {
        if (CanStartTeleport() == false)
        {
            return;
        }

        StartTeleport();
    }

    public void CancelTeleport()
    {
        if (_isTeleporting == false)
        {
            return;
        }

        _isTeleporting = false;
        _teleportTimer = 0f;
        CloseProgressUI();
    }

    private void ConnectLocalComponents()
    {
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }

        if (_playerStatus == null)
        {
            _playerStatus = GetComponent<PlayerStatus>();
        }
    }

    private void SubscribePlayerStatus()
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.PlayerDamaged += HandlePlayerDamaged;
    }

    private void UnsubscribePlayerStatus()
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.PlayerDamaged -= HandlePlayerDamaged;
    }

    private bool CanStartTeleport()
    {
        if (_baseTeleportPoint == null)
        {
            Debug.LogWarning("[PlayerBaseTeleport] 베이스 귀환 지점이 연결되지 않았습니다.");
            return false;
        }

        float distanceFromBase = Vector3.Distance(transform.position, _baseTeleportPoint.position);
        if (distanceFromBase < _minimumDistanceFromBase)
        {
            global::CinderkeepLog.Verbose("[PlayerBaseTeleport] 베이스와 가까워 귀환을 시작하지 않습니다.");
            return false;
        }

        return true;
    }

    private void StartTeleport()
    {
        _teleportCastSeconds = Mathf.Max(_teleportCastSeconds, 0.1f);
        _teleportTimer = 0f;
        _isTeleporting = true;

        if (_progressUI != null)
        {
            _progressUI.Open(_teleportCastSeconds);
        }
    }

    private void UpdateTeleportCast()
    {
        _teleportTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_teleportTimer / _teleportCastSeconds);

        if (_progressUI != null)
        {
            _progressUI.SetProgress(progress);
        }

        if (progress >= 1f)
        {
            CompleteTeleport();
        }
    }

    private void CompleteTeleport()
    {
        MoveToBase();
        _isTeleporting = false;
        _teleportTimer = 0f;
        CloseProgressUI();
    }

    private void MoveToBase()
    {
        Vector3 destination = GetDestinationPosition();
        Quaternion destinationRotation = _baseTeleportPoint.rotation;

        if (_characterController != null)
        {
            _characterController.enabled = false;
        }

        transform.SetPositionAndRotation(destination, destinationRotation);

        if (_characterController != null)
        {
            _characterController.enabled = true;
        }
    }

    private Vector3 GetDestinationPosition()
    {
        Vector3 destination = _baseTeleportPoint.position;

        if (_snapDestinationToNavMesh == false)
        {
            return destination;
        }

        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(destination, out navMeshHit, _navMeshSampleRadius, NavMesh.AllAreas) == true)
        {
            return navMeshHit.position;
        }

        return destination;
    }

    private void HandlePlayerDamaged(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        CancelTeleport();
    }

    private void CloseProgressUI()
    {
        if (_progressUI == null)
        {
            return;
        }

        _progressUI.Close();
    }
}
