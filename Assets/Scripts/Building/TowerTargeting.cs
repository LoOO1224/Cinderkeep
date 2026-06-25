using System.Collections;
using UnityEngine;

// 타워 공격 범위 안에서 가장 가까운 살아있는 적을 주기적으로 찾습니다.
// 웨이브와 보스가 동시에 접근해도 BuildingTower는 CurrentTarget만 읽어서 공격합니다.
public sealed class TowerTargeting : MonoBehaviour
{
    private const int MaxOverlapCount = 20;
    private const float ScanDelay = 0.2f;

    [Header("Targeting Settings")]
    [Tooltip("적을 찾을 최대 거리입니다. BuildingTower 초기화 시 buildings.json의 AttackRange로 덮어씁니다.")]
    [SerializeField] private float _attackRange = 8f;
    [Tooltip("감지 기준 위치입니다. 비어 있으면 이 오브젝트 위치를 사용합니다.")]
    [SerializeField] private Transform _scanPoint;

    private readonly Collider[] _overlapColliders = new Collider[MaxOverlapCount];
    private Coroutine _scanRoutine;
    private EnemyStatus _currentTarget;

    public EnemyStatus CurrentTarget
    {
        get
        {
            return _currentTarget;
        }
    }

    public bool HasTarget
    {
        get
        {
            return _currentTarget != null;
        }
    }

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
        _currentTarget = FindNearestEnemy();
    }

    private EnemyStatus FindNearestEnemy()
    {
        Vector3 scanPosition = _scanPoint.position;
        int hitCount = Physics.OverlapSphereNonAlloc(
            scanPosition,
            _attackRange,
            _overlapColliders);

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

        return targetCollider.GetComponentInParent<EnemyStatus>() != null;
    }

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
    private void OnDrawGizmosSelected()
    {
        ConnectScan();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_scanPoint.position, _attackRange);
    }
#endif
}
