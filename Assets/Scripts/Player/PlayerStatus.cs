using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStatus : MonoBehaviour
{
    [FormerlySerializedAs("PlayerStatus")]
    [SerializeField] private float _health = 100f;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _stamina = 150f;
    [SerializeField] private float _maxStamina = 150f;

    [Header("스테미나 설정")]
    [SerializeField] private float _staminaRecoveryRate = 15f; // 스테미나 초당 회복량
    [SerializeField] private float _staminaConsumeRate = 15f; // 달리기 시 초당 소모량


    private bool _isExhausted = false;
    private PlayerMovement _playerMovement;

    public float CurrentHealth
    {
        get { return _health; }
    }

    public float CurrentStamina
    {
        get
        {
            if (_isExhausted == true) return 0f;
            return _stamina;
        }
    }

    //HUD 연결용 예상 함수들
    public float GetCurrentHealth() { return _health; }
    public float GetMaxHealth() { return _maxHealth; }
    public float GetCurrentStamina() { return _stamina; }
    public float GetMaxStamina() { return _maxStamina; }


    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        ActivateRecoverStamina();
    }


    //스테미나 자연회복
    private void ActivateRecoverStamina()
    {
        bool isRunningNow = _playerMovement != null && _playerMovement.IsRunningNow;

        if (IsDead() == false && _stamina < _maxStamina && isRunningNow == false)
        {
            RecoverStamina(_staminaRecoveryRate * Time.deltaTime);
        }
    }

    //스테미나 소모
    public void ConsumeStaminaInMovement()
    {
        UseStamina(_staminaConsumeRate * Time.deltaTime);
    }


    public void TakeDamage(float amount)
    {
        if (IsDead() == true) return; 

        _health -= amount;
        _health = Mathf.Max(_health, 0f);

        Debug.Log($"[PlayerStatus] 데미지 {amount} 피해! 현재 체력: {_health}/{_maxHealth}");

        if (IsDead() == true)
        {
            OnDeath();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead() == true) return; 

        _health += amount;

        _health = Mathf.Min(_health, _maxHealth);
    }

    public bool UseStamina(float amount)
    {
        if (IsDead() == true) return false;

        if (_stamina >= amount)
        {
            _stamina -= amount;
            if (_stamina <= 0.1f)
            {
                _stamina = 0f;
                _isExhausted = true;
                Debug.LogWarning("[PlayerStatus] 스태미나 고갈! 탈진 상태 진입 (달리기 불가)");
            }
            return true;
        }
        return false;
    }

    public void RecoverStamina(float amount)
    {
        if (IsDead() == true) return;

        _stamina += amount;
        _stamina = Mathf.Min(_stamina, _maxStamina);

        if (_isExhausted == true && _stamina >= 30f)
        {
            _isExhausted = false;
            Debug.Log("[PlayerStatus] 스태미나 충분히 회복됨. 탈진 상태 해제");
        }

    }

    private bool IsDead()
    {
        return _health <= 0f;
    }

    private void OnDeath()
    {
        Debug.LogWarning("[PlayerStatus] 플레이어 죽음 후 절차 진행 ");
    }

}
