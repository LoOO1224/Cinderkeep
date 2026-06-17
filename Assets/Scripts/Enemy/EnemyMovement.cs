using UnityEngine;
using Cinderkeep.Gameplay;

public sealed class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _stopDistance;

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        _moveSpeed = enemyData.MoveSpeed;
        _stopDistance = enemyData.StopDistance;
    }

    public void MoveToTarget(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, targetTransform.position);
        if (distance <= _stopDistance)
        {
            return;
        }

        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetTransform.position, _moveSpeed * Time.deltaTime);
        transform.position = nextPosition;
    }
}
