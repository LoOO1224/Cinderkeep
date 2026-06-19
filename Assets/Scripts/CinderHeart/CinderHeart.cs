using Cinderkeep.Gameplay;
using UnityEngine;

// CinderHeart는 3일 루프의 핵심 방어 대상입니다.
// 체력이 0이 되면 GameManager를 통해 게임 오버 흐름으로 넘깁니다.
public sealed class CinderHeart : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("CinderHeart 최대 체력입니다. 0이 되면 게임 오버가 됩니다.")]
    [SerializeField] private float _maxHealth = 500f;

    [Header("Damage Test")]
    [Tooltip("디버그용 테스트 피해량입니다. 실제 적 공격 피해는 EnemyAttack에서 조절합니다.")]
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
        Debug.Log("[CinderHeart] 피해: " + damage + ", 현재 체력: " + _currentHealth + " / " + _maxHealth);

        if (_currentHealth <= 0f)
        {
            DestroyCinderHeart();
        }
    }

    public void TakeTestDamage()
    {
        TakeDamage(_testDamage);
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
    }
}
