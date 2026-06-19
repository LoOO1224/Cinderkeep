using UnityEngine;

// 피해를 받을 수 있는 오브젝트에 붙이는 공통 컴포넌트입니다.
// 실제 체력 처리는 PlayerStatus, EnemyStatus, BuildingHp, CinderHeart 같은 역할 컴포넌트로 넘깁니다.
// 적 체력 기준은 EnemyStatus이며, Damageable은 피해 요청을 전달하는 통로 역할만 합니다.
public sealed class Damageable : MonoBehaviour
{
    [Header("Fallback Health")]
    [SerializeField] private float _maxHealth = 100f;

    private PlayerStatus _playerStatus;
    private EnemyStatus _enemyStatus;
    private BuildingHp _buildingHp;
    private CinderHeart _cinderHeart;
    private float _currentHealth;
    private bool _isDead;

    public float CurrentHealth
    {
        get
        {
            return _currentHealth;
        }
    }

    public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }
    }

    public bool IsDead
    {
        get
        {
            return _isDead;
        }
    }

    private void Awake()
    {
        ConnectStatusComponents();
        InitializeFallbackHealth();
    }

    public void Initialize(float maxHealth)
    {
        _maxHealth = Mathf.Max(1f, maxHealth);
        InitializeFallbackHealth();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        if (ApplyDamageToRoleComponent(damage) == true)
        {
            return;
        }

        ApplyDamageToFallbackHealth(damage);
    }

    private void ConnectStatusComponents()
    {
        _playerStatus = GetComponent<PlayerStatus>();
        _enemyStatus = GetComponent<EnemyStatus>();
        _buildingHp = GetComponent<BuildingHp>();
        _cinderHeart = GetComponent<CinderHeart>();
    }

    private void InitializeFallbackHealth()
    {
        _currentHealth = _maxHealth;
        _isDead = false;
    }

    private bool ApplyDamageToRoleComponent(float damage)
    {
        if (_playerStatus != null)
        {
            _playerStatus.TakeDamage(damage);
            return true;
        }

        if (_enemyStatus != null)
        {
            _enemyStatus.TakeDamage(damage);
            return true;
        }

        if (_buildingHp != null)
        {
            _buildingHp.TakeDamage(damage);
            return true;
        }

        if (_cinderHeart != null)
        {
            _cinderHeart.TakeDamage(damage);
            return true;
        }

        return false;
    }

    private void ApplyDamageToFallbackHealth(float damage)
    {
        if (_isDead == true)
        {
            return;
        }

        _currentHealth -= damage;
        if (_currentHealth <= 0f)
        {
            ProcessDeath();
        }
    }

    private void ProcessDeath()
    {
        _currentHealth = 0f;
        _isDead = true;

        Debug.Log(gameObject.name + " is dead.");
    }
}
