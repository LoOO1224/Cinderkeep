using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepEnemyAttack : MonoBehaviour
    {
        [SerializeField] private CinderkeepEnemyMovement CinderkeepEnemyMovement_Movement;
        [SerializeField] private Transform Transform_Target;
        [SerializeField] private int _damage = 4;
        [SerializeField] private float _attackInterval = 1.2f;

        private float _timer;

        private void Awake()
        {
            if (CinderkeepEnemyMovement_Movement == null)
            {
                CinderkeepEnemyMovement_Movement = GetComponent<CinderkeepEnemyMovement>();
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _attackInterval || CinderkeepEnemyMovement_Movement == null || !CinderkeepEnemyMovement_Movement.HasReachedTarget())
            {
                return;
            }

            _timer = 0f;
            AttackTarget();
        }

        public void SetTarget(Transform target)
        {
            Transform_Target = target;
        }

        public void SetDamage(int damage)
        {
            _damage = Mathf.Max(1, damage);
        }

        public void SetAttackInterval(float attackInterval)
        {
            _attackInterval = Mathf.Max(0.1f, attackInterval);
        }

        private void AttackTarget()
        {
            Transform target = Transform_Target != null ? Transform_Target : CinderkeepEnemyMovement_Movement.Target;
            if (target == null)
            {
                return;
            }

            CinderkeepFlameHeart flameHeart = target.GetComponentInParent<CinderkeepFlameHeart>();
            if (flameHeart != null)
            {
                flameHeart.TakeDamage(_damage);
            }
        }
    }
}
