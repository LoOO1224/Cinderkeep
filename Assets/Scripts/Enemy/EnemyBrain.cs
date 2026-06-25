using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 적의 현재 타깃을 정하고 이동/공격 컴포넌트에 실행을 요청하는 AI 흐름 관리자입니다.
// 감지, 이동, 공격, 경로 계산은 전용 컴포넌트/helper가 담당하고 Brain은 우선순위 판단만 유지합니다.
public sealed class EnemyBrain : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string BuildTag = "Build";
    private const string CinderHeartTag = "CinderHeart";
    private const float DecisionInterval = 0.2f;
    private const int MaxTowerOverlapCount = 20;

    [Tooltip("플레이어 감지와 야간 감지 모드를 담당하는 컴포넌트입니다.")]
    [SerializeField] private EnemyDetector _enemyDetector;
    [Tooltip("현재 선택된 타깃에게 피해를 적용하는 공격 컴포넌트입니다.")]
    [SerializeField] private EnemyAttack _enemyAttack;
    [Tooltip("현재 타깃 위치로 이동하거나 멈추는 이동 컴포넌트입니다.")]
    [SerializeField] private EnemyMovement _enemyMovement;
    [Tooltip("밤과 보스 페이즈에서 최종 목표로 삼는 CinderHeart Damageable입니다.")]
    [SerializeField] private Damageable _cinderHeartDamageable;
    [Tooltip("플레이어와 건축물을 공격할 수 있는 거리입니다.")]
    [SerializeField] private float _attackDistance = 2.3f;
    [Tooltip("CinderHeart를 공격할 수 있는 거리입니다.")]
    [SerializeField] private float _cinderHeartAttackDistance = 3f;
    [Tooltip("CinderHeart 경로를 막는 건축물을 찾을 거리입니다.")]
    [SerializeField] private float _blockingBuildingDetectDistance = 5f;
    [Tooltip("막고 있는 건축물을 찾을 때 사용하는 감지 반경입니다.")]
    [SerializeField] private float _blockingBuildingDetectRadius = 1f;
    [Tooltip("밤에 주변 타워를 보복 타깃으로 찾을 거리입니다.")]
    [SerializeField] private float _towerDetectDistance = 5f;
    [Tooltip("최근에 자신을 공격한 플레이어/타워를 기억하는 시간입니다.")]
    [SerializeField] private float _attackerMemoryDuration = 7f;
    [Tooltip("true이면 밤 우선순위: 공격자 -> 플레이어 -> 길막 건축물 -> CinderHeart 순서를 사용합니다.")]
    [SerializeField] private bool _isNightTime;
    [Tooltip("true이면 감지된 플레이어를 CinderHeart보다 우선 추적합니다. 보스는 피격 어그로만 쓰기 위해 false로 둡니다.")]
    [SerializeField] private bool _usePlayerDetectionAsPriority = true;
    [Tooltip("true이면 근처 타워를 CinderHeart보다 우선 추적합니다. 보스는 기본 목표를 유지하기 위해 false로 둡니다.")]
    [SerializeField] private bool _useTowerDetectionAsPriority = true;

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
    private float _nextDecisionTime;

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

    private void Update()
    {
        if (Time.time >= _nextDecisionTime)
        {
            RefreshBrainTargets();
        }

        MoveByCurrentTarget();
        TryAttackCurrentTarget();
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
            RefreshBrainTargets();

            yield return waitInterval;
        }
    }

    public void SetPlayerDetectionPriorityEnabled(bool isEnabled)
    {
        _usePlayerDetectionAsPriority = isEnabled;

        if (isEnabled)
        {
            return;
        }

        ClearPlayerAttackTarget();
    }

    public void SetTowerDetectionPriorityEnabled(bool isEnabled)
    {
        _useTowerDetectionAsPriority = isEnabled;

        if (isEnabled)
        {
            return;
        }

        ClearTowerAttackTarget();
    }

    private void RefreshBrainTargets()
    {
        RefreshTargets();
        _nextDecisionTime = Time.time + DecisionInterval;
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

        if (_usePlayerDetectionAsPriority && TrySetPlayerTargetFromDetector())
        {
            return;
        }

        if (_usePlayerDetectionAsPriority == false)
        {
            ClearPlayerAttackTarget();
        }

        if (TrySetBuildingTargetFromBlockedPath())
        {
            return;
        }

        if (_useTowerDetectionAsPriority && TrySetTowerTarget())
        {
            return;
        }

        if (_useTowerDetectionAsPriority == false)
        {
            ClearTowerAttackTarget();
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

        if (EnemyPathQuery.CanReachTarget(transform, attackerDamageable.transform, _attackerPath))
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

        if (EnemyPathQuery.IsPathBlocked(transform, _cinderHeartDamageable.transform, _cinderHeartPath) == false)
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

        if (IsCinderHeartTarget(targetObject))
        {
            return IsInAttackDistance(targetObject.transform, _cinderHeartAttackDistance);
        }

        return false;
    }

    private bool IsCinderHeartTarget(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return false;
        }

        if (targetObject.CompareTag(CinderHeartTag))
        {
            return true;
        }

        if (targetObject.GetComponent<CinderHeart>() != null)
        {
            return true;
        }

        return targetObject.GetComponentInParent<CinderHeart>() != null;
    }

    private bool IsInAttackDistance(Transform targetTransform, float attackDistance)
    {
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        return distance <= attackDistance;
    }

    private BuildingHp FindBlockingBuilding(Vector3 targetPosition)
    {
        return EnemyPathQuery.FindBlockingBuilding(
            transform.position,
            targetPosition,
            _blockingBuildingDetectRadius,
            _blockingBuildingDetectDistance,
            BuildTag);
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
