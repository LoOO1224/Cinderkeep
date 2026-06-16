using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepEnemyMovement : MonoBehaviour
    {
        [SerializeField] private Transform Transform_Target;
        [SerializeField] private float _moveSpeed = 2.2f;
        [SerializeField] private float _stopDistance = 1.4f;

        public Transform Target => Transform_Target;

        private void Update()
        {
            MoveToTarget();
        }

        public void SetTarget(Transform target)
        {
            Transform_Target = target;
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            _moveSpeed = Mathf.Max(0.1f, moveSpeed);
        }

        public void SetStopDistance(float stopDistance)
        {
            _stopDistance = Mathf.Max(0.2f, stopDistance);
        }

        public bool HasReachedTarget()
        {
            if (Transform_Target == null)
            {
                return false;
            }

            Vector3 toTarget = Transform_Target.position - transform.position;
            toTarget.y = 0f;
            return toTarget.magnitude <= _stopDistance;
        }

        private void MoveToTarget()
        {
            if (Transform_Target == null)
            {
                return;
            }

            Vector3 toTarget = Transform_Target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude <= _stopDistance)
            {
                return;
            }

            Vector3 direction = toTarget.normalized;
            transform.position += direction * (_moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
