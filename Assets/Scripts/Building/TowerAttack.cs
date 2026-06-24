using UnityEngine;

// 타워가 EnemyStatus 대상을 공격하고 피해를 전달하는 전투 컴포넌트입니다.
// 설치/비용/체력은 다른 건축 스크립트가 담당하고, 이 클래스는 공격 쿨타임과 피해 적용만 담당합니다.
public sealed class TowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("타워가 적에게 주는 피해량입니다. BuildingTower 초기화 시 buildings.json 값으로 덮어쓸 수 있습니다.")]
    [SerializeField] private float _attackDamage = 5f;
    [Tooltip("한 번 공격한 뒤 다음 공격까지 기다리는 시간입니다.")]
    [SerializeField] private float _attackInterval = 1.4f;
    [Header("Connected Components")]
    [Tooltip("연결하면 DamageDealer를 통해 피해와 Run Result 기록을 함께 처리합니다.")]
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

        if (_damageDealer != null)
        {
            _damageDealer.SetSourceType(DamageSourceType.Tower);
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
        if (targetEnemyStatus == null)
        {
            return false;
        }

        if (targetEnemyStatus.IsDead)
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
            _damageDealer.SetSourceType(DamageSourceType.Tower);
            _damageDealer.SetDamageValue(_attackDamage);
            _damageDealer.ApplyDamage(enemyDamageable);
            return;
        }

        targetEnemyStatus.TakeDamage(_attackDamage);
    }

    private void RecordAttackTime()
    {
        _lastAttackTime = Time.time;
    }

    private void ConnectDamageDealer()
    {
        if (_damageDealer != null)
        {
            return;
        }

        _damageDealer = GetComponent<DamageDealer>();
        if (_damageDealer != null)
        {
            _damageDealer.SetSourceType(DamageSourceType.Tower);
        }
    }
}
