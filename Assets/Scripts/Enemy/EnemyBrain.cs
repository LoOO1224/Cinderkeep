using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 적의 현재 타깃을 정하고 이동/공격 컴포넌트에 실행을 요청하는 AI 흐름 관리자입니다.
// 감지, 이동, 공격 자체는 EnemyDetector, EnemyMovement, EnemyAttack이 담당합니다.
public sealed class EnemyBrain : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string BuildTag = "Build";
    private const string CinderHeartTag = "CinderHeart";
    private const float DecisionInterval = 0.2f;
    private const int MaxTowerOverlapCount = 20;

    [Tooltip("Player detection component.")]
    [SerializeField] private EnemyDetector _enemyDetector;
    [Tooltip("Enemy attack executor.")]
    [SerializeField] private EnemyAttack _enemyAttack;
    [Tooltip("Enemy movement executor.")]
    [SerializeField] private EnemyMovement _enemyMovement;
    [Tooltip("Fallback CinderHeart attack target.")]
    [SerializeField] private Damageable _cinderHeartDamageable;
    [Tooltip("Attack distance for players and buildings.")]
    [SerializeField] private float _attackDistance = 2.3f;
    [Tooltip("Attack distance for CinderHeart.")]
    [SerializeField] private float _cinderHeartAttackDistance = 3f;
    [Tooltip("Distance used to find a building blocking the current path.")]
    [SerializeField] private float _blockingBuildingDetectDistance = 5f;
    [Tooltip("Radius used to find a building blocking the current path.")]
    [SerializeField] private float _blockingBuildingDetectRadius = 1f;
    [Tooltip("Night-only distance for finding nearby towers.")]
    [SerializeField] private float _towerDetectDistance = 5f;
    [Tooltip("Seconds to remember the most recent attacker.")]
    [SerializeField] private float _attackerMemoryDuration = 7f;
    [Tooltip("Whether the enemy should use night detection and target priority.")]
    [SerializeField] private bool _isNightTime;

    private readonly Collider[] _towerOverlapColliders = new Collider[MaxTowerOverlapCount];

    private NavMeshPath _cinderHeartPath;
    private NavMeshPath _attackerPath;
    private Coroutine _brainDecisionRoutine;
    private Damageable _currentAttackTarget;
    private BuildingHp _currentBuildingAttackTarget;
    private bool _canChaseCinderHeart = true;
    private Damageable _recentPlayerAttacker;
    private Damageable _recentTowerAttacker;
    private float _lastPlayerAttackedTime;
    private float _lastTowerAttackedTime;

    private void Awake()
    {
        _cinderHeartPath = new NavMeshPath();
        _attackerPath = new NavMeshPath();

        ConnectComponents();
        ApplyDetectorMode();
    }

    private void OnEnable()
    {
        StartBrainRoutine();
    }

    private void OnDisable()
    {
        StopBrainRoutine();
    }

    public void SetNightTime(bool isNightTime)
    {
        _isNightTime = isNightTime;
        ApplyDetectorMode();
    }

    private void ApplyDetectorMode()
    {
        if (_enemyDetector == null)
        {
            return;
        }

        _enemyDetector.SetNightDetectionEnabled(_isNightTime);
    }

    public void SetCinderHeartTarget(Damageable cinderHeartDamageable)
    {
        _cinderHeartDamageable = cinderHeartDamageable;
    }

    public void SetCinderHeartChaseEnabled(bool isEnabled)
    {
        _canChaseCinderHeart = isEnabled;

        if (isEnabled == true)
        {
            return;
        }

        ClearCurrentBuildingAttackTarget();
        ClearCinderHeartAttackTarget();

        if (_enemyMovement != null)
        {
            _enemyMovement.StopMoving();
        }
    }

    public void ReportAttacker(Damageable attackerDamageable)
    {
        if (attackerDamageable == null)
        {
            return;
        }

        if (attackerDamageable.CompareTag(PlayerTag))
        {
            _recentPlayerAttacker = attackerDamageable;
            _lastPlayerAttackedTime = Time.time;
            return;
        }

        if (attackerDamageable.GetComponentInParent<BuildingTower>() != null)
        {
            _recentTowerAttacker = attackerDamageable;
            _lastTowerAttackedTime = Time.time;
        }
    }

    public void SetAttackTarget(Damageable targetDamageable)
    {
        _currentAttackTarget = targetDamageable;
        _currentBuildingAttackTarget = null;
    }

    public void ClearAttackTarget(Damageable targetDamageable)
    {
        if (_currentAttackTarget != targetDamageable)
        {
            return;
        }

        _currentAttackTarget = null;
    }

    public void ClearBuildingAttackTarget(BuildingHp buildingHp)
    {
        if (_currentBuildingAttackTarget != buildingHp)
        {
            return;
        }

        _currentBuildingAttackTarget = null;
    }

    private void ConnectComponents()
    {
        if (_enemyDetector == null)
        {
            _enemyDetector = GetComponent<EnemyDetector>();
        }

        if (_enemyAttack == null)
        {
            _enemyAttack = GetComponent<EnemyAttack>();
        }

        if (_enemyMovement == null)
        {
            _enemyMovement = GetComponent<EnemyMovement>();
        }
    }

    private void StartBrainRoutine()
    {
        StopBrainRoutine();
        _brainDecisionRoutine = StartCoroutine(BrainDecisionRoutine());
    }

    private void StopBrainRoutine()
    {
        if (_brainDecisionRoutine == null)
        {
            return;
        }

        StopCoroutine(_brainDecisionRoutine);
        _brainDecisionRoutine = null;
    }

    private IEnumerator BrainDecisionRoutine()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(DecisionInterval);

        while (true)
        {
            RefreshTargets();
            MoveByCurrentTarget();
            TryAttackCurrentTarget();

            yield return waitInterval;
        }
    }

    private void RefreshTargets()
    {
        if (_isNightTime)
        {
            RefreshNightTargets();
            return;
        }

        RefreshDayTargets();
    }

    private void RefreshNightTargets()
    {
        if (TrySetRecentAttackerTarget())
        {
            return;
        }

        if (TrySetPlayerTargetFromDetector())
        {
            return;
        }

        if (TrySetBuildingTargetFromBlockedPath())
        {
            return;
        }

        if (TrySetTowerTarget())
        {
            return;
        }

        TrySetCinderHeartTarget();
    }

    private void RefreshDayTargets()
    {
        if (TrySetPlayerTargetFromDetector())
        {
            return;
        }

        ClearAllTargets();
    }

    private void ClearAllTargets()
    {
        ClearPlayerAttackTarget();
        ClearCurrentBuildingAttackTarget();
        ClearTowerAttackTarget();
        ClearCinderHeartAttackTarget();
    }

    private bool TrySetRecentAttackerTarget()
    {
        Damageable attackerDamageable = GetNearestRecentAttacker();
        if (attackerDamageable == null)
        {
            return false;
        }

        if (CanReachTarget(attackerDamageable.transform))
        {
            _currentBuildingAttackTarget = null;
            _currentAttackTarget = attackerDamageable;
            return true;
        }

        BuildingHp blockingBuildingHp = FindBlockingBuilding(attackerDamageable.transform.position);
        if (blockingBuildingHp == null)
        {
            return false;
        }

        SetBuildingAttackTarget(blockingBuildingHp);
        return true;
    }

    private Damageable GetNearestRecentAttacker()
    {
        return EnemyTargetSelector.SelectNearestRecentAttacker(
            transform,
            _recentPlayerAttacker,
            _lastPlayerAttackedTime,
            _recentTowerAttacker,
            _lastTowerAttackedTime,
            _attackerMemoryDuration);
    }

    private bool TrySetPlayerTargetFromDetector()
    {
        if (_enemyDetector == null || _enemyDetector.HasDetectedPlayer == false)
        {
            ClearPlayerAttackTarget();
            return false;
        }

        Damageable detectedPlayerDamageable = GetDamageableFromTransform(_enemyDetector.DetectedPlayer);
        if (detectedPlayerDamageable == null)
        {
            ClearPlayerAttackTarget();
            return false;
        }

        _currentBuildingAttackTarget = null;
        _currentAttackTarget = detectedPlayerDamageable;
        return true;
    }

    private bool TrySetBuildingTargetFromBlockedPath()
    {
        if (_canChaseCinderHeart == false || _cinderHeartDamageable == null)
        {
            ClearCurrentBuildingAttackTarget();
            return false;
        }

        if (IsCinderHeartPathBlocked() == false)
        {
            ClearCurrentBuildingAttackTarget();
            return false;
        }

        if (_currentBuildingAttackTarget != null && _currentBuildingAttackTarget.IsDestroyed == false)
        {
            _currentAttackTarget = null;
            return true;
        }

        BuildingHp blockingBuildingHp = FindBlockingBuilding(_cinderHeartDamageable.transform.position);
        if (blockingBuildingHp == null)
        {
            ClearCurrentBuildingAttackTarget();
            return false;
        }

        SetBuildingAttackTarget(blockingBuildingHp);
        return true;
    }

    private void SetBuildingAttackTarget(BuildingHp buildingHp)
    {
        if (buildingHp == null || buildingHp.IsDestroyed)
        {
            return;
        }

        _currentBuildingAttackTarget = buildingHp;
        _currentAttackTarget = null;
    }

    private bool TrySetTowerTarget()
    {
        Damageable towerDamageable = FindNearestTowerDamageable();
        if (towerDamageable == null)
        {
            ClearTowerAttackTarget();
            return false;
        }

        _currentBuildingAttackTarget = null;
        _currentAttackTarget = towerDamageable;
        return true;
    }

    private Damageable FindNearestTowerDamageable()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            _towerDetectDistance,
            _towerOverlapColliders,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        return EnemyTargetSelector.SelectNearestDamageable(
            transform.position,
            _towerOverlapColliders,
            hitCount,
            GetTowerDamageableFromCollider);
    }

    private Damageable GetTowerDamageableFromCollider(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        BuildingTower buildingTower = hitCollider.GetComponentInParent<BuildingTower>();
        if (buildingTower == null)
        {
            return null;
        }

        return buildingTower.GetComponent<Damageable>();
    }

    private bool TrySetCinderHeartTarget()
    {
        if (_canChaseCinderHeart == false)
        {
            ClearCinderHeartAttackTarget();
            return false;
        }

        if (_currentBuildingAttackTarget != null)
        {
            return false;
        }

        if (_cinderHeartDamageable == null)
        {
            return false;
        }

        _currentAttackTarget = _cinderHeartDamageable;
        return true;
    }

    private void MoveByCurrentTarget()
    {
        if (_enemyMovement == null)
        {
            return;
        }

        if (_currentBuildingAttackTarget != null)
        {
            _enemyMovement.MoveToTarget(_currentBuildingAttackTarget.transform);
            return;
        }

        if (_currentAttackTarget != null)
        {
            _enemyMovement.MoveToTarget(_currentAttackTarget.transform);
            return;
        }

        _enemyMovement.WanderAroundSpawnPoint();
    }

    private void TryAttackCurrentTarget()
    {
        if (_currentBuildingAttackTarget != null)
        {
            TryAttackCurrentBuildingTarget();
            return;
        }

        if (_currentAttackTarget == null)
        {
            return;
        }

        if (CanAttackTarget(_currentAttackTarget.gameObject) == false)
        {
            return;
        }

        if (_enemyAttack == null)
        {
            return;
        }

        _enemyAttack.TryAttack(_currentAttackTarget);
    }

    private void TryAttackCurrentBuildingTarget()
    {
        if (_currentBuildingAttackTarget == null)
        {
            return;
        }

        if (_currentBuildingAttackTarget.IsDestroyed)
        {
            _currentBuildingAttackTarget = null;
            return;
        }

        if (CanAttackTarget(_currentBuildingAttackTarget.gameObject) == false)
        {
            return;
        }

        if (_enemyAttack == null)
        {
            return;
        }

        _enemyAttack.TryAttack(_currentBuildingAttackTarget);
    }

    private bool CanAttackTarget(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return false;
        }

        if (_isNightTime == false && targetObject.CompareTag(PlayerTag) == false)
        {
            return false;
        }

        if (targetObject.CompareTag(PlayerTag))
        {
            return IsInAttackDistance(targetObject.transform, _attackDistance);
        }

        if (targetObject.CompareTag(BuildTag))
        {
            return IsInAttackDistance(targetObject.transform, _attackDistance);
        }

        if (targetObject.CompareTag(CinderHeartTag))
        {
            return IsInAttackDistance(targetObject.transform, _cinderHeartAttackDistance);
        }

        return false;
    }

    private bool IsInAttackDistance(Transform targetTransform, float attackDistance)
    {
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        return distance <= attackDistance;
    }

    private bool CanReachTarget(Transform targetTransform)
    {
        if (targetTransform == null || _attackerPath == null)
        {
            return false;
        }

        NavMeshHit enemyPositionOnNavMesh;
        if (NavMesh.SamplePosition(transform.position, out enemyPositionOnNavMesh, 2f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        NavMeshHit targetPositionOnNavMesh;
        if (NavMesh.SamplePosition(targetTransform.position, out targetPositionOnNavMesh, 4f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        bool hasPath = NavMesh.CalculatePath(
            enemyPositionOnNavMesh.position,
            targetPositionOnNavMesh.position,
            NavMesh.AllAreas,
            _attackerPath);

        if (hasPath == false)
        {
            return false;
        }

        return _attackerPath.status == NavMeshPathStatus.PathComplete;
    }

    private bool IsCinderHeartPathBlocked()
    {
        if (_cinderHeartDamageable == null)
        {
            return false;
        }

        NavMeshHit enemyPositionOnNavMesh;
        if (NavMesh.SamplePosition(transform.position, out enemyPositionOnNavMesh, 2f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        NavMeshHit cinderHeartPositionOnNavMesh;
        if (NavMesh.SamplePosition(_cinderHeartDamageable.transform.position, out cinderHeartPositionOnNavMesh, 4f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        bool hasPath = NavMesh.CalculatePath(
            enemyPositionOnNavMesh.position,
            cinderHeartPositionOnNavMesh.position,
            NavMesh.AllAreas,
            _cinderHeartPath);

        if (hasPath == false)
        {
            return true;
        }

        return _cinderHeartPath.status == NavMeshPathStatus.PathPartial ||
               _cinderHeartPath.status == NavMeshPathStatus.PathInvalid;
    }

    private BuildingHp FindBlockingBuilding(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return null;
        }

        Vector3 origin = transform.position + Vector3.up * 0.8f;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            _blockingBuildingDetectRadius,
            direction.normalized,
            _blockingBuildingDetectDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            BuildingHp buildingHp = GetBuildingHpFromCollider(hits[i].collider);
            if (buildingHp != null)
            {
                return buildingHp;
            }
        }

        return null;
    }

    private BuildingHp GetBuildingHpFromCollider(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        BuildingHp buildingHp = hitCollider.GetComponentInParent<BuildingHp>();
        if (buildingHp == null)
        {
            return null;
        }

        if (buildingHp.IsDestroyed)
        {
            return null;
        }

        if (hitCollider.CompareTag(BuildTag))
        {
            return buildingHp;
        }

        if (buildingHp.CompareTag(BuildTag))
        {
            return buildingHp;
        }

        return null;
    }

    private void ClearPlayerAttackTarget()
    {
        if (_currentAttackTarget == null)
        {
            return;
        }

        if (_currentAttackTarget.CompareTag(PlayerTag))
        {
            _currentAttackTarget = null;
        }
    }

    private void ClearCinderHeartAttackTarget()
    {
        if (_currentAttackTarget != _cinderHeartDamageable)
        {
            return;
        }

        _currentAttackTarget = null;
    }

    private void ClearCurrentBuildingAttackTarget()
    {
        _currentBuildingAttackTarget = null;
    }

    private void ClearTowerAttackTarget()
    {
        if (_currentAttackTarget == null)
        {
            return;
        }

        if (_currentAttackTarget.GetComponentInParent<BuildingTower>() != null)
        {
            _currentAttackTarget = null;
        }
    }

    private Damageable GetDamageableFromTransform(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return null;
        }

        Damageable damageable = targetTransform.GetComponent<Damageable>();
        if (damageable != null)
        {
            return damageable;
        }

        return targetTransform.GetComponentInParent<Damageable>();
    }
}
