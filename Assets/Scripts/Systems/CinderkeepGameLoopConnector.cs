using Cinderkeep.Gameplay;
using System;
using UnityEngine;
// 게임 시작 시 팀원들이 만든 컴포넌트를 서로 연결하는 얇은 연결 컴포넌트입니다.
// 실제 기능은 각 전용 컴포넌트가 담당하고, 이 클래스는 시작 시점의 연결 순서만 관리합니다.

// 4.00 게임 루프 씬에서 팀원들이 만든 컴포넌트를 서로 연결하는 얇은 연결 컴포넌트입니다.
// 실제 기능은 각 컴포넌트가 담당하고, 이 클래스는 시작 시점의 연결 순서만 관리합니다.
public sealed class CinderkeepGameLoopConnector : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameManager GameManager_GameManager;
    [SerializeField] private GameDataManager GameDataManager_GameDataManager;

    [Header("Player")]
    [SerializeField] private PlayerStatus PlayerStatus_PlayerStatus;

    [Header("UI")]
    [SerializeField] private PlayerHUD PlayerHUD_PlayerHUD;
    [SerializeField] private ResourceUI ResourceUI_ResourceUI;

    [Header("World")]
    [SerializeField] private Transform Transform_CinderHeartTarget;
    [SerializeField] private Camera Camera_GameCamera;

    [Header("Enemies")]
    [SerializeField] private EnemyRuntimeSet[] _enemyRuntimeSets;

    private void Start()
    {
        InitializeGameFlow();
        ConnectPlayerHud();
        ConnectResourceHud();
        ConnectEnemies();
    }

    private void InitializeGameFlow()
    {
        if (GameManager_GameManager == null)
        {
            return;
        }

        GameManager_GameManager.StartNewGame();
    }

    private void ConnectPlayerHud()
    {
        if (PlayerHUD_PlayerHUD == null)
        {
            return;
        }

        PlayerHUD_PlayerHUD.SetPlayerStatus(PlayerStatus_PlayerStatus);
    }

    private void ConnectResourceHud()
    {
        if (ResourceUI_ResourceUI == null)
        {
            return;
        }

        if (GameManager_GameManager == null)
        {
            return;
        }

        ResourceUI_ResourceUI.SetPlayerModel(GameManager_GameManager.PlayerModel);
    }

    private void ConnectEnemies()
    {
        if (_enemyRuntimeSets == null)
        {
            return;
        }

        for (int i = 0; i < _enemyRuntimeSets.Length; i++)
        {
            ConnectEnemy(_enemyRuntimeSets[i]);
        }
    }

    private void ConnectEnemy(EnemyRuntimeSet enemyRuntimeSet)
    {
        if (enemyRuntimeSet == null)
        {
            return;
        }

        EnemyData enemyData = GetEnemyData(enemyRuntimeSet.EnemyDataId);
        if (enemyData == null)
        {
            Debug.LogWarning("CinderkeepGameLoopConnector: EnemyData가 없습니다. id=" + enemyRuntimeSet.EnemyDataId);
            return;
        }

        enemyRuntimeSet.Initialize(enemyData, Transform_CinderHeartTarget, Camera_GameCamera);
    }

    private EnemyData GetEnemyData(string enemyDataId)
    {
        if (GameDataManager_GameDataManager == null)
        {
            return null;
        }

        GameDataManager_GameDataManager.Initialize();
        return GameDataManager_GameDataManager.GetEnemy(enemyDataId);
    }
}

[Serializable]
public sealed class EnemyRuntimeSet
{
    [SerializeField] private string _enemyDataId = "ice_zombie";
    [SerializeField] private EnemyStatus EnemyStatus_EnemyStatus;
    [SerializeField] private EnemyAttack EnemyAttack_EnemyAttack;
    [SerializeField] private EnemyDetector EnemyDetector_EnemyDetector;
    [SerializeField] private EnemyMovement EnemyMovement_EnemyMovement;
    [SerializeField] private EnemyHud EnemyHud_EnemyHud;

    public string EnemyDataId
    {
        get
        {
            return _enemyDataId;
        }
    }

    public void Initialize(EnemyData enemyData, Transform cinderHeartTarget, Camera gameCamera)
    {
        if (EnemyStatus_EnemyStatus != null)
        {
            EnemyStatus_EnemyStatus.Initialize(enemyData);
        }

        if (EnemyAttack_EnemyAttack != null)
        {
            EnemyAttack_EnemyAttack.Initialize(enemyData);
        }

        if (EnemyDetector_EnemyDetector != null)
        {
            EnemyDetector_EnemyDetector.Initialize(enemyData);
        }

        if (EnemyMovement_EnemyMovement != null)
        {
            EnemyMovement_EnemyMovement.SetCinderHeartTarget(cinderHeartTarget);
            EnemyMovement_EnemyMovement.Initialize(enemyData, EnemyDetector_EnemyDetector);
        }

        if (EnemyHud_EnemyHud != null)
        {
            EnemyHud_EnemyHud.SetTargetCamera(gameCamera);
        }
    }
}
