using System;
using UnityEngine;

// EnemyBrain에서 쓰는 타깃 선택 전용 유틸입니다.
// 최근 공격자, 가까운 Damageable처럼 순수 선택 규칙만 계산하고 이동/공격 실행은 담당하지 않습니다.
public static class EnemyTargetSelector
{
    public static Damageable SelectNearestRecentAttacker(
        Transform origin,
        Damageable recentPlayerAttacker,
        float lastPlayerAttackedTime,
        Damageable recentTowerAttacker,
        float lastTowerAttackedTime,
        float memoryDuration)
    {
        Damageable validPlayerAttacker = GetValidRecentAttacker(recentPlayerAttacker, lastPlayerAttackedTime, memoryDuration);
        Damageable validTowerAttacker = GetValidRecentAttacker(recentTowerAttacker, lastTowerAttackedTime, memoryDuration);

        if (validPlayerAttacker == null)
        {
            return validTowerAttacker;
        }

        if (validTowerAttacker == null)
        {
            return validPlayerAttacker;
        }

        if (origin == null)
        {
            return validPlayerAttacker;
        }

        float playerDistanceSqr = (validPlayerAttacker.transform.position - origin.position).sqrMagnitude;
        float towerDistanceSqr = (validTowerAttacker.transform.position - origin.position).sqrMagnitude;
        return playerDistanceSqr <= towerDistanceSqr ? validPlayerAttacker : validTowerAttacker;
    }

    public static Damageable SelectNearestDamageable(
        Vector3 originPosition,
        Collider[] overlapColliders,
        int hitCount,
        Func<Collider, Damageable> resolveDamageable)
    {
        if (overlapColliders == null || resolveDamageable == null || hitCount <= 0)
        {
            return null;
        }

        Damageable nearestDamageable = null;
        float nearestDistanceSqr = float.MaxValue;
        int safeHitCount = Mathf.Min(hitCount, overlapColliders.Length);
        for (int i = 0; i < safeHitCount; i++)
        {
            Damageable damageable = resolveDamageable(overlapColliders[i]);
            if (damageable == null)
            {
                continue;
            }

            float distanceSqr = (damageable.transform.position - originPosition).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearestDistanceSqr = distanceSqr;
            nearestDamageable = damageable;
        }

        return nearestDamageable;
    }

    private static Damageable GetValidRecentAttacker(Damageable attackerDamageable, float lastAttackedTime, float memoryDuration)
    {
        if (attackerDamageable == null)
        {
            return null;
        }

        if (Time.time > lastAttackedTime + memoryDuration)
        {
            return null;
        }

        if (attackerDamageable.gameObject.activeInHierarchy == false)
        {
            return null;
        }

        return attackerDamageable;
    }
}
