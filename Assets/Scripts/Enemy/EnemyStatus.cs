using Cinderkeep.Gameplay;
using System;
using UnityEngine;

// 몬스터의 체력을 관리하는 컴포넌트입니다.
// 데미지 계산과 사망 처리를 담당하고, 이동/감지/공격 판단은 다른 컴포넌트가 담당합니다.
// 기준: 적 체력 원본은 EnemyStatus입니다.
// 적 체력 감소, HUD 갱신, 피격 알림, 사망 처리는 이 클래스에서만 관리합니다.
// EnemyHp는 예전 작업과 연결하기 위한 호환용 전달자이며, 새 체력 기능은 이 클래스에 추가합니다.
public sealed class EnemyStatus : MonoBehaviour
{
    public static event Action<EnemyStatus> EnemyDamaged;

    [Header("Health")]
    [Tooltip("몬스터 최대 체력입니다. EnemyData로 초기화되며, 데이터가 없을 때 fallback 값으로 사용됩니다.")]
    [SerializeField] private float _maxHealth = 1f;
    [Tooltip("체력이 0이 되었을 때 오브젝트를 비활성화할지 결정합니다.")]
    [SerializeField] private bool _deactivateOnDeath = true;

    [Header("Connected Components")]
    [Tooltip("몬스터 머리 위 HP UI입니다. 비어 있으면 자식 오브젝트에서 찾습니다.")]
    [SerializeField] private EnemyHud _enemyHud;

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
        NotifyEnemyDamaged();
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

        if (_enemyHud == null)
        {
            _enemyHud = GetComponentInChildren<EnemyHud>();
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
        if (_enemyHud == null)
        {
            return;
        }

        _enemyHud.RefreshHealth(_currentHealth, _maxHealth);
    }

    private void NotifyEnemyDamaged()
    {
        if (EnemyDamaged == null)
        {
            return;
        }

        EnemyDamaged(this);
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
