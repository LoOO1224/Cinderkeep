using Cinderkeep.Gameplay;
using System;
using UnityEngine;

// 보스의 체력을 관리하는 컴포넌트입니다.
// 체력 감소, HUD 갱신, 피격 이벤트, 사망 이벤트를 한 곳에서 관리합니다.
// 게임 클리어 판단은 GameFlowController가 담당합니다.
public sealed class BossStatus : MonoBehaviour
{
    public static event Action<BossStatus> BossDamagedGlobal;
    public static event Action<float> BossDamagedByAmountGlobal;
    public static event Action<BossStatus> BossDiedGlobal;

    public event Action<BossStatus> Died;

    [Header("Health")]
    [Tooltip("보스 최대 체력입니다. BossData로 초기화되며, 데이터가 없을 때 fallback 값으로 사용됩니다.")]
    [SerializeField] private float _maxHealth = 1f;

    [Tooltip("체력이 0이 되었을 때 오브젝트를 비활성화할지 결정합니다.")]
    [SerializeField] private bool _deactivateOnDeath = true;

    [Header("Connected Components")]
    [Tooltip("보스 HP UI입니다. 비어 있으면 자식 오브젝트에서 찾습니다.")]
    [SerializeField] private EnemyHud _bossHud;

    private float _currentHealth;

    public float MaxHealth
    {
        get { return _maxHealth; }
    }

    public float CurrentHealth
    {
        get { return _currentHealth; }
    }

    public bool IsDead
    {
        get { return _currentHealth <= 0f; }
    }

    private void Awake()
    {
        ConnectComponents();
        InitializeHealth(_maxHealth);
    }

    public void Initialize(BossData bossData)
    {
        if (bossData == null)
        {
            return;
        }

        InitializeHealth(bossData.Health);
    }

    public void ResetHealth(float maxHealth)
    {
        InitializeHealth(maxHealth);
    }

    public void SetDeactivateOnDeath(bool deactivateOnDeath)
    {
        _deactivateOnDeath = deactivateOnDeath;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        if (IsDead)
        {
            return;
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        RefreshHud();
        NotifyBossDamaged();
        NotifyBossDamagedByAmount(damage);

        global::CinderkeepLog.Verbose("[BossStatus] " + gameObject.name + " 피해: " + damage + ", 현재 체력: " + _currentHealth + " / " + _maxHealth);

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
        if (_bossHud == null)
        {
            _bossHud = GetComponentInChildren<EnemyHud>();
        }
    }

    private void InitializeHealth(float maxHealth)
    {
        _maxHealth = Mathf.Max(1f, maxHealth);
        _currentHealth = _maxHealth;
        RefreshHud();
    }

    private void RefreshHud()
    {
        if (_bossHud == null)
        {
            return;
        }

        _bossHud.RefreshHealth(_currentHealth, _maxHealth);
    }

    private void ProcessDeath()
    {
        global::CinderkeepLog.Verbose("[BossStatus] " + gameObject.name + " 사망 처리");

        NotifyDied();
        NotifyBossDiedGlobal();

        if (_deactivateOnDeath == false)
        {
            return;
        }

        gameObject.SetActive(false);
    }

    private void NotifyDied()
    {
        if (Died == null)
        {
            return;
        }

        Died(this);
    }

    private void NotifyBossDamaged()
    {
        if (BossDamagedGlobal == null)
        {
            return;
        }

        BossDamagedGlobal(this);
    }

    private void NotifyBossDamagedByAmount(float damage)
    {
        if (BossDamagedByAmountGlobal == null)
        {
            return;
        }

        BossDamagedByAmountGlobal(damage);
    }

    private void NotifyBossDiedGlobal()
    {
        if (BossDiedGlobal == null)
        {
            return;
        }

        BossDiedGlobal(this);
    }
}