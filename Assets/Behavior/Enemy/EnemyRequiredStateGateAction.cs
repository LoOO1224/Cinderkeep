using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Required State Gate",
    story: "[Self] passes when state is [RequiredState]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_required_state_gate_action")]
// Behavior Graph 게이트 액션입니다. EnemyBehaviorState가 RequiredState와 일치할 때만 Success를 반환합니다.
// 낮/밤 행동 분기를 Behavior Graph 노드 레벨에서 제어할 때 사용합니다.
public partial class EnemyRequiredStateGateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<string> RequiredState = new BlackboardVariable<string>("NightAssault");

    protected override Status OnStart()
    {
        return CheckState();
    }

    protected override Status OnUpdate()
    {
        return CheckState();
    }

    private Status CheckState()
    {
        GameObject selfObject = GetSelfObject();
        if (IsUnityObjectNull(selfObject))
        {
            Debug.LogWarning("EnemyRequiredStateGateAction: Self가 없습니다.");
            return Status.Failure;
        }

        EnemyBehaviorState behaviorState = selfObject.GetComponent<EnemyBehaviorState>();
        if (behaviorState == null)
        {
            Debug.LogWarning("EnemyRequiredStateGateAction: EnemyBehaviorState가 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        string requiredStateName = RequiredState == null ? string.Empty : RequiredState.Value;
        bool isMatched = behaviorState.IsCurrentState(requiredStateName);

        return isMatched ? Status.Success : Status.Failure;
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