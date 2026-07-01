using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Enemy Attack Detected Player",
    story: "[Self] attacks detected player within [AttackDistance]",
    category: "Action/Cinderkeep/Enemy",
    id: "cinderkeep_enemy_attack_detected_player_action_v1")]
public partial class EnemyAttackDetectedPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    // 기존 EnemyBrain의 기본 _attackDistance 값과 맞춥니다.
    [SerializeReference] public BlackboardVariable<float> AttackDistance = new BlackboardVariable<float>(2.3f);

    private EnemyDetector _enemyDetector;
    private EnemyAttack _enemyAttack;
    private EnemyMovement _enemyMovement;

    protected override Status OnStart()
    {
        GameObject selfObject = GetSelfObject();
        if (IsUnityObjectNull(selfObject))
        {
            Debug.LogWarning("EnemyAttackDetectedPlayerAction: Self가 없습니다.");
            return Status.Failure;
        }

        _enemyDetector = selfObject.GetComponent<EnemyDetector>();
        if (_enemyDetector == null)
        {
            Debug.LogWarning("EnemyAttackDetectedPlayerAction: Self에 EnemyDetector가 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        _enemyAttack = selfObject.GetComponent<EnemyAttack>();
        if (_enemyAttack == null)
        {
            Debug.LogWarning("EnemyAttackDetectedPlayerAction: Self에 EnemyAttack이 없습니다. object=" + selfObject.name);
            return Status.Failure;
        }

        _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        if (_enemyMovement == null)
        {
            Debug.LogWarning("EnemyAttackDetectedPlayerAction: Self에 EnemyMovement가 없습니다. object=" + selfObject.name);
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

        RefreshComponentsIfNeeded(selfObject);

        if(_enemyDetector == null || _enemyAttack == null)
        {
            return Status.Failure;
        }

        Transform detectedPlayerTransform = _enemyDetector.DetectedPlayer;
        if(_enemyDetector.HasDetectedPlayer == false || detectedPlayerTransform == null)
        {
            return Status.Failure;
        }
        Damageable playerDamageable = GetDamageableFromTransform(detectedPlayerTransform);
        if(playerDamageable == null)
        {
            return Status.Failure;
        }

        if (IsInAttackDistance(selfObject.transform, detectedPlayerTransform) == false)
        {
            return Status.Failure;
        }
        if(_enemyMovement != null)
        {
            _enemyMovement.StopMoving();
        }
        _enemyAttack.TryAttack(playerDamageable);

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }







    private void RefreshComponentsIfNeeded(GameObject selfObject)
    {
        if (_enemyDetector == null)
        {
            _enemyDetector = selfObject.GetComponent<EnemyDetector>();
        }

        if (_enemyAttack == null)
        {
            _enemyAttack = selfObject.GetComponent<EnemyAttack>();
        }

        if (_enemyMovement == null)
        {
            _enemyMovement = selfObject.GetComponent<EnemyMovement>();
        }
    }

    private bool IsInAttackDistance(Transform selfTransform, Transform targetTransform)
    {
        if(selfTransform == null || targetTransform == null)
        {
            return false;
        }
        float attackDistance = AttackDistance == null ? 2.3f : AttackDistance.Value;
        float distance = Vector3.Distance(selfTransform.position, targetTransform.position);
        return distance <= attackDistance;
    }

    private Damageable GetDamageableFromTransform(Transform targetTransform)
    {
        if(targetTransform == null)
        {
            return null;
        }
        Damageable damageable = targetTransform.GetComponent<Damageable>();
        if(damageable != null)
        {
            return damageable;
        }
        return targetTransform.GetComponentInParent<Damageable>();
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