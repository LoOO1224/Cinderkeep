using UnityEngine;

// 타워가 EnemyStatus 대상에게 피해를 전달하는 전투 컴포넌트입니다.
// 타깃 탐색은 TowerTargeting, 전체 반복 실행은 BuildingTower가 담당합니다.
public sealed class TowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("타워가 적에게 주는 피해량입니다. BuildingTower 초기화 시 buildings.json 값으로 덮어씁니다.")]
    [SerializeField] private float _attackDamage = 5f;
    [Tooltip("다음 공격까지 기다리는 시간입니다.")]
    [SerializeField] private float _attackInterval = 1.4f;

    [Header("Connected Components")]
    [Tooltip("Run Result에 타워 피해량을 남기기 위한 공통 피해 전달자입니다.")]
    [SerializeField] private DamageDealer _damageDealer;

    private float _lastAttackTime;

    public float AttackDamage
    {
        get
        {
            return _attackDamage;
        }
    }

    private void Awake()
    {
        ConnectDamageDealer();
    }

    public void SetAttackDamage(float attackDamage)
    {
        _attackDamage = Mathf.Max(0f, attackDamage);
        ConnectDamageDealer();

        if (_damageDealer != null)
        {
            _damageDealer.SetDamageValue(_attackDamage);
        }
    }

    public void SetAttackInterval(float attackInterval)
    {
        _attackInterval = Mathf.Max(0.1f, attackInterval);
    }

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + _attackInterval;
    }

    public bool TryAttack(EnemyStatus targetEnemyStatus)
    {
        if (targetEnemyStatus == null || targetEnemyStatus.IsDead)
        {
            return false;
        }

        if (CanAttack() == false)
        {
            return false;
        }

        ApplyDamageToEnemy(targetEnemyStatus);
        RecordAttackTime();
        return true;
    }

    private void ApplyDamageToEnemy(EnemyStatus targetEnemyStatus)
    {
        Damageable enemyDamageable = targetEnemyStatus.GetComponentInParent<Damageable>();
        if (_damageDealer != null && enemyDamageable != null)
        {
            _damageDealer.SetDamageValue(_attackDamage);
            _damageDealer.ApplyDamage(enemyDamageable);
            return;
        }

        targetEnemyStatus.TakeDamage(_attackDamage);
        RecordDirectTowerDamage(_attackDamage);
    }

    private void RecordDirectTowerDamage(float damage)
    {
        if (Cinderkeep.Gameplay.RunResultTracker.Instance == null)
        {
            return;
        }

        Cinderkeep.Gameplay.RunResultTracker.Instance.RecordTowerDamageDealt(damage);
    }

    private void RecordAttackTime()
    {
        _lastAttackTime = Time.time;
    }

    private void ConnectDamageDealer()
    {
        if (_damageDealer == null)
        {
            _damageDealer = GetComponent<DamageDealer>();
        }

        if (_damageDealer == null)
        {
            _damageDealer = gameObject.AddComponent<DamageDealer>();
        }

        _damageDealer.SetSourceType(DamageSourceType.Tower);
        _damageDealer.SetDamageValue(_attackDamage);
    }
}
