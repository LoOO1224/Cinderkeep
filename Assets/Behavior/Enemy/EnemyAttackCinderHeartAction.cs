using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Attack CinderHeart",
    story: "[Self] attacks [CinderHeart] within [AttackDistance] when state is [RequiredState]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_attack_cinderheart_action_v1")]

public partial class EnemyAttackCinderHeartAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> CinderHeart;
    [SerializeReference] public BlackboardVariable<float> AttackDistance = new BlackboardVariable<float>(3f);
    [SerializeReference] public BlackboardVariable<string> RequiredState = new BlackboardVariable<string>("NightAssault");
    [SerializeReference] public BlackboardVariable<bool> InterruptWhenPlayerDetected = new BlackboardVariable<bool>(true);

    private EnemyAttack _enemyAttack;
    private EnemyMovement _enemyMovement;
    private EnemyDetector _enemyDetector;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if(IsUnityObjectNull(selfObject))
        {
            Debug.LogWarning("EnemyAttackCinderHeartAction: Self가 없습니다.");
            return Status.Failure;
        }

        _enemyAttack = selfObject.GetComponent<EnemyAttack>();
        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        _enemyDetector = selfObject.GetComponent<EnemyDetector>();

        if(_enemyAttack == null)
        {
            Debug.LogWarning("EnemyAttackCinderHeartAction: Self에 EnemyAttack이 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GameObject selfObject = GetSelfObject ();
        GameObject cinderHeartObject = CinderHeart == null ? null : CinderHeart.Value;

        if (IsUnityObjectNull(selfObject) || IsUnityObjectNull(cinderHeartObject))
        {
            return Status.Failure;
        }

        if (IsRequiredStateMatched(selfObject) == false)
        {
            return Status.Failure;
        }

        if (ShouldInterruptForDetectedPlayer(selfObject))
        {
            return Status.Failure;
        }

        Damageable cinderHeartDamageable = GetDamageableFromObject(cinderHeartObject);
        if (cinderHeartDamageable == null)
        {
            return Status.Failure;
        }


        float attackDistance = AttackDistance == null ? 3f : AttackDistance.Value;
        float distance = Vector3.Distance(selfObject.transform.position, cinderHeartDamageable.transform.position);

        if(distance >  attackDistance)
        {
            return Status.Failure;
        }

        if (_enemyMovement == null)
        {
            _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        }

        if (_enemyMovement != null)
        {
            _enemyMovement.StopMoving();
        }

        if (_enemyAttack == null)
        {
            _enemyAttack = selfObject.GetComponent<EnemyAttack>();
        }

        if (_enemyAttack == null)
        {
            return Status.Failure;
        }

        _enemyAttack.TryAttack(cinderHeartDamageable);

        return Status.Running;

    }

    private bool ShouldInterruptForDetectedPlayer(GameObject selfObject)
    {
        if(InterruptWhenPlayerDetected == null || InterruptWhenPlayerDetected.Value == false)
        {
            return false;
        }

        if(_enemyDetector == null)
        {
            _enemyDetector = selfObject.GetComponent<EnemyDetector>();
        }
        if (_enemyDetector == null)
        {
            return false;
        }

        return _enemyDetector.HasDetectedPlayer && _enemyDetector.DetectedPlayer != null;

    }

    private bool IsRequiredStateMatched(GameObject selfObject)
    {
        string requiredStateName = RequiredState == null ? null : RequiredState.Value;
        if(string.IsNullOrWhiteSpace(requiredStateName))
        {
            return true;
        }
        EnemyBehaviorState behaviorState = selfObject.GetComponent<EnemyBehaviorState>();

        if(behaviorState == null)
        {
            return false;
        }

        return behaviorState.IsCurrentState(requiredStateName);
    }

    private Damageable GetDamageableFromObject(GameObject targetObject)
    {
        if(targetObject == null)
        {
            return null;
        }

        Damageable damageable = targetObject.GetComponent<Damageable>();
        if (damageable != null)
        {
            return damageable;
        }

        damageable = targetObject.GetComponentInParent<Damageable>();
        if (damageable != null)
        {
            return damageable;
        }

        damageable = targetObject.GetComponentInChildren<Damageable>(true);
        if (damageable != null)
        {
            return damageable;
        }

        Debug.LogWarning("EnemyAttackCinderHeartAction: CinderHeart 또는 부모/자식에서 Damageable을 찾지 못했습니다. target=" + targetObject.name);
        return null;
    }


    private GameObject GetSelfObject()
    {
        if (Self != null && IsUnityObjectNull(Self.Value) == false)
        {
            return Self.Value;
        }

        return GameObject;
    }

    private static bool IsUnityObjectNull(UnityEngine.Object targetObject)
    {
        return ReferenceEquals(targetObject, null) || targetObject == null;
    }
}
