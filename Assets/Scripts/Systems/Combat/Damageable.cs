using UnityEngine;

// 피해를 받을 수 있는 대상에게 붙는 공통 진입점입니다.
// 실제 체력 처리는 PlayerStatus, EnemyStatus, BuildingHp, CinderHeart 중 붙어 있는 역할 컴포넌트로 위임합니다.
public sealed class Damageable : MonoBehaviour
{
    [Header("Fallback Health")]
    [Tooltip("역할 체력 컴포넌트가 없을 때만 사용하는 예비 체력입니다.")]
    [SerializeField] private float _maxHealth = 100f;

    private PlayerStatus _playerStatus;
    private EnemyStatus _enemyStatus;
    private BossStatus _bossStatus;
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
        _bossStatus = GetComponent<BossStatus>();
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

        if (_bossStatus != null)
        {
            _bossStatus.TakeDamage(damage);
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

        global::CinderkeepLog.Verbose(gameObject.name + " is dead.");
    }
}
