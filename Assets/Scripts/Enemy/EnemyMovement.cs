using Cinderkeep.Gameplay;
using UnityEngine;
using UnityEngine.AI;

// EnemyBrain이 정한 목표까지 실제 이동만 담당합니다.
// 목표 주변 도착 지점을 분산해 CinderHeart 앞에서 여러 적이 한 점에 뭉치는 현상을 줄입니다.
// 몬스터의 실제 이동 실행만 담당하는 컴포넌트입니다.
// 누구를 따라갈지, 무엇을 공격할지는 EnemyBrain이 판단하고 이 클래스는 받은 위치로만 이동합니다.
public sealed class EnemyMovement : MonoBehaviour
{
    private const float MinimumAvoidanceRadius = 0.45f;
    private const int MinimumAvoidancePriority = 25;
    private const int MaximumAvoidancePriority = 75;
    private const float GoldenAngleDegrees = 137.50776f;

    [Tooltip("몬스터 이동 속도입니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _moveSpeed;
    [Tooltip("목표와 이 거리 안에 들어오면 이동을 멈춥니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _stopDistance;
    [Tooltip("낮에 CinderHeart를 향하지 않을 때 스폰 지점 주변을 배회하는 반경입니다.")]
    [SerializeField] private float _wanderRadius = 6f;
    [Tooltip("낮 배회 목표를 다시 정하는 간격입니다.")]
    [SerializeField] private float _wanderInterval = 2.5f;
    [Header("Crowd Separation")]
    [Tooltip("적끼리 너무 겹치지 않도록 주변 적을 확인하는 반경입니다.")]
    [SerializeField] private float _separationRadius = 1.05f;
    [Tooltip("한 프레임에 적용하는 밀어내기 강도입니다.")]
    [SerializeField] private float _separationStrength = 0.65f;
    [Tooltip("같은 목표를 향한 적들이 한 점에 쌓이지 않도록 도착 지점을 나누는 반경입니다.")]
    [SerializeField] private float _arrivalSpreadRadius = 1.35f;

    private NavMeshAgent _navMeshAgent;
    private static readonly Collider[] SeparationHits = new Collider[16];
    private Vector3 _spawnPosition;
    private Vector3 _wanderTargetPosition;
    private Vector3 _targetSpreadOffset;
    private float _nextWanderTime;
    private float _speedMultiplier = 1f;
    private float _slowUntilTime;
    private Transform _lastSpreadTarget;
    private bool _isInitialized;

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        ConnectComponents();
        _moveSpeed = enemyData.MoveSpeed;
        _stopDistance = enemyData.StopDistance;
        _spawnPosition = transform.position;
        _wanderTargetPosition = _spawnPosition;
        _speedMultiplier = 1f;
        _slowUntilTime = 0f;
        ApplyNavMeshAgentSettings();
        _isInitialized = true;
    }

    public void Initialize(EnemyData enemyData, EnemyDetector enemyDetector)
    {
        // EnemyDetector는 기존 초기화 호출 호환성을 위해 인자로만 받습니다.
        // 이동 대상 판단은 EnemyBrain이 담당하므로 이 클래스에서는 사용하지 않습니다.
        Initialize(enemyData);
    }

    public void Initialize(BossData bossData)
    {
        if (bossData == null)
        {
            return;
        }

        ConnectComponents();
        _moveSpeed = bossData.MoveSpeed;
        _stopDistance = bossData.StopDistance;
        _spawnPosition = transform.position;
        _wanderTargetPosition = _spawnPosition;
        _speedMultiplier = 1f;
        _slowUntilTime = 0f;
        ApplyNavMeshAgentSettings();
        _isInitialized = true;
    }

    public void ApplyMoveSpeedMultiplier(float speedMultiplier, float durationSeconds)
    {
        if (durationSeconds <= 0f)
        {
            return;
        }

        _speedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f);
        _slowUntilTime = Mathf.Max(_slowUntilTime, Time.time + durationSeconds);
        ApplyNavMeshAgentSettings();
    }

    public void Initialize(BossData bossData, EnemyDetector enemyDetector)
    {
        Initialize(bossData);
    }

    public void MoveToTarget(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            StopMoving();
            return;
        }

        if (_isInitialized == false)
        {
            return;
        }

        if (CanStopAtPosition(targetTransform.position))
        {
            StopMoving();
            ApplyCrowdSeparation();
            return;
        }

        MoveWithNavMeshOrTransform(GetMoveTargetPosition(targetTransform));
        ApplyCrowdSeparation();
    }

    public void MoveToPosition(Vector3 targetPosition)
    {
        if (_isInitialized == false)
        {
            return;
        }

        if (CanStopAtPosition(targetPosition))
        {
            StopMoving();
            ApplyCrowdSeparation();
            return;
        }

        MoveWithNavMeshOrTransform(targetPosition);
        ApplyCrowdSeparation();
    }

    public void WanderAroundSpawnPoint()
    {
        if (_isInitialized == false)
        {
            return;
        }

        if (Time.time >= _nextWanderTime)
        {
            RefreshWanderTargetPosition();
        }

        MoveToPosition(_wanderTargetPosition);
    }

    public void StopMoving()
    {
        if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh && _navMeshAgent.hasPath)
        {
            _navMeshAgent.ResetPath();
        }
    }

    private void ConnectComponents()
    {
        if (_navMeshAgent == null)
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    private void ApplyNavMeshAgentSettings()
    {
        if (_navMeshAgent == null)
        {
            return;
        }

        _navMeshAgent.speed = GetCurrentMoveSpeed();
        _navMeshAgent.stoppingDistance = _stopDistance;
        _navMeshAgent.radius = Mathf.Max(_navMeshAgent.radius, MinimumAvoidanceRadius);
        _navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        _navMeshAgent.avoidancePriority = Random.Range(MinimumAvoidancePriority, MaximumAvoidancePriority + 1);
    }

    private void RefreshWanderTargetPosition()
    {
        _nextWanderTime = Time.time + _wanderInterval;
        Vector2 randomCircle = Random.insideUnitCircle * _wanderRadius;
        _wanderTargetPosition = _spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        _wanderTargetPosition.y = transform.position.y;
    }

    private Vector3 GetSpreadTargetPosition(Transform targetTransform)
    {
        if (targetTransform == null || _arrivalSpreadRadius <= 0f)
        {
            return targetTransform == null ? transform.position : targetTransform.position;
        }

        if (_lastSpreadTarget != targetTransform)
        {
            _lastSpreadTarget = targetTransform;
            _targetSpreadOffset = CreateStableSpreadOffset(targetTransform);
        }

        Vector3 targetPosition = targetTransform.position + _targetSpreadOffset;
        targetPosition.y = targetTransform.position.y;

        if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(targetPosition, out navMeshHit, _arrivalSpreadRadius + 0.5f, NavMesh.AllAreas))
            {
                return navMeshHit.position;
            }
        }

        return targetPosition;
    }

    private Vector3 GetMoveTargetPosition(Transform targetTransform)
    {
        if (IsCinderHeartTarget(targetTransform))
        {
            return targetTransform.position;
        }

        return GetSpreadTargetPosition(targetTransform);
    }

    private bool IsCinderHeartTarget(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return false;
        }

        if (targetTransform.GetComponent<CinderHeart>() != null)
        {
            return true;
        }

        return targetTransform.GetComponentInParent<CinderHeart>() != null;
    }

    private Vector3 CreateStableSpreadOffset(Transform targetTransform)
    {
        float spreadRadius = Mathf.Min(_arrivalSpreadRadius, Mathf.Max(0.25f, _stopDistance * 0.2f));
        if (spreadRadius <= 0.05f)
        {
            return Vector3.zero;
        }

        int rawHash = GetInstanceID() ^ targetTransform.GetInstanceID();
        int hash = rawHash == int.MinValue ? int.MaxValue : Mathf.Abs(rawHash);
        float angle = (hash % 360 + (hash % 7) * GoldenAngleDegrees) * Mathf.Deg2Rad;
        float ringRatio = 0.45f + (hash % 3) * 0.25f;
        float radius = spreadRadius * Mathf.Clamp01(ringRatio);

        return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
    }

    private bool CanStopAtPosition(Vector3 targetPosition)
    {
        targetPosition.y = transform.position.y;
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance <= _stopDistance;
    }

    private void MoveWithNavMeshOrTransform(Vector3 targetPosition)
    {
        if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.speed = GetCurrentMoveSpeed();
            if (_navMeshAgent.SetDestination(targetPosition))
            {
                RotateToTarget(targetPosition);
                return;
            }
        }

        DisableInvalidNavMeshAgent();
        MoveDirectlyToTarget(targetPosition);
    }

    private void MoveDirectlyToTarget(Vector3 targetPosition)
    {
        targetPosition.y = transform.position.y;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, GetCurrentMoveSpeed() * Time.deltaTime);
        transform.position = nextPosition;
        RotateToTarget(targetPosition);
    }

    private void ApplyCrowdSeparation()
    {
        if (_separationRadius <= 0f || _separationStrength <= 0f)
        {
            return;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _separationRadius, SeparationHits);
        if (hitCount <= 1)
        {
            return;
        }

        Vector3 pushDirection = Vector3.zero;
        int neighborCount = 0;
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = SeparationHits[i];
            if (hitCollider == null)
            {
                continue;
            }

            EnemyMovement otherMovement = hitCollider.GetComponentInParent<EnemyMovement>();
            if (otherMovement == null || otherMovement == this)
            {
                continue;
            }

            Vector3 away = transform.position - otherMovement.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude <= 0.0001f)
            {
                away = UnityEngine.Random.insideUnitSphere;
                away.y = 0f;
            }

            float distance = Mathf.Max(away.magnitude, 0.1f);
            pushDirection += away.normalized / distance;
            neighborCount++;
        }

        if (neighborCount <= 0)
        {
            return;
        }

        Vector3 pushOffset = pushDirection.normalized * (_separationStrength * Time.deltaTime);
        if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.Move(pushOffset);
            return;
        }

        transform.position += pushOffset;
    }

    private float GetCurrentMoveSpeed()
    {
        if (Time.time > _slowUntilTime)
        {
            _speedMultiplier = 1f;
        }

        return Mathf.Max(0f, _moveSpeed * _speedMultiplier);
    }

    private void RotateToTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void DisableInvalidNavMeshAgent()
    {
        if (_navMeshAgent == null || _navMeshAgent.enabled == false)
        {
            return;
        }

        if (_navMeshAgent.isOnNavMesh)
        {
            return;
        }

        _navMeshAgent.enabled = false;
    }
}
