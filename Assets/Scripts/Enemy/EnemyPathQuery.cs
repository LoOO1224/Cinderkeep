using UnityEngine;
using UnityEngine.AI;

// EnemyBrain에서 쓰는 NavMesh 경로 검사와 막는 건축물 탐색을 담당합니다.
// AI 판단 순서는 Brain이 유지하고, 경로 계산 세부 구현은 이 helper로 분리합니다.
public static class EnemyPathQuery
{
    public static bool CanReachTarget(Transform originTransform, Transform targetTransform, NavMeshPath path)
    {
        if (originTransform == null || targetTransform == null || path == null)
        {
            return false;
        }

        NavMeshHit originOnNavMesh;
        if (NavMesh.SamplePosition(originTransform.position, out originOnNavMesh, 2f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        NavMeshHit targetOnNavMesh;
        if (NavMesh.SamplePosition(targetTransform.position, out targetOnNavMesh, 4f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        bool hasPath = NavMesh.CalculatePath(
            originOnNavMesh.position,
            targetOnNavMesh.position,
            NavMesh.AllAreas,
            path);

        return hasPath && path.status == NavMeshPathStatus.PathComplete;
    }

    public static bool IsPathBlocked(Transform originTransform, Transform targetTransform, NavMeshPath path)
    {
        if (originTransform == null || targetTransform == null || path == null)
        {
            return false;
        }

        NavMeshHit originOnNavMesh;
        if (NavMesh.SamplePosition(originTransform.position, out originOnNavMesh, 2f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        NavMeshHit targetOnNavMesh;
        if (NavMesh.SamplePosition(targetTransform.position, out targetOnNavMesh, 4f, NavMesh.AllAreas) == false)
        {
            return false;
        }

        bool hasPath = NavMesh.CalculatePath(
            originOnNavMesh.position,
            targetOnNavMesh.position,
            NavMesh.AllAreas,
            path);

        if (hasPath == false)
        {
            return true;
        }

        return path.status == NavMeshPathStatus.PathPartial ||
               path.status == NavMeshPathStatus.PathInvalid;
    }

    public static BuildingHp FindBlockingBuilding(
        Vector3 originPosition,
        Vector3 targetPosition,
        float detectRadius,
        float detectDistance,
        string buildTag)
    {
        Vector3 direction = targetPosition - originPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return null;
        }

        Vector3 rayOrigin = originPosition + Vector3.up * 0.8f;
        RaycastHit[] hits = Physics.SphereCastAll(
            rayOrigin,
            detectRadius,
            direction.normalized,
            detectDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            BuildingHp buildingHp = GetBuildingHpFromCollider(hits[i].collider, buildTag);
            if (buildingHp != null)
            {
                return buildingHp;
            }
        }

        return null;
    }

    private static BuildingHp GetBuildingHpFromCollider(Collider hitCollider, string buildTag)
    {
        if (hitCollider == null)
        {
            return null;
        }

        BuildingHp buildingHp = hitCollider.GetComponentInParent<BuildingHp>();
        if (buildingHp == null || buildingHp.IsDestroyed)
        {
            return null;
        }

        if (hitCollider.CompareTag(buildTag))
        {
            return buildingHp;
        }

        if (buildingHp.CompareTag(buildTag))
        {
            return buildingHp;
        }

        return null;
    }
}
