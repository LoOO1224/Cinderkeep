using System.Collections;
using UnityEngine;

// 타워 공격 범위 안에서 가장 가까운 살아있는 적을 주기적으로 찾습니다.
// Enemy가 BuildingHp + Build Tag로 건물을 찾는 것과 같이,
// EnemyStatus + Enemy Tag로 적만 찾습니다.
public sealed class TowerTargeting : MonoBehaviour
{
    private const string EnemyTag = "Enemy";
    private const float ScanDelay = 0.2f;

    [Header("Targeting Settings")]
    [Tooltip("적을 찾을 최대 거리입니다. BuildingTower 초기화 시 buildings.json의 attackRange로 덮어씁니다.")]
    [SerializeField] private float _attackRange = 8f;
    [Tooltip("감지 기준 위치입니다. 비어 있으면 이 오브젝트 위치를 사용합니다.")]
    [SerializeField] private Transform _scanPoint;

    private BuildingTower _ownerTower;
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
        _ownerTower = GetComponent<BuildingTower>();
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

    // 사거리 안 콜리더 탐색(트리거 제외)
    private EnemyStatus FindNearestEnemy()
    {
        Vector3 scanPosition = _scanPoint.position;

        // Trigger(BuildSpot 등)는 무시합니다.
        Collider[] overlapColliders = Physics.OverlapSphere(
            scanPosition,
            _attackRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        if (overlapColliders == null || overlapColliders.Length == 0)
        {
            return null;
        }

        return TowerTargetSelector.SelectNearestEnemyStatus(
            scanPosition,
            overlapColliders,
            overlapColliders.Length,
            ResolveEnemyStatusFromCollider);
    }

    private EnemyStatus ResolveEnemyStatusFromCollider(Collider targetCollider)
    {
        if (IsEnemyCollider(targetCollider) == false)
        {
            return null;
        }

        EnemyStatus enemyStatus = targetCollider.GetComponentInParent<EnemyStatus>();
        if (IsValidTarget(enemyStatus) == false)
        {
            return null;
        }

        return enemyStatus;
    }

    // Enemy 태그와 EnemyStatus가 있는지 확인하고 타워 자신 스스로 제외
    private bool IsEnemyCollider(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return false;
        }

        // 자기 타워 콜리더는 제외합니다
        if (_ownerTower != null)
        {
            BuildingTower towerOnCollider = targetCollider.GetComponentInParent<BuildingTower>();
            if (towerOnCollider == _ownerTower)
            {
                return false;
            }
        }

        EnemyStatus enemyStatus = targetCollider.GetComponentInParent<EnemyStatus>();
        if (enemyStatus == null)
        {
            return false;
        }

        if (enemyStatus.CompareTag(EnemyTag) == false)
        {
            return false;
        }

        return true;
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