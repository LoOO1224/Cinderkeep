// ==================================================
// FILE: Assets/Behavior/Enemy/EnemyBehaviorTestInitializer.cs
// ==================================================

using System.Collections;
using Cinderkeep.Gameplay;
using UnityEngine;

// 변경점:
// Behavior Graph 이동 테스트를 위해 씬에 직접 배치한 Enemy를 강제로 초기화하는 테스트용 스크립트입니다.
//
// 사용 목적:
// - EnemyLoopConnector를 거쳐 스폰되지 않은 Enemy도 EnemyMovement.Initialize()가 호출되도록 합니다.
// - EnemyMovement 내부의 _isInitialized가 true가 되지 않아 MoveToTarget()이 바로 return되는 문제를 해결합니다.
//
// 주의:
// - 이 스크립트는 테스트용입니다.
// - 나중에 실제 스폰된 Enemy 프리팹에 Behavior Agent를 붙이는 구조가 완성되면 제거해도 됩니다.

public sealed class EnemyBehaviorTestInitializer : MonoBehaviour
{
    [Header("Test Enemy Data")]
    [Tooltip("enemies.json의 _id입니다. 기본 테스트는 ice_zombie를 사용합니다.")]
    [SerializeField] private string _enemyDataId = "ice_zombie";

    [Header("Behavior Test")]
    [Tooltip("테스트 중 기존 EnemyBrain이 이동 명령을 중복으로 내리지 않도록 끕니다.")]
    [SerializeField] private bool _disableEnemyBrain = true;

    private IEnumerator Start()
    {
        // GameManager와 GameDataManager가 초기화될 시간을 조금 기다립니다.
        yield return null;

        GameDataManager gameDataManager = GetGameDataManager();
        if (gameDataManager == null)
        {
            Debug.LogWarning("EnemyBehaviorTestInitializer: GameDataManager를 찾지 못했습니다.");
            yield break;
        }

        gameDataManager.Initialize();

        EnemyData enemyData = gameDataManager.GetEnemy(_enemyDataId);
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyBehaviorTestInitializer: EnemyData를 찾지 못했습니다. id=" + _enemyDataId);
            yield break;
        }

        InitializeEnemyComponents(enemyData);
    }

    private GameDataManager GetGameDataManager()
    {
        if (GameManager.Inst != null)
        {
            GameDataManager managerFromGameManager = GameManager.Inst.GetGameDataManager();
            if (managerFromGameManager != null)
            {
                return managerFromGameManager;
            }
        }

        return FindFirstObjectByType<GameDataManager>();
    }

    private void InitializeEnemyComponents(EnemyData enemyData)
    {
        EnemyStatus enemyStatus = GetComponent<EnemyStatus>();
        EnemyDetector enemyDetector = GetComponent<EnemyDetector>();
        EnemyAttack enemyAttack = GetComponent<EnemyAttack>();
        EnemyMovement enemyMovement = GetComponent<EnemyMovement>();
        EnemyBrain enemyBrain = GetComponent<EnemyBrain>();

        if (enemyStatus != null)
        {
            enemyStatus.Initialize(enemyData);
        }

        if (enemyDetector != null)
        {
            enemyDetector.Initialize(enemyData);
        }

        if (enemyAttack != null)
        {
            enemyAttack.Initialize(enemyData);
        }

        if (enemyMovement != null)
        {
            // 핵심:
            // 이 호출이 되어야 EnemyMovement 내부 _isInitialized가 true가 됩니다.
            // Inspector에서 Move Speed만 직접 바꾸는 것으로는 _isInitialized가 true가 되지 않습니다.
            enemyMovement.Initialize(enemyData, enemyDetector);
        }
        else
        {
            Debug.LogWarning("EnemyBehaviorTestInitializer: EnemyMovement가 없습니다. object=" + gameObject.name);
        }

        if (_disableEnemyBrain && enemyBrain != null)
        {
            enemyBrain.enabled = false;
        }

        Debug.Log("EnemyBehaviorTestInitializer: Enemy 초기화 완료. id=" + enemyData.Id + ", object=" + gameObject.name);
    }
}