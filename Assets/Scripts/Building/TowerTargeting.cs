using System.Collections;
using UnityEngine;

public sealed class TowerTargeting : MonoBehaviour
{
    private const string EnemyTag = "Enemy";
    private const int MaxOverlapCount = 20;
    private const float ScanDelay = 0.2f;

    [Header("Targeting Settings")]
    [Tooltip("적을 찾을 최대 거리입니다. buildings.json의 attackRange로 덮어씁니다.")]
    [SerializeField] private float _attackRange = 8f;
    [Tooltip("감지 기준 위치입니다. 비어 있으면 이 오브젝트의 위치를 사용합니다.")]
    [SerializeField] private Transform _scanPoint;

    private readonly Collider[] _overlapColliders = new Collider[MaxOverlapCount];
    private Coroutine _scanRoutine;
    private EnemyStatus _currentTarget;

    public EnemyStatus CurrentTarget { get { return _currentTarget; } }

    public bool HasTarget { get { return _currentTarget != null; } }

    private void Awake()
    {
        ConnectScan();
    }

    private void OnEnable()
    {
        StartScanRoutine();
    }

    private void OnDisable()
    {
        StopScanRoutine();
        _currentTarget = null;
    }

    public void SetAttackRange(float attackRange)
    {
        _attackRange = Mathf.Max(0.1f, attackRange);
    }

    // 타워 추가 사거리 관련
    public void AddAttackRangeBonus(float bonusRange)
    {
        if (bonusRange <= 0f)
        {
            return;
        }

        _attackRange += bonusRange;
    }

    private void ConnectScan()
    {
        if (_scanPoint != null)
        {
            return;
        }

        _scanPoint = transform;
    }

    private void StartScanRoutine()
    {
        StopScanRoutine();
        _scanRoutine = StartCoroutine(ScanEnemyRoutine());
    }

    private void StopScanRoutine()
    {
        if (_scanRoutine == null)
        {
            return;
        }

        StopCoroutine(_scanRoutine);
        _scanRoutine = null;
    }

    private IEnumerator ScanEnemyRoutine()
    {
        WaitForSeconds waitDelay = new WaitForSeconds(ScanDelay);

        while (true)
        {
            RefreshCurrentTarget();
            yield return waitDelay;
        }
    }

    private void RefreshCurrentTarget()
    {
        ConnectScan();

        if (IsValidTarget(_currentTarget))
        {
            return;
        }

        _currentTarget = FindNearestEnemy();
    }

    private EnemyStatus FindNearestEnemy()
    {
        Vector3 scanPosition = _scanPoint.position;
        int hitCount = Physics.OverlapSphereNonAlloc(
            scanPosition,
            _attackRange,
            _overlapColliders
            );

        EnemyStatus nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            Collider targetCollider = _overlapColliders[i];
            if (IsEnemyCollider(targetCollider) == false)
            {
                continue;
            }

            EnemyStatus enemyStatus = targetCollider.GetComponentInParent<EnemyStatus>();
            if (IsValidTarget(enemyStatus) == false)
            {
                continue;
            }

            float distance = Vector3.Distance(scanPosition, enemyStatus.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemyStatus;
            }
        }

        return nearestEnemy;
    }

    private bool IsEnemyCollider(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return false;
        }

        if (targetCollider.transform == transform)
        {
            return false;
        }

        return targetCollider.CompareTag(EnemyTag);
    }

    // 여러 상황들 유효 검사
    private bool IsValidTarget(EnemyStatus enemyStatus)
    {
        if (enemyStatus == null)
        {
            return false;
        }

        if (enemyStatus.IsDead)
        {
            return false;
        }

        if (enemyStatus.gameObject.activeInHierarchy == false)
        {
            return false;
        }

        ConnectScan();
        float distance = Vector3.Distance(_scanPoint.position, enemyStatus.transform.position);
        return distance <= _attackRange;
    }

#if UNITY_EDITOR
    // 에디터에서 사거리를 눈으로 확인하는 용
    private void OnDrawGizmosSelected()
    {
        ConnectScan();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_scanPoint.position, _attackRange);
    }
#endif
}