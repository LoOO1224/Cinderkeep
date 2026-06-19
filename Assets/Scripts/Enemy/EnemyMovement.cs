using Cinderkeep.Gameplay;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyMovement : MonoBehaviour
{
    [Tooltip("몬스터 이동 속도입니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _moveSpeed;
    [Tooltip("목표와 이 거리 안에 들어오면 이동을 멈춥니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _stopDistance;

    private NavMeshAgent _navMeshAgent;
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
        ApplyNavMeshAgentSettings();

        _isInitialized = true;
    }

    public void MoveToTarget(Transform targetTransform)
    {
        if(_isInitialized == false)
        {
            return;
        }
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

    private void ConnectComponents()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
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

    private bool CanStopAtTarget(Transform targetTransform)
    {
        float distance = Vector3.Distance(transform.position, targetTransform.position);

        if (distance <= _stopDistance)
        {
            return true; 
        }
        else
        {
            return false; 
        }
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
