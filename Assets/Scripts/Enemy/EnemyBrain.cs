using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 몬스터가 어떤 대상을 향해 이동하고 공격할지 판단하는 컴포넌트입니다.
// 실제 이동은 EnemyMovement, 플레이어 감지는 EnemyDetector, 피해 적용은 EnemyAttack이 담당합니다.
public sealed class EnemyBrain : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string BuildTag = "Build";
    private const string CinderHeartTag = "CinderHeart";
    private const string TowerTag = "Tower";
    private const float DecisionInterval = 0.2f;
    private const int MaxTowerOverlapCount = 20;

    [Tooltip("플레이어 감지 결과를 가져오는 컴포넌트입니다.")]
    [SerializeField] private EnemyDetector _enemyDetector;
    [Tooltip("실제 피해 적용과 공격 쿨타임을 담당하는 컴포넌트입니다.")]
    [SerializeField] private EnemyAttack _enemyAttack;
    [Tooltip("실제 이동 실행을 담당하는 컴포넌트입니다.")]
    [SerializeField] private EnemyMovement _enemyMovement;
    [Tooltip("플레이어를 감지하지 못했을 때 향하고 공격할 CinderHeart 피해 대상입니다.")]
    [SerializeField] private Damageable _cinderHeartDamageable;
    [Tooltip("플레이어와 구조물을 공격할 수 있는 거리입니다.")]
    [SerializeField] private float _attackDistance = 2.3f;
    [Tooltip("CinderHeart를 공격할 수 있는 거리입니다.")]
    [SerializeField] private float _cinderHeartAttackDistance = 3f;
    [Tooltip("CinderHeart 경로가 막혔을 때 앞쪽 건축물을 찾는 거리입니다.")]
    [SerializeField] private float _blockingBuildingDetectDistance = 5f;
    [Tooltip("CinderHeart 경로가 막혔을 때 앞쪽 건축물을 찾는 감지 반경입니다.")]
    [SerializeField] private float _blockingBuildingDetectRadius = 1f;
    [Tooltip("Tower를 공격 대상으로 찾는 거리입니다.")]
    [SerializeField] private float _towerDetectDistance = 5f;

    private readonly NavMeshPath _cinderHeartPath = new NavMeshPath();
    private readonly Collider[] _towerOverlapColliders = new Collider[MaxTowerOverlapCount];

    private Coroutine _brainDecisionRoutine;
    private Damageable _currentAttackTarget;
    private BuildingHp _currentBuildingAttackTarget;
    private bool _canChaseCinderHeart = true;

    private void Awake()
    {
        ConnectComponents();
    }

    private void OnEnable()
    {
        StartBrainRoutine();
    }

    private void OnDisable()
    {
        StopBrainRoutine();
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

    // 플레이어, CinderHeart처럼 Damageable을 기준으로 공격할 대상을 외부에서 지정할 때 사용합니다.
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
        // 적 판단은 매 프레임이 아니라 짧은 주기로 갱신해 과한 연산을 피합니다.
        // 우선순위는 플레이어 감지 -> 막고 있는 건축물 -> CinderHeart -> 낮 배회 순서입니다.
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
        if (TrySetCinderHeartTarget())
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


        ClearCinderHeartAttackTarget();
        ClearPlayerAttackTarget();
        ClearCurrentBuildingAttackTarget();
        ClearTowerAttackTarget();
    }

    private bool TrySetCinderHeartTarget()
    {
        if (_canChaseCinderHeart == false || _cinderHeartDamageable == null)
        {
            ClearCinderHeartAttackTarget();
            return false;
        }

        _currentBuildingAttackTarget = null;
        _currentAttackTarget = _cinderHeartDamageable;
        return true;
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
            _towerOverlapColliders);

        Damageable nearestTowerDamageable = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Damageable towerDamageable = GetTowerDamageableFromCollider(_towerOverlapColliders[i]);
            if (towerDamageable == null)
            {
                continue;
            }

            float distanceSqr = (towerDamageable.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearestDistanceSqr = distanceSqr;
            nearestTowerDamageable = towerDamageable;
        }

        return nearestTowerDamageable;
    }

    private Damageable GetTowerDamageableFromCollider(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        Damageable damageable = hitCollider.GetComponentInParent<Damageable>();
        if (damageable == null)
        {
            return null;
        }

        if (hitCollider.CompareTag(TowerTag))
        {
            return damageable;
        }

        if (damageable.CompareTag(TowerTag))
        {
            return damageable;
        }

        return null;
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

        if (targetObject.CompareTag(CinderHeartTag))
        {
            return IsInAttackDistance(targetObject.transform, _cinderHeartAttackDistance);
        }

        if (targetObject.CompareTag(PlayerTag))
        {
            return IsInAttackDistance(targetObject.transform, _attackDistance);
        }

        if (targetObject.CompareTag(BuildTag))
        {
            return IsInAttackDistance(targetObject.transform, _attackDistance);
        }

        if (targetObject.CompareTag(TowerTag))
        {
            return IsInAttackDistance(targetObject.transform, _attackDistance);
        }

        return false;
    }


    private bool IsInAttackDistance(Transform targetTransform, float attackDistance)
    {
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        return distance <= attackDistance;
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

    // CinderHeart로 향하는 앞쪽 경로에서 Build 태그를 가진 건축물을 찾습니다.
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

    if (_currentAttackTarget.CompareTag(TowerTag))
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
