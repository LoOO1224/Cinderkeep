using Cinderkeep.Gameplay;
using System.Collections;
using UnityEngine;

// 몬스터가 플레이어를 감지하는 컴포넌트입니다.
// 감지 결과만 들고 있고, 이동과 공격은 EnemyMovement와 EnemyBrain이 처리합니다.
public sealed class EnemyDetector : MonoBehaviour
{
    [Tooltip("낮에 플레이어를 감지할 수 있는 시야각입니다. 피격 반응 감지는 이 각도 제한을 무시합니다.")]
    [SerializeField] private float _dayViewAngle = 150f;
    [Tooltip("밤에 플레이어를 감지할 수 있는 시야각입니다. 피격 반응 감지는 이 각도 제한을 무시합니다.")]
    [SerializeField] private float _nightViewAngle = 90f;

    private float _viewAngle = 90f;
    private const string PlayerTag = "Player";
    private const int MaxOverlapCount = 20;
    private const float DefaultDetectionInterval = 0.2f;

    private readonly Collider[] _overlapColliders = new Collider[MaxOverlapCount];

    private Coroutine _detectionRoutine;
    [Tooltip("플레이어 감지 갱신 주기입니다. EnemyData의 _detectorInterval 값으로 초기화됩니다.")]
    [SerializeField] private float _detectionInterval = DefaultDetectionInterval;
    [Tooltip("낮에 플레이어를 감지할 수 있는 거리입니다.")]
    [SerializeField] private float _dayDetectorDistance = 6f;
    [Tooltip("밤에 플레이어를 감지할 수 있는 거리입니다.")]
    [SerializeField] private float _nightDetectorDistance = 12f;

    private float _detectorDistance = 12f;
    private bool _isNightDetectionEnabled;

    public Transform DetectedPlayer { get; private set; }

    public bool HasDetectedPlayer
    {
        get
        {
            return DetectedPlayer != null;
        }
    }

    private void OnEnable()
    {
        RefreshDetectorDistance();
        RefreshViewAngle();
        StartDetectionRoutine();
    }

    private void OnDisable()
    {
        StopDetectionRoutine();
    }

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        _dayDetectorDistance = enemyData.DetectorDistance;
        if (enemyData.DetectorInterval > 0f)
        {
            _detectionInterval = enemyData.DetectorInterval;
        }

        RefreshDetectorDistance();

        if (isActiveAndEnabled)
        {
            StartDetectionRoutine();
        }
    }

    public void EnableAlertMode()
    {
        if (HasDetectedPlayer)
        {
            return;
        }

        DetectPlayerWithoutViewAngle();
    }

    public void SetNightDetectionEnabled(bool isEnabled)
    {
        _isNightDetectionEnabled = isEnabled;
        RefreshDetectorDistance();
        RefreshViewAngle();

        if (HasDetectedPlayer)
        {
            ClearPlayerIfOutOfRange();
        }
    }

    private void RefreshDetectorDistance()
    {
        if (_isNightDetectionEnabled)
        {
            _detectorDistance = _nightDetectorDistance;
            return;
        }

        _detectorDistance = _dayDetectorDistance;
    }

    private void RefreshViewAngle()
    {
        if (_isNightDetectionEnabled)
        {
            _viewAngle = _nightViewAngle;
            return;
        }

        _viewAngle = _dayViewAngle;
    }

    private void StartDetectionRoutine()
    {
        StopDetectionRoutine();
        _detectionRoutine = StartCoroutine(DetectPlayerRoutine());
    }

    private void StopDetectionRoutine()
    {
        if (_detectionRoutine == null)
        {
            return;
        }

        StopCoroutine(_detectionRoutine);
        _detectionRoutine = null;
    }

    private IEnumerator DetectPlayerRoutine()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(_detectionInterval);

        while (true)
        {
            DetectPlayer();
            yield return waitInterval;
        }
    }

    private void DetectPlayer()
    {
        if (HasDetectedPlayer)
        {
            ClearPlayerIfOutOfRange();
            return;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider targetCollider = _overlapColliders[i];
            if (IsPlayerTarget(targetCollider) == false)
            {
                continue;
            }

            if (IsInViewAngle(targetCollider.transform))
            {
                DetectedPlayer = targetCollider.transform;
                return;
            }
        }
    }

    private void DetectPlayerWithoutViewAngle()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider targetCollider = _overlapColliders[i];
            if (IsPlayerTarget(targetCollider))
            {
                DetectedPlayer = targetCollider.transform;
                Debug.Log(gameObject.name + ": 피격 반응으로 플레이어를 감지했습니다.");
                return;
            }
        }

        Debug.Log(gameObject.name + ": 피격되었지만 감지 범위 안에 플레이어가 없습니다.");
    }

    private void ClearPlayerIfOutOfRange()
    {
        if (DetectedPlayer == null)
        {
            return;
        }

        float currentDistance = Vector3.Distance(transform.position, DetectedPlayer.position);
        if (currentDistance > _detectorDistance)
        {
            DetectedPlayer = null;
        }
    }

    private bool IsPlayerTarget(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return false;
        }

        if (targetCollider.transform == transform)
        {
            return false;
        }

        return targetCollider.CompareTag(PlayerTag);
    }

    private bool IsInViewAngle(Transform targetTransform)
    {
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= _viewAngle * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectorDistance);

        Gizmos.color = Color.yellow;

        Vector3 forwardDirection = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle * 0.5f, 0) * forwardDirection;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle * 0.5f, 0) * forwardDirection;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * _detectorDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * _detectorDistance);

        Gizmos.DrawLine(transform.position + leftBoundary * _detectorDistance, transform.position + rightBoundary * _detectorDistance);
    }
}
