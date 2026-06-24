using Cinderkeep.Gameplay;
using System;
using UnityEngine;

// GameFlowController가 알려준 현재 페이즈와 일차를 기준으로 EnemySpawnPoint들을 지휘합니다.
// 실제 적 생성은 EnemySpawnPoint가 담당하고, 이 클래스는 어떤 스폰 지점을 켜고 끌지 결정합니다.
public sealed class GameFlowEnemySpawnDirector : MonoBehaviour
{
    [Header("Enemy Spawn")]
    [Tooltip("GameFlowController의 현재 페이즈에 맞춰 켜고 끌 EnemySpawnPoint 목록입니다.")]
    [SerializeField] private EnemySpawnPoint[] _enemySpawnPoints;

    private GameObjectManager _gameObjectManager;
    private EnemyLoopConnector _enemyLoopConnector;
    private Action<EnemyStatus> _bossDefeatedHandler;

    public void Initialize(GameObjectManager gameObjectManager, EnemyLoopConnector enemyLoopConnector)
    {
        _gameObjectManager = gameObjectManager;
        _enemyLoopConnector = enemyLoopConnector;
        InitializeEnemySpawnPoints();
    }

    public void StartSpawn(EnemySpawnMode spawnMode, int day)
    {
        StartSpawn(spawnMode, day, null);
    }

    public void StartSpawn(EnemySpawnMode spawnMode, int day, Action<EnemyStatus> bossDefeatedHandler)
    {
        if (_enemySpawnPoints == null)
        {
            return;
        }

        _bossDefeatedHandler = bossDefeatedHandler;
        EnemySpawnStep spawnStep = GetSpawnStep(spawnMode, day);
        bool bossSpawnPointAssigned = false;
        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            EnemySpawnPoint spawnPoint = _enemySpawnPoints[i];
            if (spawnPoint == null)
            {
                continue;
            }

            bool shouldActivate = true;
            if (spawnMode == EnemySpawnMode.Boss)
            {
                shouldActivate = bossSpawnPointAssigned == false && spawnPoint.HasBossSpawnCandidate();
                if (shouldActivate)
                {
                    bossSpawnPointAssigned = true;
                }

                spawnPoint.SetBossDefeatedHandler(_bossDefeatedHandler);
            }
            else
            {
                spawnPoint.SetBossDefeatedHandler(null);
            }

            spawnPoint.SetSpawnStep(spawnStep);
            spawnPoint.SetSpawnPointActive(shouldActivate);
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
            spawnPoint.SetBossDefeatedHandler(null);
            spawnPoint.ClearSpawnedEnemies();
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
