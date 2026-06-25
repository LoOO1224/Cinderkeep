using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;


[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Move To Target",
    story: "[Self] moves to [Target]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_move_to_target_action_v1")]
public partial class EnemyMoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private EnemyMovement _enemyMovement;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if (IsUnityObjectNull(selfObject))
        {
            LogFailure("EnemyMoveToTargetAction: Self가 없습니다.", true);
            return Status.Failure;
        }

        GameObject targetObject = Target == null ? null : Target.Value;
        if (IsUnityObjectNull(targetObject))
        {
            LogFailure("EnemyMoveToTargetAction: Target이 없습니다.", false);
            return Status.Failure;
        }

        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        if (_enemyMovement == null)
        {
            LogFailure("EnemyMoveToTargetAction: Self에 EnemyMovement가 없습니다.", true);
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GameObject selfObject = GetSelfObject();
        GameObject targetObject = Target == null ? null : Target.Value;

        if (IsUnityObjectNull(selfObject) || IsUnityObjectNull(targetObject))
        {
            return Status.Failure;
        }

        if (_enemyMovement == null)
        {
            _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        }

        if (_enemyMovement == null)
        {
            return Status.Failure;
        }

       
        _enemyMovement.MoveToTarget(targetObject.transform);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        
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