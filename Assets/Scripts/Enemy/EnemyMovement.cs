using Cinderkeep.Gameplay;
using UnityEngine;
using UnityEngine.AI;

// 몬스터의 실제 이동 실행만 담당하는 컴포넌트입니다.
// 누구를 따라갈지, 무엇을 공격할지는 EnemyBrain이 판단하고 이 클래스는 받은 위치로만 이동합니다.
public sealed class EnemyMovement : MonoBehaviour
{
    [Tooltip("몬스터 이동 속도입니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _moveSpeed;
    [Tooltip("목표와 이 거리 안에 들어오면 이동을 멈춥니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _stopDistance;
    [Tooltip("낮에 CinderHeart를 향하지 않을 때 스폰 지점 주변을 배회하는 반경입니다.")]
    [SerializeField] private float _wanderRadius = 6f;
    [Tooltip("낮 배회 목표를 다시 정하는 간격입니다.")]
    [SerializeField] private float _wanderInterval = 2.5f;

    private NavMeshAgent _navMeshAgent;
    private Vector3 _spawnPosition;
    private Vector3 _wanderTargetPosition;
    private float _nextWanderTime;
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
        ApplyNavMeshAgentSettings();
        _isInitialized = true;
    }

    public void Initialize(EnemyData enemyData, EnemyDetector enemyDetector)
    {
        // EnemyDetector는 기존 초기화 호출 호환성을 위해 인자로만 받습니다.
        // 이동 대상 판단은 EnemyBrain이 담당하므로 이 클래스에서는 사용하지 않습니다.
        Initialize(enemyData);
    }

    public void MoveToTarget(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            StopMoving();
            return;
        }

        MoveToPosition(targetTransform.position);
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
            return;
        }

        MoveWithNavMeshOrTransform(targetPosition);
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
        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh && _navMeshAgent.hasPath)
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

        _navMeshAgent.speed = _moveSpeed;
        _navMeshAgent.stoppingDistance = _stopDistance;
    }

    private void RefreshWanderTargetPosition()
    {
        _nextWanderTime = Time.time + _wanderInterval;
        Vector2 randomCircle = Random.insideUnitCircle * _wanderRadius;
        _wanderTargetPosition = _spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        _wanderTargetPosition.y = transform.position.y;
    }

    private bool CanStopAtPosition(Vector3 targetPosition)
    {
        targetPosition.y = transform.position.y;
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance <= _stopDistance;
    }

    private void MoveWithNavMeshOrTransform(Vector3 targetPosition)
    {
        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = false;
            if (_navMeshAgent.SetDestination(targetPosition))
            {
                RotateToTarget(targetPosition);
                return;
            }
        }

        MoveDirectlyToTarget(targetPosition);
    }

    private void MoveDirectlyToTarget(Vector3 targetPosition)
    {
        targetPosition.y = transform.position.y;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
        transform.position = nextPosition;
        RotateToTarget(targetPosition);
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
}
