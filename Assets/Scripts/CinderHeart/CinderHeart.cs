using Cinderkeep.Gameplay;
using System;
using UnityEngine;

// CinderHeart는 3일 루프의 핵심 방어 대상입니다.
// 체력이 0이 되면 GameManager를 통해 게임 오버 흐름으로 넘깁니다.
public sealed class CinderHeart : MonoBehaviour
{
    public static event Action<float> CinderHeartDamagedGlobal;

    [Header("Health")]
    [Tooltip("CinderHeart 최대 체력입니다. 0이 되면 게임 오버가 됩니다.")]
    [SerializeField] private float _maxHealth = 500f;

    [Header("Skill Stats")]
    [Tooltip("CinderHeart 보상 스킬로 증가하는 공격력입니다. 이후 CinderHeart 공격 로직에서 사용합니다.")]
    [SerializeField] private float _attackDamage;

    [Header("Debug Damage")]
    [Tooltip("에디터 확인용 피해량입니다. 실제 적 공격 피해는 EnemyAttack 또는 enemies.json에서 조절합니다.")]
    [SerializeField] private float _testDamage = 1f;

    private float _currentHealth;
    private bool _isDestroyed;

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

    public bool IsDestroyed
    {
        get
        {
            return _isDestroyed;
        }
    }

    public float AttackDamage
    {
        get
        {
            return _attackDamage;
        }
    }

    private void Awake()
    {
        InitializeHealth();
    }

    private void OnValidate()
    {
        ClampInspectorValues();
    }

    public Transform GetTargetTransform()
    {
        return transform;
    }

    public void InitializeHealth()
    {
        _maxHealth = Mathf.Max(1f, _maxHealth);
        _currentHealth = _maxHealth;
        _isDestroyed = false;
    }

    public void TakeDamage(float damage)
    {
        if (_isDestroyed == true)
        {
            return;
        }

        if (damage <= 0f)
        {
            return;
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        NotifyCinderHeartDamaged(damage);
        Debug.Log("[CinderHeart] 피해: " + damage + ", 현재 체력: " + _currentHealth + " / " + _maxHealth);

        if (_currentHealth <= 0f)
        {
            DestroyCinderHeart();
        }
    }

    public void TakeDebugDamage()
    {
        TakeDamage(_testDamage);
    }

    public void AddAttackDamage(float amount)
    {
        // 아침 보상 스킬로 증가하는 CinderHeart 공격력입니다.
        // 기본 스킬 수치는 cinderheart_skills.json의 value 값을 수정해 조절합니다.
        if (amount <= 0f)
        {
            return;
        }

        _attackDamage += amount;
        Debug.Log("[CinderHeart] 공격력 증가: +" + amount + ", 현재 공격력: " + _attackDamage);
    }

    public void AddMaxHealth(float amount)
    {
        // 최대 체력 증가 보상은 즉시 현재 체력도 같이 올려 다음 밤 준비 보상으로 체감되게 합니다.
        // 보상량은 cinderheart_skills.json의 value 값을 수정해 조절합니다.
        if (amount <= 0f)
        {
            return;
        }

        _maxHealth += amount;
        _currentHealth += amount;
        ClampInspectorValues();
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);
        Debug.Log("[CinderHeart] 최대 체력 증가: +" + amount + ", 현재 체력: " + _currentHealth + " / " + _maxHealth);
    }

    public void Heal(float amount)
    {
        if (_isDestroyed == true)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        Debug.Log("[CinderHeart] 체력 회복: +" + amount + ", 현재 체력: " + _currentHealth + " / " + _maxHealth);
    }

    public void HealByRate(float rate)
    {
        if (rate <= 0f)
        {
            return;
        }

        Heal(_maxHealth * rate);
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    private void DestroyCinderHeart()
    {
        _isDestroyed = true;
        Debug.LogWarning("[CinderHeart] 체력이 0이 되어 게임 오버를 요청합니다.");

        if (GameManager.Inst == null)
        {
            return;
        }

        GameManager.Inst.EndGame();
    }

    private void NotifyCinderHeartDamaged(float damage)
    {
        if (CinderHeartDamagedGlobal == null)
        {
            return;
        }

        CinderHeartDamagedGlobal(damage);
    }

    private void ClampInspectorValues()
    {
        if (_maxHealth < 1f)
        {
            _maxHealth = 1f;
        }

        if (_testDamage < 0f)
        {
            _testDamage = 0f;
        }

        if (_attackDamage < 0f)
        {
            _attackDamage = 0f;
        }
    }
}
