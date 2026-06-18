using UnityEngine;
using Cinderkeep.Gameplay;

public class EnemyBrain : MonoBehaviour
{
    private EnemyAttack _enemyAttack;
    private EnemyMovement _enemyMovement;   // 추후 이동 판단 관련 추가 가능성 열어둠
    private Damageable _currentTarget;
    private bool _attacked;

    public void Awake()
    {
        _enemyAttack = GetComponent<EnemyAttack>();
        _enemyMovement = GetComponent<EnemyMovement>();
    }

    // 추후 EnemyDetector의 거리계산 로직 불러와서 사용할 예정
    private void EnemyDetectorDistanceLogic()
    {
        if (_currentTarget == null) return;

        DecideAttackTarget(_currentTarget.gameObject);

        if (_attacked)
        {
            // 공격 성공 시 로직
        }
    }

    private void DecideAttackTarget(GameObject targetObject)
    {
        if (targetObject == null) return;

        var damageable = targetObject.GetComponent<Damageable>();
        if (damageable == null) return;

        if (targetObject.CompareTag("Build"))
        {
            if (_enemyAttack.TryAttack(damageable))
            {
                _attacked = true;
            }
        }

        else if (targetObject.CompareTag("Player"))
        {
            if (_enemyAttack.TryAttack(damageable))
            {
                _attacked = true;
            }
        }

        else
        {
            _attacked = false;
        }
    }
}
