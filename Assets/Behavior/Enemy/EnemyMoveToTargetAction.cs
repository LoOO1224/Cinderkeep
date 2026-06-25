using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;


[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Move To Target",
    story: "[Self] moves to [Target] until [ArriveDistance]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_move_to_target_action_v1"
    )]
public class EnemyMoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> ArriveDistance = new BlackboardVariable<float>(0.3f);

    private EnemyMovement _enemyMovement;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if(IsUnityObjectNull(selfObject))
        {
            LogFailure("EnemyMoveToTargetAction : Self가 없습니다.", true);
            return Status.Failure;
        }

        if(IsUnityObjectNull(Target == null ? null : Target.Value))
        {
            LogFailure("EnemyMoveToTargetAction : Target이 없습니다", false);
            return Status.Failure;
        }

        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        if(_enemyMovement == null)
        {
            LogFailure("EnemyMoveToTargetAction: Self에 EnemyMovement가 없습니다.", true);
            return Status.Failure;
        }

        return Status.Running;

    }

    private GameObject GetSelfObject()
    {
        if(Self == null && IsUnityObjectNull(Self.Value) == false)
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
