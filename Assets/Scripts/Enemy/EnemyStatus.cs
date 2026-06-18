using Cinderkeep.Gameplay;
using UnityEngine;

// 몬스터의 체력을 관리하는 컴포넌트입니다.
// 데미지 계산과 사망 처리를 담당하고, 이동/감지/공격 판단은 다른 컴포넌트가 담당합니다.
public sealed class EnemyStatus : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 1f;
    [SerializeField] private bool _deactivateOnDeath = true;

    [Header("Connected Components")]
    [SerializeField] private EnemyHud EnemyHud_EnemyHud;

    private EnemyDetector _enemyDetector;
    private float _currentHealth;

    public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }
    }

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

    private void Awake()
    {
        ConnectComponents();
        InitializeHealth(_maxHealth);
    }

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        InitializeHealth(enemyData.Health);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        RefreshHud();
        AlertByDamage();

        Debug.Log("[EnemyStatus] " + gameObject.name + " 피해: " + damage + ", 현재 체력: " + _currentHealth + " / " + _maxHealth);

        if (IsDead)
        {
            ProcessDeath();
        }
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    private void ConnectComponents()
    {
        _enemyDetector = GetComponent<EnemyDetector>();

        if (EnemyHud_EnemyHud == null)
        {
            EnemyHud_EnemyHud = GetComponentInChildren<EnemyHud>();
        }
    }

    private void InitializeHealth(float maxHealth)
    {
        _maxHealth = Mathf.Max(1f, maxHealth);
        _currentHealth = _maxHealth;
        RefreshHud();
    }

    private void AlertByDamage()
    {
        if (_enemyDetector == null)
        {
            return;
        }

        _enemyDetector.EnableAlertMode();
    }

    private void RefreshHud()
    {
        if (EnemyHud_EnemyHud == null)
        {
            return;
        }

        EnemyHud_EnemyHud.RefreshHealth(_currentHealth, _maxHealth);
    }

    private void ProcessDeath()
    {
        Debug.Log("[EnemyStatus] " + gameObject.name + " 사망 처리");

        if (_deactivateOnDeath == false)
        {
            return;
        }

        gameObject.SetActive(false);
    }
}
