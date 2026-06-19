using Cinderkeep.Gameplay;
using UnityEngine;

// GameFlowController가 알려준 현재 페이즈와 일차를 기준으로 적 스폰 지점들을 지휘합니다.
// 실제 적 생성은 EnemySpawnPoint가 담당하고, 이 클래스는 어떤 스폰 지점을 켜고 끌지만 결정합니다.
public sealed class GameFlowEnemySpawnDirector : MonoBehaviour
{
    [Header("Enemy Spawn")]
    [Tooltip("GameFlowController의 현재 페이즈에 맞춰 켜고 끌 적 스폰 지점 목록입니다.")]
    [SerializeField] private EnemySpawnPoint[] _enemySpawnPoints;

    private GameObjectManager _gameObjectManager;
    private EnemyLoopConnector _enemyLoopConnector;

    public void Initialize(GameObjectManager gameObjectManager, EnemyLoopConnector enemyLoopConnector)
    {
        _gameObjectManager = gameObjectManager;
        _enemyLoopConnector = enemyLoopConnector;
        InitializeEnemySpawnPoints();
    }

    public void StartSpawn(EnemySpawnMode spawnMode, int day)
    {
        if (_enemySpawnPoints == null)
        {
            return;
        }

        EnemySpawnStep spawnStep = GetSpawnStep(spawnMode, day);
        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            EnemySpawnPoint spawnPoint = _enemySpawnPoints[i];
            if (spawnPoint == null)
            {
                continue;
            }

            spawnPoint.SetSpawnStep(spawnStep);
            spawnPoint.SetSpawnPointActive(true);
            spawnPoint.SetSpawnMode(spawnMode, day);
        }
    }

    public void StopSpawn()
    {
        if (_enemySpawnPoints == null)
        {
            return;
        }

        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            EnemySpawnPoint spawnPoint = _enemySpawnPoints[i];
            if (spawnPoint == null)
            {
                continue;
            }

            spawnPoint.SetSpawnPointActive(false);
        }
    }

    private void InitializeEnemySpawnPoints()
    {
        if (_enemySpawnPoints == null)
        {
            return;
        }

        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            EnemySpawnPoint spawnPoint = _enemySpawnPoints[i];
            if (spawnPoint == null)
            {
                continue;
            }

            spawnPoint.Initialize(_gameObjectManager, _enemyLoopConnector);
        }
    }

    private EnemySpawnStep GetSpawnStep(EnemySpawnMode spawnMode, int day)
    {
        if (spawnMode == EnemySpawnMode.Boss)
        {
            return EnemySpawnStep.Step3;
        }

        if (day <= 1)
        {
            return EnemySpawnStep.Step1;
        }

        if (day == 2)
        {
            return EnemySpawnStep.Step2;
        }

        return EnemySpawnStep.Step3;
    }
}
