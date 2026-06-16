using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepAutoShooter : MonoBehaviour
    {
        [SerializeField] private Transform Transform_FireOrigin;
        [SerializeField] private CinderkeepProjectile CinderkeepProjectile_ProjectileTemplate;
        [SerializeField] private float _range = 24f;
        [SerializeField] private float _cooldown = 0.55f;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
        }

        public void SetProjectileTemplate(CinderkeepProjectile projectileTemplate)
        {
            CinderkeepProjectile_ProjectileTemplate = projectileTemplate;
        }

        public void SetFireOrigin(Transform fireOrigin)
        {
            Transform_FireOrigin = fireOrigin;
        }

        public bool TryFireNearestEnemy()
        {
            if (CinderkeepProjectile_ProjectileTemplate == null || _timer < _cooldown)
            {
                return false;
            }

            CinderkeepEnemy target = FindNearestEnemy();
            if (target == null)
            {
                return false;
            }

            _timer = 0f;
            FireAt(target.transform);
            return true;
        }

        private CinderkeepEnemy FindNearestEnemy()
        {
            CinderkeepEnemy nearestEnemy = null;
            float nearestDistanceSqr = _range * _range;
            CinderkeepEnemy[] enemies = FindObjectsByType<CinderkeepEnemy>(FindObjectsSortMode.None);

            for (int i = 0; i < enemies.Length; i++)
            {
                if (!enemies[i].IsAlive)
                {
                    continue;
                }

                float distanceSqr = (enemies[i].transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestEnemy = enemies[i];
                }
            }

            return nearestEnemy;
        }

        private void FireAt(Transform target)
        {
            Transform origin = Transform_FireOrigin != null ? Transform_FireOrigin : transform;
            CinderkeepProjectile projectile = Instantiate(CinderkeepProjectile_ProjectileTemplate, origin.position, Quaternion.identity);
            projectile.gameObject.SetActive(true);
            GameObjectManager.Instance?.RegisterGameObject(projectile.gameObject);
            Vector3 direction = (target.position + Vector3.up - origin.position).normalized;
            projectile.Launch(direction);
        }
    }
}
