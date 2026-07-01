using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Wander Around Spawn Point",
    story: "[Self] wanders around spawn point when state is [RequiredState]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_wander_around_spawn_point_action_v2")]
public partial class EnemyWanderAroundSpawnPointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<string> RequiredState = new BlackboardVariable<string>("DayWander");

    [SerializeReference] public BlackboardVariable<bool> StopOnEnd = new BlackboardVariable<bool>(false);

    [SerializeReference] public BlackboardVariable<bool> InterruptWhenPlayerDetected = new BlackboardVariable<bool>(true);

    private EnemyMovement _enemyMovement;
    private EnemyDetector _enemyDetector;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if (IsUnityObjectNull(selfObject))
        {
            Debug.LogWarning("EnemyWanderAroundSpawnPointAction: Self가 없습니다.");
            return Status.Failure;
        }

        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        if (_enemyMovement == null)
        {
            Debug.LogWarning("EnemyWanderAroundSpawnPointAction: Self에 EnemyMovement가 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }
        _enemyDetector = selfObject.GetComponent<EnemyDetector>();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GameObject selfObject = GetSelfObject();
        if (IsUnityObjectNull(selfObject))
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

        if (_enemyMovement == null)
        {
            _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        }

        if (_enemyMovement == null)
        {
            return Status.Failure;
        }

        _enemyMovement.WanderAroundSpawnPoint();

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (StopOnEnd == null || StopOnEnd.Value == false)
        {
            return;
        }

        if (_enemyMovement != null)
        {
            _enemyMovement.StopMoving();
        }
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

        if(_enemyDetector == null)
        {
            return false;
        }

        return _enemyDetector.HasDetectedPlayer && _enemyDetector.DetectedPlayer != null;
    }

    private bool IsRequiredStateMatched(GameObject selfObject)
    {
        string requiredStateName = RequiredState == null ? string.Empty : RequiredState.Value;
        if(string.IsNullOrWhiteSpace(requiredStateName))
        {
            return true;
        }

        EnemyBehaviorState behaviorState = selfObject.GetComponent<EnemyBehaviorState>();

        if(behaviorState == null)
        {
            Debug.LogWarning("EnemyWanderAroundSpawnPointAction: EnemyBehaviorState가 없습니다. object=" + selfObject.name);
            return false;
        }
        return behaviorState.IsCurrentState(requiredStateName);
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