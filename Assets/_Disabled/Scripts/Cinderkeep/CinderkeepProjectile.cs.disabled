using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepProjectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 18f;
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _lifeTime = 2f;

        private Vector3 _direction = Vector3.forward;
        private float _timer;

        private void OnEnable()
        {
            _timer = 0f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += _direction * (_speed * Time.deltaTime);

            if (_timer >= _lifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CinderkeepEnemy enemy = other.GetComponentInParent<CinderkeepEnemy>();
            if (enemy == null)
            {
                return;
            }

            enemy.TakeDamage(_damage);
            Destroy(gameObject);
        }

        public void Launch(Vector3 direction)
        {
            _direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);
        }
    }
}
