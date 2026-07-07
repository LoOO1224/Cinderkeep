using System.Collections.Generic;
using UnityEngine;

// EnemySpawnPoint가 만든 적 목록을 추적합니다.
// 생존 적 계산, 낮/밤 상태 전파, 웨이브 종료 정리에 사용합니다.
public sealed class EnemySpawnRuntimeTracker
{
    private readonly List<GameObject> _spawnedEnemies = new List<GameObject>();

    public void RegisterEnemy(GameObject enemyObject)
    {
        if (enemyObject == null)
        {
            return;
        }

        _spawnedEnemies.Add(enemyObject);
    }

    public void SetCinderHeartChaseEnabledForAliveEnemies(bool isEnabled)
    {
        RemoveMissingEnemies();

        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            GameObject enemyObject = GetAliveEnemyObject(_spawnedEnemies[i]);
            if (enemyObject == null)
            {
                continue;
            }

            EnemyBrain enemyBrain = enemyObject.GetComponent<EnemyBrain>();
            if (enemyBrain != null)
            {
                enemyBrain.SetCinderHeartChaseEnabled(isEnabled);
            }

            ApplyBehaviorModeToEnemy(enemyObject, isEnabled);
        }
    }

    public void SetNightTimeForAliveEnemies(bool isNightTime)
    {
        RemoveMissingEnemies();

        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            GameObject enemyObject = GetAliveEnemyObject(_spawnedEnemies[i]);
            if (enemyObject == null)
            {
                continue;
            }

            EnemyBrain enemyBrain = enemyObject.GetComponent<EnemyBrain>();
            if (enemyBrain != null)
            {
                enemyBrain.SetNightTime(isNightTime);
            }

            EnemyDetector enemyDetector = enemyObject.GetComponent<EnemyDetector>();
            if (enemyDetector != null)
            {
                enemyDetector.SetNightDetectionEnabled(isNightTime);
            }

            ApplyBehaviorModeToEnemy(enemyObject, isNightTime);
        }
    }

    public int GetAliveEnemyCount()
    {
        RemoveMissingEnemies();

        int aliveCount = 0;
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            GameObject enemyObject = _spawnedEnemies[i];
            if (enemyObject != null && enemyObject.activeInHierarchy)
            {
                aliveCount++;
            }
        }

        return aliveCount;
    }

    public void DestroyTrackedEnemies()
    {
        DestroyTrackedEnemies(null);
    }

    public void DestroyTrackedEnemies(Cinderkeep.Gameplay.GameObjectManager gameObjectManager)
    {
        for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemyObject = _spawnedEnemies[i];
            if (enemyObject != null)
            {
                if (gameObjectManager != null)
                {
                    gameObjectManager.UnregisterGameObject(enemyObject);
                }

                DestroyEnemyObject(enemyObject);
            }
        }

        _spawnedEnemies.Clear();
    }

    public void Clear()
    {
        _spawnedEnemies.Clear();
    }

    private void ApplyBehaviorModeToEnemy(GameObject enemyObject, bool canChaseCinderHeart)
    {
        Component behaviorState = enemyObject.GetComponent("EnemyBehaviorState");
        if (behaviorState == null)
        {
            return;
        }

        string methodName = canChaseCinderHeart ? "SetNightAssaultMode" : "SetDayWanderMode";
        behaviorState.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
    }

    private void RemoveMissingEnemies()
    {
        for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (_spawnedEnemies[i] == null)
            {
                _spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private GameObject GetAliveEnemyObject(GameObject enemyObject)
    {
        if (enemyObject == null || enemyObject.activeInHierarchy == false)
        {
            return null;
        }

        return enemyObject;
    }

    private void DestroyEnemyObject(GameObject enemyObject)
    {
        if (Application.isPlaying)
        {
            Object.Destroy(enemyObject);
            return;
        }

        Object.DestroyImmediate(enemyObject);
    }
}
