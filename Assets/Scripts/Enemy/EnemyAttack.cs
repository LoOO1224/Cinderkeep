using UnityEngine;
using Cinderkeep.Gameplay;

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
}
