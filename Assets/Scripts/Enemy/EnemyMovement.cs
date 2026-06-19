using Cinderkeep.Gameplay;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyMovement : MonoBehaviour
{
    [Tooltip("플레이어 감지 결과를 제공하는 컴포넌트입니다.")]
    [SerializeField] private EnemyDetector _enemyDetector;
    [Tooltip("플레이어를 못 봤을 때 몬스터가 향하는 CinderHeart Transform입니다.")]
    [SerializeField] private Transform _cinderHeartTarget;
    [Tooltip("몬스터 이동 속도입니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _moveSpeed;
    [Tooltip("목표와 이 거리 안에 들어오면 이동을 멈춥니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _stopDistance;

    private const float PathUpdateInterval = 0.2f;

    private NavMeshAgent _navMeshAgent;
    private Coroutine _movementRoutine;
    private bool _isInitialized;

    private void OnEnable()
    {
        StartMovementRoutine();
    }

    private void OnDisable()
    {
        StopMovementRoutine();
    }

    public void Initialize(EnemyData enemyData)
    {
        Initialize(enemyData, _enemyDetector);
    }

    public void Initialize(EnemyData enemyData, EnemyDetector enemyDetector)
    {
        if (enemyData == null)
        {
            return;
        }

        ConnectComponents(enemyDetector);
        _moveSpeed = enemyData.MoveSpeed;
        _stopDistance = enemyData.StopDistance;
        ApplyNavMeshAgentSettings();

        _isInitialized = true;
        StartMovementRoutine();
    }

    public void SetCinderHeartTarget(Transform cinderHeartTarget)
    {
        _cinderHeartTarget = cinderHeartTarget;
    }

    public void MoveToTarget(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            StopMoving();
            return;
        }

        if (CanStopAtTarget(targetTransform))
        {
            StopMoving();
            return;
        }

        MoveWithNavMeshOrTransform(targetTransform);
    }

    private void ConnectComponents(EnemyDetector enemyDetector)
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (enemyDetector != null)
        {
            _enemyDetector = enemyDetector;
        }

        if (_enemyDetector == null)
        {
            _enemyDetector = GetComponent<EnemyDetector>();
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

    private void StartMovementRoutine()
    {
        if (_isInitialized == false)
        {
            return;
        }

        StopMovementRoutine();
        _movementRoutine = StartCoroutine(MoveToPriorityTargetRoutine());
    }

    private void StopMovementRoutine()
    {
        if (_movementRoutine == null)
        {
            return;
        }

        StopCoroutine(_movementRoutine);
        _movementRoutine = null;
    }

    private IEnumerator MoveToPriorityTargetRoutine()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(PathUpdateInterval);

        while (true)
        {
            MoveToPriorityTarget();
            yield return waitInterval;
        }
    }

    private void MoveToPriorityTarget()
    {
        Transform targetTransform = GetPriorityTarget();
        MoveToTarget(targetTransform);
    }

    private Transform GetPriorityTarget()
    {
        if (_enemyDetector != null && _enemyDetector.HasDetectedPlayer)
        {
            return _enemyDetector.DetectedPlayer;
        }

        return _cinderHeartTarget;
    }

    private bool CanStopAtTarget(Transform targetTransform)
    {
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        return distance <= _stopDistance;
    }

    private void MoveWithNavMeshOrTransform(Transform targetTransform)
    {
        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = false;
            if (_navMeshAgent.SetDestination(targetTransform.position))
            {
                RotateToTarget(targetTransform.position);
                return;
            }
        }

        MoveDirectlyToTarget(targetTransform.position);
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

    private void StopMoving()
    {
        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh && _navMeshAgent.hasPath)
        {
            _navMeshAgent.ResetPath();
        }
    }
}
