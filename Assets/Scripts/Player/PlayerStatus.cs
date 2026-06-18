using UnityEngine;

// 플레이어의 체력과 스태미나를 관리하는 컴포넌트입니다.
// 이동 입력과 HUD 표시는 다른 컴포넌트가 맡고, 이 클래스는 수치 계산만 담당합니다.
public sealed class PlayerStatus : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _health = 100f;
    [SerializeField] private float _maxHealth = 100f;

    [Header("Stamina")]
    [SerializeField] private float _stamina = 150f;
    [SerializeField] private float _maxStamina = 150f;
    [SerializeField] private float _staminaRecoveryRate = 15f;
    [SerializeField] private float _staminaConsumeRate = 15f;
    [SerializeField] private float _exhaustedRecoveryPoint = 30f;

    private bool _isExhausted;
    private PlayerMovement _playerMovement;

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

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        ClampStatusValues();
    }

    private void Update()
    {
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

        _health -= amount;
        _health = Mathf.Max(_health, 0f);

        Debug.Log("[PlayerStatus] 피해를 받았습니다. 현재 체력: " + _health + " / " + _maxHealth);

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

        _health += amount;
        _health = Mathf.Min(_health, _maxHealth);
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
            Debug.Log("[PlayerStatus] 스태미나가 회복되어 다시 달릴 수 있습니다.");
        }
    }

    private void RecoverStaminaByTime()
    {
        bool isRunningNow = CheckIsRunningNow();

        if (IsDead() == false && _stamina < _maxStamina && isRunningNow == false)
        {
            RecoverStamina(_staminaRecoveryRate * Time.deltaTime);
        }
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
        _health = Mathf.Clamp(_health, 0f, _maxHealth);
        _stamina = Mathf.Clamp(_stamina, 0f, _maxStamina);
    }

    private bool IsDead()
    {
        return _health <= 0f;
    }

    private void ProcessDeath()
    {
        Debug.LogWarning("[PlayerStatus] 플레이어 사망 처리 진입점입니다. 이후 GameOver UI와 연결합니다.");
    }
}
