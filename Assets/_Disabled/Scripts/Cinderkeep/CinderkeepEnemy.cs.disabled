using UnityEngine;

namespace OODong.Cinderkeep
{
    [RequireComponent(typeof(CinderkeepEnemyMovement))]
    [RequireComponent(typeof(CinderkeepEnemyAttack))]
    // Enemy 본체는 체력/사망 보고만 가진다.
    // 이동은 CinderkeepEnemyMovement, 공격은 CinderkeepEnemyAttack이 담당해서 actor 역할을 분리한다.
    public sealed class CinderkeepEnemy : MonoBehaviour
    {
        [SerializeField] private CinderkeepEnemyMovement CinderkeepEnemyMovement_Movement;
        [SerializeField] private CinderkeepEnemyAttack CinderkeepEnemyAttack_Attack;
        [SerializeField] private string _enemyDataId = "plant";
        [SerializeField] private int _health = 1;
        [SerializeField] private bool _isBoss;

        public bool IsAlive => _health > 0 && gameObject.activeInHierarchy;
        public bool IsBoss => _isBoss;

        private void Awake()
        {
            ResolveComponents();
        }

        public void SetTarget(Transform target)
        {
            ResolveComponents();
            CinderkeepEnemyMovement_Movement.SetTarget(target);
            CinderkeepEnemyAttack_Attack.SetTarget(target);
        }

        public void Configure(CinderkeepEnemyData enemyData, bool isBoss)
        {
            // JSON Static Data를 받아 런타임 적 스펙에 반영한다.
            // TODO(팀원 작업 요청): 애니메이터/피격 이펙트/드랍 테이블도 여기서 직접 처리하지 말고 전용 컴포넌트로 붙여 주세요.
            if (enemyData == null)
            {
                return;
            }

            ResolveComponents();
            _enemyDataId = enemyData.Id;
            _health = enemyData.Health;
            _isBoss = isBoss;
            CinderkeepEnemyMovement_Movement.SetMoveSpeed(enemyData.MoveSpeed);
            CinderkeepEnemyMovement_Movement.SetStopDistance(enemyData.StopDistance);
            CinderkeepEnemyAttack_Attack.SetDamage(enemyData.AttackDamage);
            CinderkeepEnemyAttack_Attack.SetAttackInterval(enemyData.AttackInterval);
            transform.localScale = Vector3.one * enemyData.VisualScale;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || _health <= 0)
            {
                return;
            }

            _health -= damage;
            if (_health <= 0)
            {
                GameManager.Instance?.ReportEnemyDefeated(this);
                Destroy(gameObject);
            }
        }

        private void ResolveComponents()
        {
            if (CinderkeepEnemyMovement_Movement == null)
            {
                CinderkeepEnemyMovement_Movement = GetComponent<CinderkeepEnemyMovement>();
            }

            if (CinderkeepEnemyAttack_Attack == null)
            {
                CinderkeepEnemyAttack_Attack = GetComponent<CinderkeepEnemyAttack>();
            }
        }
    }
}
