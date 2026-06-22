using System.Collections.Generic;
using UnityEngine;

// 한 스폰 지점이 만든 적 목록을 추적하는 클래스입니다.
// EnemySpawnPoint가 스폰 규칙을 판단할 때 현재 살아 있는 적 수를 확인할 수 있게 돕습니다.
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
            if (enemyObject == null || enemyObject.activeInHierarchy == false)
            {
                continue;
            }

            EnemyBrain enemyBrain = enemyObject.GetComponent<EnemyBrain>();
            if (enemyBrain != null)
            {
                enemyBrain.SetCinderHeartChaseEnabled(isEnabled);
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
            if (enemyObject != null && enemyObject.activeInHierarchy == true)
            {
                aliveCount++;
            }
        }

        return aliveCount;
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
}
