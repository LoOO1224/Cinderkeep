using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Attack Blocking Building",
    story: "[Self] attacks blocking building toward [CinderHeart] within [AttackDistance]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_attack_blocking_building_action")]
public partial class EnemyAttackBlockingBuildingAction : Action
{
    private const string BuildTag = "Build";
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> CinderHeart;

    [SerializeReference] public BlackboardVariable<float> AttackDistance = new BlackboardVariable<float>(2.3f);
    [SerializeReference] public BlackboardVariable<float> BlockingBuildingDetectDistance = new BlackboardVariable<float>(5f);
    [SerializeReference] public BlackboardVariable<float> BlockingBuildingDetectRadius = new BlackboardVariable<float>(1f);
    [SerializeReference] public BlackboardVariable<string> RequiredState = new BlackboardVariable<string>("NightAssault");

    [SerializeReference] public BlackboardVariable<bool> InterruptWhenPlayerDetected = new BlackboardVariable<bool>(true);

    private EnemyAttack _enemyAttack;
    private EnemyMovement _enemyMovement;
    private EnemyDetector _enemyDetector;
    private NavMeshPath _path;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if(IsUnityObjectNull(selfObject))
        {
            Debug.LogWarning("EnemyAttackBlockingBuildingAction: Self가 없습니다.");
            return Status.Failure;
        }

        _enemyAttack = selfObject.GetComponent<EnemyAttack>();
        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        _enemyDetector = selfObject.GetComponent<EnemyDetector>();
        _path = new NavMeshPath();

        if(_enemyAttack == null)
        {
            Debug.LogWarning("EnemyAttackBlockingBuildingAction: Self에 EnemyAttack이 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GameObject selfObject = GetSelfObject();
        GameObject cinderHeartObject = CinderHeart == null ? null : CinderHeart.Value;

        if(IsUnityObjectNull(selfObject) || IsUnityObjectNull(cinderHeartObject))
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
        if(blockingBuilding == null)
        {
            return Status.Failure;
        }

        float attackDistance = AttackDistance == null ? 2.3f : AttackDistance.Value;
        float distance = Vector3.Distance(selfObject.transform.position, blockingBuilding.transform.position);

        if(distance > attackDistance)
        {
            return Status.Failure;
        }

        if(_enemyMovement == null)
        {
            _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        }

        if(_enemyMovement != null)
        {
            _enemyMovement.StopMoving();
        }

        if(_enemyAttack == null)
        {
            _enemyAttack = selfObject.GetComponent<EnemyAttack>();
        }
        if(_enemyAttack == null)
        {
            return Status.Failure;
        }

        _enemyAttack.TryAttack(blockingBuilding);

        return Status.Running;
    }

    private BuildingHp FindBlockingBuilding(GameObject selfObject, GameObject cinderHeartObject)
    {
        if(_path == null)
        {
            _path = new NavMeshPath();
        }

        bool isPathBlocked = EnemyPathQuery.IsPathBlocked(
            selfObject.transform,
            cinderHeartObject.transform,
            _path);
        if(isPathBlocked == false)
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
