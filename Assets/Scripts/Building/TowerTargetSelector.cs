using System;
using UnityEngine;

// TowerTargeting에서 쓰는 적 선택 전용 스크립트입니다.
// EnemyTargetSelector와 같은 역할이며, 이동/공격 실행은 담당하지 않습니다.
public static class TowerTargetSelector
{
    // 후보 중 가장 가까운 적 1명 선택
    public static EnemyStatus SelectNearestEnemyStatus(
        Vector3 originPosition,
        Collider[] overlapColliders,
        int hitCount,
        Func<Collider, EnemyStatus> resolveEnemyStatus)
    {
        if (overlapColliders == null || resolveEnemyStatus == null || hitCount <= 0)
        {
            return null;
        }

        EnemyStatus nearestEnemyStatus = null;
        float nearestDistanceSqr = float.MaxValue;
        int safeHitCount = Mathf.Min(hitCount, overlapColliders.Length);

        for (int i = 0; i < safeHitCount; i++)
        {
            EnemyStatus enemyStatus = resolveEnemyStatus(overlapColliders[i]);
            if (enemyStatus == null)
            {
                continue;
            }

            float distanceSqr = (enemyStatus.transform.position - originPosition).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearestDistanceSqr = distanceSqr;
            nearestEnemyStatus = enemyStatus;
        }

        return nearestEnemyStatus;
    }
}