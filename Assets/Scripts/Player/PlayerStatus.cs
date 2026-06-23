using System;
using Cinderkeep.Gameplay;
using UnityEngine;

// 플레이어의 체력과 스태미나를 관리하는 컴포넌트입니다.
// 이동 입력과 HUD 표시는 다른 컴포넌트가 맡고, 이 클래스는 수치 계산과 사망 처리만 담당합니다.
public sealed class PlayerStatus : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("현재 플레이어 체력입니다. 0이 되면 게임 오버 흐름으로 연결됩니다.")]
    [SerializeField] private float _health = 100f;
    [Tooltip("플레이어 최대 체력입니다.")]
    [SerializeField] private float _maxHealth = 100f;

    [Header("Satiety")]
    [Tooltip("플레이어 현재 포만도입니다.")]
    [SerializeField] private float _satiety = 100f;
    [Tooltip("플레이어 최대 포만도입니다.")]
    [SerializeField] private float _maxSatiety = 100f;
    [Tooltip("초당 소모되는 포만도입니다.")]
    [SerializeField] private float _satietyConsumeRate = 1f;

    [Header("Stamina")]
    [Tooltip("현재 플레이어 스태미나입니다. 달리면 감소하고 멈추면 회복됩니다.")]
    [SerializeField] private float _stamina = 150f;
    [Tooltip("플레이어 최대 스태미나입니다.")]
    [SerializeField] private float _maxStamina = 150f;
    [Tooltip("달리지 않을 때 초당 회복되는 스태미나 양입니다.")]
    [SerializeField] private float _staminaRecoveryRate = 15f;
    [Tooltip("달리는 동안 초당 소모되는 스태미나 양입니다.")]
    [SerializeField] private float _staminaConsumeRate = 15f;
    [Tooltip("스태미나가 고갈된 뒤 다시 달릴 수 있게 되는 회복 기준점입니다.")]
    [SerializeField] private float _exhaustedRecoveryPoint = 30f;

    private bool _isExhausted;
    private bool _isStarving;
    private bool _isGameOverRequested;
    private PlayerMovement _playerMovement;
    private DeathCinderHeartView _deathCinderHeartView;

    public event Action<float> PlayerDamaged;

    public float CurrentHealth
    {
        get
        {
            return _health;
        }
    }

    public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }
    }

    public float CurrentStamina
    {
        get
        {
            return _stamina;
        }
    }
    public float MaxStamina
    {
        get
        {
            return _maxStamina;
        }
    }

    public float CurrentSatiety
    {
        get
        {
            return _satiety;
        }
    }

    public float MaxSatiety
    {
        get
        {
            return _maxSatiety;
        }
    }

    public bool IsStarving
    {
        get
        {
            return _isStarving;
        }
    }

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _deathCinderHeartView = GetComponent<DeathCinderHeartView>();
        ClampStatusValues();
    }

    private void Update()
    {
        RecoverStaminaByTime();
        HungerByTime();
        Hunger();
    }

    public float GetCurrentHealth()
    {
        return _health;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    public float GetCurrentStamina()
    {
        return _stamina;
    }

    public float GetMaxStamina()
    {
        return _maxStamina;
    }

    public float GetCurrentSatiety()
    {
        return _satiety;
    }

    public float GetMaxSatiety()
    {
        return _maxSatiety;
    }

    public bool CanRun()
    {
        if (IsDead() == true)
        {
            return false;
        }

        if (_isExhausted == true)
        {
            return false;
        }

        return _stamina > 0f;
    }

    public bool Hunger()
    {
        if (_satiety > 0f)
        {
            _isStarving = false;
            return false;
        }
        else
        {
            _isStarving = true;
            return true;
        }
    }

    public void ConsumeStaminaForRun()
    {
        UseStamina(_staminaConsumeRate * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead() == true)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        _health -= amount;
        _health = Mathf.Max(_health, 0f);

        Debug.Log("[PlayerStatus] 피해: " + amount + ", 현재 체력: " + _health + " / " + _maxHealth);
        NotifyPlayerDamaged(amount);

        if (IsDead() == true)
        {
            ProcessDeath();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead() == true)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        _health += amount;
        _health = Mathf.Min(_health, _maxHealth);
    }

    public void EatFood(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _satiety += amount;
        _satiety = Mathf.Min(_satiety, _maxSatiety);
    }

    public bool UseStamina(float amount)
    {
        if (IsDead() == true)
        {
            return false;
        }

        if (_stamina < amount)
        {
            return false;
        }

        _stamina -= amount;

        if (_stamina <= 0.1f)
        {
            _stamina = 0f;
            _isExhausted = true;
            Debug.LogWarning("[PlayerStatus] 스태미나가 고갈되어 잠시 달릴 수 없습니다.");
        }

        return true;
    }
    public void RecoverStamina(float amount)
    {
        if (IsDead() == true)
        {
            return;
        }

        // 모든 회복을 막고 싶다면 여기에도 아래 조건을 넣으세요.
        // if (_isStarving == true)
        // {
        //     return;
        // }

        _stamina += amount;
        _stamina = Mathf.Min(_stamina, _maxStamina);

        if (_isExhausted == true && _stamina >= _exhaustedRecoveryPoint)
        {
            _isExhausted = false;
            Debug.Log("[PlayerStatus] 스태미나가 회복되어 다시 달릴 수 있습니다.");
        }
    }

    private void RecoverStaminaByTime()
    {
        if (_isStarving == true)
        {
            return;
        }

        bool isRunningNow = CheckIsRunningNow();

        if (IsDead() == false && _stamina < _maxStamina && isRunningNow == false)
        {
            RecoverStamina(_staminaRecoveryRate * Time.deltaTime);
        }
    }

    private void HungerByTime()
    {
        if (IsDead() == true)
        {
            return;
        }

        _satiety -= _satietyConsumeRate * Time.deltaTime;
        _satiety = Mathf.Max(_satiety, 0f);
    }
    private bool CheckIsRunningNow()
    {
        if (_playerMovement == null)
        {
            return false;
        }

        return _playerMovement.IsRunningNow;
    }

    private void ClampStatusValues()
    {
        _maxHealth = Mathf.Max(_maxHealth, 1f);
        _maxStamina = Mathf.Max(_maxStamina, 1f);
        _maxSatiety = Mathf.Max(_maxSatiety, 1f);
        _health = Mathf.Clamp(_health, 0f, _maxHealth);
        _stamina = Mathf.Clamp(_stamina, 0f, _maxStamina);
        _satiety = Mathf.Clamp(_satiety, 0f, _maxSatiety);
    }

    private bool IsDead()
    {
        return _health <= 0f;
    }

    private void NotifyPlayerDamaged(float damage)
    {
        if (PlayerDamaged == null)
        {
            return;
        }

        PlayerDamaged(damage);
    }

    private void ProcessDeath()
    {
        if (_isGameOverRequested == true)
        {
            return;
        }

        _isGameOverRequested = true;
        Debug.LogWarning("[PlayerStatus] 플레이어가 사망하여 게임 오버를 요청합니다.");
        ShowDeathView();

        if (GameManager.Inst == null)
        {
            return;
        }

        GameManager.Inst.EndGame();
    }

    private void ShowDeathView()
    {
        if (_deathCinderHeartView == null)
        {
            return;
        }

        _deathCinderHeartView.ShowCinderHeartView();
    }
}