using Cinderkeep.Gameplay;
using UnityEngine;

// 몬스터의 실제 공격 실행을 담당하는 컴포넌트입니다.
// 공격 대상 판단은 EnemyBrain이 맡고, 이 클래스는 쿨타임과 피해 적용만 처리합니다.
public sealed class EnemyAttack : MonoBehaviour
{
    [SerializeField] private float _attackDamage;
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

        targetDamageable.TakeDamage(_attackDamage);
        RecordAttackTime();
        return true;
    }
}
