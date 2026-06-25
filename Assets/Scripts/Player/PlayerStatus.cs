using System;
using Cinderkeep.Gameplay;
using UnityEngine;

// 플레이어의 체력, 스태미나, 포만도를 관리하는 컴포넌트입니다.
// 이동 입력, 퀵슬롯, 전투, HUD 표시는 다른 컴포넌트가 맡고, 이 클래스는 수치 계산과 사망/부활 처리만 담당합니다.
public sealed class PlayerStatus : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("현재 플레이어 체력입니다. 0이 되면 CinderHeart 관전/부활 대기 상태로 전환됩니다.")]
    [SerializeField] private float _health = 100f;
    [Tooltip("플레이어 최대 체력입니다.")]
    [SerializeField] private float _maxHealth = 100f;
    [Tooltip("부활 직후 피해를 무시하는 시간입니다.")]
    [SerializeField] private float _reviveInvulnerableSeconds = 3f;

    [Header("Satiety")]
    [Tooltip("플레이어 현재 포만도입니다.")]
    [SerializeField] private float _satiety = 100f;
    [Tooltip("플레이어 최대 포만도입니다.")]
    [SerializeField] private float _maxSatiety = 100f;
    [Tooltip("초당 소모되는 포만도입니다. 0이 되면 자동 스태미나 회복이 멈춥니다.")]
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
    private float _equipmentDefense;
    private float _equipmentMaxHealthBonus;
    private float _equipmentMaxStaminaBonus;
    private float _baseMaxHealth;
    private float _baseMaxStamina;
    private float _baseMaxSatiety;
    private float _baseStaminaRecoveryRate;
    private float _invulnerableUntilTime;
    private PlayerMovement _playerMovement;
    private PlayerController _playerController;
    private DeathCinderHeartView _deathCinderHeartView;

    public event Action<float> PlayerDamaged;
    public static event Action<float> PlayerDamagedGlobal;
    public static event Action PlayerDiedGlobal;

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

    private void Awake()
    {
        CacheBaseStatusValues();
    }

    private void Start()
    {
        ResolveRuntimeReferences();
        ClampStatusValues();
    }

    private void Update()
    {
        ConsumeSatietyByTime();
        RefreshStarvingState();
        RecoverStaminaByTime();
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

        if (IsInvulnerable())
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        float finalDamage = GetFinalDamage(amount);
        _health -= finalDamage;
        _health = Mathf.Max(_health, 0f);

        global::CinderkeepLog.Verbose("[PlayerStatus] 피해: " + finalDamage + ", 현재 체력: " + _health + " / " + _maxHealth);
        NotifyPlayerDamaged(finalDamage);

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

    public void AddMaxHealth(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _maxHealth += amount;
        _health += amount;
        ClampStatusValues();
    }

    public void AddMaxStamina(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _maxStamina += amount;
        _stamina += amount;
        ClampStatusValues();
    }

    public void AddStaminaRecoveryRate(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _staminaRecoveryRate += amount;
    }

    public void AddMaxSatiety(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _maxSatiety += amount;
        _satiety += amount;
        ClampStatusValues();
    }

    public void SetEquipmentStatBonuses(float defense, float maxHealthBonus, float maxStaminaBonus)
    {
        float healthDelta = maxHealthBonus - _equipmentMaxHealthBonus;
        float staminaDelta = maxStaminaBonus - _equipmentMaxStaminaBonus;

        _equipmentDefense = Mathf.Max(0f, defense);
        _equipmentMaxHealthBonus = maxHealthBonus;
        _equipmentMaxStaminaBonus = maxStaminaBonus;

        _maxHealth += healthDelta;
        _maxStamina += staminaDelta;

        if (healthDelta > 0f)
        {
            _health += healthDelta;
        }

        if (staminaDelta > 0f)
        {
            _stamina += staminaDelta;
        }

        ClampStatusValues();
    }

    public void Revive(float healthRate)
    {
        ResolveRuntimeReferences();

        if (IsDead() == false)
        {
            return;
        }

        float safeHealthRate = Mathf.Clamp01(healthRate);
        if (safeHealthRate <= 0f)
        {
            safeHealthRate = 0.4f;
        }

        _health = Mathf.Max(1f, _maxHealth * safeHealthRate);
        _stamina = Mathf.Max(_stamina, _maxStamina * 0.5f);
        _isGameOverRequested = false;
        _invulnerableUntilTime = Time.time + Mathf.Max(0f, _reviveInvulnerableSeconds);

        if (_playerController != null)
        {
            _playerController.SetState(PlayerControlState.Normal);
        }

        HideDeathView();
        ClampStatusValues();
    }

    public void ResetStatusForNewRun()
    {
        ResolveRuntimeReferences();

        _maxHealth = _baseMaxHealth;
        _maxStamina = _baseMaxStamina;
        _maxSatiety = _baseMaxSatiety;
        _staminaRecoveryRate = _baseStaminaRecoveryRate;
        _equipmentDefense = 0f;
        _equipmentMaxHealthBonus = 0f;
        _equipmentMaxStaminaBonus = 0f;
        _health = _maxHealth;
        _stamina = _maxStamina;
        _satiety = _maxSatiety;
        _isExhausted = false;
        _isGameOverRequested = false;
        _invulnerableUntilTime = 0f;

        if (_playerController != null)
        {
            _playerController.SetState(PlayerControlState.Normal);
        }

        HideDeathView();
        ClampStatusValues();
    }

    public void EatFood(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _satiety += amount;
        _satiety = Mathf.Min(_satiety, _maxSatiety);
        RefreshStarvingState();
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

        _stamina += amount;
        _stamina = Mathf.Min(_stamina, _maxStamina);

        if (_isExhausted == true && _stamina >= _exhaustedRecoveryPoint)
        {
            _isExhausted = false;
            global::CinderkeepLog.Verbose("[PlayerStatus] 스태미나가 회복되어 다시 달릴 수 있습니다.");
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

    private void ConsumeSatietyByTime()
    {
        if (IsDead() == true)
        {
            return;
        }

        _satiety -= _satietyConsumeRate * Time.deltaTime;
        _satiety = Mathf.Max(_satiety, 0f);
    }

    private void RefreshStarvingState()
    {
        _isStarving = _satiety <= 0f;
    }

    private float GetFinalDamage(float rawDamage)
    {
        if (_equipmentDefense <= 0f)
        {
            return rawDamage;
        }

        return Mathf.Max(1f, rawDamage - _equipmentDefense);
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
        RefreshStarvingState();
    }

    private void CacheBaseStatusValues()
    {
        _baseMaxHealth = Mathf.Max(1f, _maxHealth);
        _baseMaxStamina = Mathf.Max(1f, _maxStamina);
        _baseMaxSatiety = Mathf.Max(1f, _maxSatiety);
        _baseStaminaRecoveryRate = _staminaRecoveryRate;
    }

    private void ResolveRuntimeReferences()
    {
        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_playerController == null)
        {
            _playerController = GetComponent<PlayerController>();
        }

        if (_deathCinderHeartView == null)
        {
            _deathCinderHeartView = GetComponent<DeathCinderHeartView>();
        }
    }

    public bool IsDead()
    {
        return _health <= 0f;
    }

    private bool IsInvulnerable()
    {
        return _invulnerableUntilTime > 0f && Time.time < _invulnerableUntilTime;
    }

    private void NotifyPlayerDamaged(float damage)
    {
        if (PlayerDamaged == null)
        {
            NotifyPlayerDamagedGlobal(damage);
            return;
        }

        PlayerDamaged(damage);
        NotifyPlayerDamagedGlobal(damage);
    }

    private void NotifyPlayerDamagedGlobal(float damage)
    {
        if (PlayerDamagedGlobal == null)
        {
            return;
        }

        PlayerDamagedGlobal(damage);
    }

    private void ProcessDeath()
    {
        if (_isGameOverRequested == true)
        {
            return;
        }

        ResolveRuntimeReferences();
        _isGameOverRequested = true;
        NotifyPlayerDiedGlobal();
        Debug.LogWarning("[PlayerStatus] 플레이어가 사망하여 CinderHeart 관전 상태로 전환합니다.");
        if (_playerController != null)
        {
            _playerController.SetState(PlayerControlState.Dead);
        }

        ShowDeathView();
    }

    private void ShowDeathView()
    {
        if (_deathCinderHeartView == null)
        {
            return;
        }

        _deathCinderHeartView.ShowCinderHeartView();
    }

    private void HideDeathView()
    {
        if (_deathCinderHeartView == null)
        {
            return;
        }

        _deathCinderHeartView.HideCinderHeartView();
    }

    private void NotifyPlayerDiedGlobal()
    {
        if (PlayerDiedGlobal == null)
        {
            return;
        }

        PlayerDiedGlobal();
    }
}
