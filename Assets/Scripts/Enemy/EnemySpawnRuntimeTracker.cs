using System.Collections.Generic;
using UnityEngine;

// EnemySpawnPoint가 만든 적 목록을 추적합니다.
// 살아 있는 적 수 계산, 낮/밤 상태 전파, 웨이브 종료 시 잔여 적 정리에 사용합니다.
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
            GameObject enemyObject = _spawnedEnemies[i];
            if(enemyObject == null || enemyObject.activeInHierarchy == false)
            {
                continue;
            }

            EnemyBrain enemyBrain = enemyObject.GetComponent<EnemyBrain>();
            if (enemyBrain != null)
            {
                enemyBrain.SetCinderHeartChaseEnabled(isEnabled);
            }

            EnemyBehaviorState behaviorState = enemyObject.GetComponent<EnemyBehaviorState>();
            if(behaviorState != null)
            {
                behaviorState.SetMode(isEnabled ? BTEnemyMode.NightAssault : BTEnemyMode.DayWander);
            }
        }
    }

    public void SetNightTimeForAliveEnemies(bool isNightTime)
    {
        RemoveMissingEnemies();

        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            GameObject enemyObject = _spawnedEnemies[i];
            if (enemyObject == null || enemyObject.activeInHierarchy == false)
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

    private EnemyBrain GetAliveEnemyBrain(GameObject enemyObject)
    {
        if (enemyObject == null || enemyObject.activeInHierarchy == false)
        {
            return null;
        }

        return enemyObject.GetComponent<EnemyBrain>();
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
