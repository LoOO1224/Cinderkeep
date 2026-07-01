using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Move To Blocking Building",
    story: "[Self] moves to blocking building toward [CinderHeart]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_move_to_blocking_building_action")]
public partial class EnemyMoveToBlockingBuildingAction : Action
{
    private const string BuildTag = "Build";

    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> CinderHeart;

    [SerializeReference] public BlackboardVariable<float> BlockingBuildingDetectDistance = new BlackboardVariable<float>(5f);

    [SerializeReference] public BlackboardVariable<float> BlockingBuildingDetectRadius = new BlackboardVariable<float>(1f);

    [SerializeReference] public BlackboardVariable<string> RequiredState = new BlackboardVariable<string>("NightAssault");

    [SerializeReference] public BlackboardVariable<bool> InterruptWhenPlayerDetected = new BlackboardVariable<bool>(true);

    private EnemyMovement _enemyMovement;
    private EnemyDetector _enemyDetector;
    private NavMeshPath _path;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if(IsUnityObjectNull(selfObject))
        {
            Debug.LogWarning("EnemyMoveToBlockingBuildingAction: Self가 없습니다.");
            return Status.Failure;
        }
        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        _enemyDetector = selfObject.GetComponent<EnemyDetector>();
        _path = new NavMeshPath();

        if(_enemyMovement == null)
        {
            Debug.LogWarning("EnemyMoveToBlockingBuildingAction: Self에 EnemyMovement가 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GameObject selfObject = GetSelfObject();
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
        BuildingHp blockingBuilding = FindBlockingBuilding(selfObject, cinderHeartObject);
        if (blockingBuilding == null)
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

        _enemyMovement.MoveToTarget(blockingBuilding.transform);

        return Status.Running;

    }

    private BuildingHp FindBlockingBuilding(GameObject selfObject, GameObject cinderHeartObject)
    {
        if (_path == null)
        {
            _path = new NavMeshPath();
        }

        bool isPathBlocked = EnemyPathQuery.IsPathBlocked(
            selfObject.transform,
            cinderHeartObject.transform,
            _path);

        if (isPathBlocked == false)
        {
            return null;
        }

        float detectDistance = BlockingBuildingDetectDistance == null ? 5f : BlockingBuildingDetectDistance.Value;
        float detectRadius = BlockingBuildingDetectRadius == null ? 1f : BlockingBuildingDetectRadius.Value;

        return EnemyPathQuery.FindBlockingBuilding(
            selfObject.transform.position,
            cinderHeartObject.transform.position,
            detectRadius,
            detectDistance,
            BuildTag);
    }

    private bool ShouldInterruptForDetectedPlayer(GameObject selfObject)
    {
        if (InterruptWhenPlayerDetected == null || InterruptWhenPlayerDetected.Value == false)
        {
            return false;
        }

        if (_enemyDetector == null)
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
        string requiredStateName = RequiredState == null ? string.Empty : RequiredState.Value;
        if (string.IsNullOrWhiteSpace(requiredStateName))
        {
            return true;
        }

        EnemyBehaviorState behaviorState = selfObject.GetComponent<EnemyBehaviorState>();
        if (behaviorState == null)
        {
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
