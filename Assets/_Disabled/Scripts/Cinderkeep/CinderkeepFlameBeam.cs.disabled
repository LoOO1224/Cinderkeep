using UnityEngine;

namespace OODong.Cinderkeep
{
    // FlameHeart 또는 Turret에 붙일 수 있는 자동 공격 컴포넌트.
    // 현재는 가장 가까운 적에게 직접 damage를 넣는 MVP 버전이다.
    public sealed class CinderkeepFlameBeam : MonoBehaviour
    {
        [SerializeField] private Transform Transform_FireOrigin;
        [SerializeField] private float _range = 18f;
        [SerializeField] private int _damage = 2;
        [SerializeField] private float _cooldown = 0.7f;

        private float _timer;

        private void Awake()
        {
            if (Transform_FireOrigin == null)
            {
                Transform_FireOrigin = transform;
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _cooldown)
            {
                return;
            }

            CinderkeepEnemy target = FindNearestEnemy();
            if (target == null)
            {
                return;
            }

            _timer = 0f;
            target.TakeDamage(_damage);
        }

        public void SetFireOrigin(Transform fireOrigin)
        {
            Transform_FireOrigin = fireOrigin;
        }

        private CinderkeepEnemy FindNearestEnemy()
        {
            // TODO(팀원 작업 요청): 적 수가 많아지면 FindObjectsByType 대신 GameObjectManager/EnemyRegistry로 교체해 주세요.
            CinderkeepEnemy nearestEnemy = null;
            float nearestDistanceSqr = _range * _range;
            Vector3 origin = Transform_FireOrigin != null ? Transform_FireOrigin.position : transform.position;
            CinderkeepEnemy[] enemies = FindObjectsByType<CinderkeepEnemy>(FindObjectsSortMode.None);

            for (int i = 0; i < enemies.Length; i++)
            {
                CinderkeepEnemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float distanceSqr = (enemy.transform.position - origin).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }
    }
}
