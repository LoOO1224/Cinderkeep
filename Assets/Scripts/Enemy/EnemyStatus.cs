using UnityEngine;
using Cinderkeep.Gameplay;

public sealed class EnemyStatus : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 1f;

    private EnemyDetector _enemyDetector;
    private float _currentHealth;

    public float CurrentHealth
    {
        get
        {
            return _currentHealth;
        }
    }

    public bool IsDead
    {
        get
        {
            return _currentHealth <= 0f;
        }
    }

    private void Start()
    {
        _enemyDetector = GetComponent<EnemyDetector>();
    }

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        _maxHealth = enemyData.Health;
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);

        if (_enemyDetector != null)
        {
            _enemyDetector.EnableAlertMode();
        }
    }
}
