using Cinderkeep.Gameplay;
using UnityEngine;

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

    // 공격 쿨타임이 끝났는지 확인합니다.
    // 실제 공격 실행은 TryAttack에서 처리합니다.
    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + _attackInterval;
    }

    public void RecordAttackTime()
    {
        _lastAttackTime = Time.time;
    }

    // 몬스터가 공격 가능한 상태라면 대상에게 피해를 줍니다.
    // targetHealth 같은 숫자만 받으면 값 복사라 실제 체력이 줄지 않기 때문에 Damageable을 받습니다.
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
