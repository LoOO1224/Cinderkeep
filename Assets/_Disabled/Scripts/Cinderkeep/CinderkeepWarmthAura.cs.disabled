using UnityEngine;

namespace OODong.Cinderkeep
{
    // FlameHeart 주변 보호 오라.
    // 적이 너무 가까이 들어오면 주기적으로 피해를 줘서 "화로 주변은 안전하다"는 룰을 표현한다.
    public sealed class CinderkeepWarmthAura : MonoBehaviour
    {
        [SerializeField] private float _radius = 5.5f;
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _tickInterval = 1.4f;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _tickInterval)
            {
                return;
            }

            _timer = 0f;
            DamageEnemiesInRange();
        }

        private void DamageEnemiesInRange()
        {
            // TODO(팀원 작업 요청): 파티클/범위 표시가 필요하면 AuraView 컴포넌트를 별도로 만들어 주세요.
            CinderkeepEnemy[] enemies = FindObjectsByType<CinderkeepEnemy>(FindObjectsSortMode.None);
            float radiusSqr = _radius * _radius;

            for (int i = 0; i < enemies.Length; i++)
            {
                CinderkeepEnemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if ((enemy.transform.position - transform.position).sqrMagnitude <= radiusSqr)
                {
                    enemy.TakeDamage(_damage);
                }
            }
        }
    }
}
