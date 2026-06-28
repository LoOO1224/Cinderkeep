using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Move To Detected Player",
    story: "[Self] moves to detected Player",
    category:"Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_move_to_detected_player_action"
    )]

public partial class EnemyMoveToDetectedPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private EnemyDetector _enemyDetector;
    private EnemyMovement _enemyMovement;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if (IsUnityObjectNull(selfObject))
        {
            Debug.LogWarning("EnemyMoveToDetectedPlayerAction: Self가 없습니다.");
            return Status.Failure;
        }

        _enemyDetector = selfObject.GetComponent<EnemyDetector>();
        if (_enemyDetector == null)
        {
            Debug.LogWarning("EnemyMoveToDetectedPlayerAction: Self에 EnemyDetector가 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        if (_enemyMovement == null)
        {
            Debug.LogWarning("EnemyMoveToDetectedPlayerAction: Self에 EnemyMovement가 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GameObject selfObject = GetSelfObject();
        if (IsUnityObjectNull(selfObject))
        {
            return Status.Failure;
        }

        if (_enemyDetector == null)
        {
            _enemyDetector = selfObject.GetComponent<EnemyDetector>();
        }

        if (_enemyMovement == null)
        {
            _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        }

        if (_enemyDetector == null || _enemyMovement == null)
        {
            return Status.Failure;
        }

        if (_enemyDetector.HasDetectedPlayer == false || _enemyDetector.DetectedPlayer == null)
        {
            return Status.Failure;
        }

        _enemyMovement.MoveToTarget(_enemyDetector.DetectedPlayer);

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