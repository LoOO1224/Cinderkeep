using Cinderkeep.Gameplay;
using UnityEngine;

// 몬스터의 실제 공격 실행을 담당하는 컴포넌트입니다.
// 공격 대상 판단은 EnemyBrain이 맡고, 이 클래스는 쿨타임과 피해 적용만 처리합니다.
public sealed class EnemyAttack : MonoBehaviour
{
    private const string CinderHeartTag = "CinderHeart";

    [Tooltip("플레이어나 일반 피해 대상에게 줄 기본 공격 피해량입니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _attackDamage;
    [Tooltip("CinderHeart를 공격할 때 사용할 피해량입니다.")]
    [SerializeField] private float _cinderHeartAttackDamage = 10f;
    [Tooltip("공격 후 다음 공격까지 기다리는 시간입니다. EnemyData로 초기화됩니다.")]
    [SerializeField] private float _attackInterval = 1f;

    private float _lastAttackTime;

    public float AttackDamage
    {
        get
        {
            return _attackDamage;
        }
    }

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        _attackDamage = enemyData.AttackDamage;
        _attackInterval = enemyData.AttackInterval;
    }

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + _attackInterval;
    }

    public void RecordAttackTime()
    {
        _lastAttackTime = Time.time;
    }

    public bool TryAttack(Damageable targetDamageable)
    {
        if (targetDamageable == null)
        {
            return false;
        }

        if (CanAttack() == false)
        {
            return false;
        }

        float attackDamage = GetAttackDamage(targetDamageable);
        targetDamageable.TakeDamage(attackDamage);
        RecordAttackTime();
        return true;
    }

    private float GetAttackDamage(Damageable targetDamageable)
    {
        if (targetDamageable.CompareTag(CinderHeartTag))
        {
            return _cinderHeartAttackDamage;
        }

        return _attackDamage;
    }
}
