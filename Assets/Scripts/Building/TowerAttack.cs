using Unity.VisualScripting;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("타워가 적에게 주는 피해량입니다. buildings.json의 attackDamage로 덮어씁니다.")]
    [SerializeField] private float _attackDamage = 5f;
    [Tooltip("공격 후 다음 공격까지 기다리는 시간입니다. buildings.json의 attackInterval로 덮어씁니다.")]
    [SerializeField] private float _attackDelay = 1.4f;
    [Header("Connected Components")]
    [Tooltip("선택 사항입니다. 연결하면 DamageDealer를 통해 피해를 전달합니다. 없으면 TakeDamage 직접 사용")]
    [SerializeField] private DamageDealer _damageDealer;

    private float _lastAttackTime;

    public float AttackDamage { get { return _attackDamage; } }

    private void Awake()
    {
        ConnectDamageDealer();
    }

    // 공격력 설정(BuildingTower.Initialize에서 호출예정)
    public void SetAttackDamage(float attackDamage)
    {
        _attackDamage = Mathf.Max(0f, attackDamage);

        if(_damageDealer != null)
        {
            _damageDealer.SetDamageValue(_attackDamage);
        }
    }

    // 공격 딜레이 설정
    public void SetAttackDelay(float attackDelay)
    {
        _attackDelay = Mathf.Max(0.1f, attackDelay);
    }

    // 쿨타임 돌았는지 확인
    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + _attackDelay;
    }

    // 여러 상황들 유효 검사
    public bool TryAttack(EnemyStatus targetEnemyStatus)
    {
        if(targetEnemyStatus == null)
        {
            return false;
        }

        if (targetEnemyStatus.IsDead)
        {
            return false;
        }

        if(CanAttack() == false)
        {
            return false;
        }

        ApplyDamageToEnemy(targetEnemyStatus);
        RecordAttackTime();
        return true;
    }

    private void ApplyDamageToEnemy(EnemyStatus targetEnemyStatus)
    {
        // 적 체력의 기준이 EnemyStatus이니까 여기로 피해를 줍니다
        Damageable enemyDamageable = targetEnemyStatus.GetComponent<Damageable>();
        if (_damageDealer != null && enemyDamageable != null)
        {
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
        if(_damageDealer != null)
        {
            return;
        }

        _damageDealer = GetComponent<DamageDealer>();
    }

}
