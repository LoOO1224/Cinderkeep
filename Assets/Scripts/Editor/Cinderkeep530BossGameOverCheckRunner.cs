using System;
using System.Reflection;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 실제 GameFlow가 스폰한 보스 공격으로 CinderHeart가 파괴되고 GameOver로 이어지는지 확인합니다.
// 에디터 전용 Check 러너이며 실제 빌드에는 포함되지 않습니다.
[InitializeOnLoad]
public static class Cinderkeep530BossGameOverCheckRunner
{
    private const string MenuPath = "Cinderkeep/5.30 Check/Run Boss GameOver Check";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string PendingKey = "Cinderkeep530BossGameOverCheck.Pending";
    private const string FrameKey = "Cinderkeep530BossGameOverCheck.Frame";
    private const int MaxWaitFrames = 1200;

    static Cinderkeep530BossGameOverCheckRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem(MenuPath)]
    public static void RunBossGameOverCheck()
    {
        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        SessionState.SetBool(PendingKey, true);
        SessionState.SetInt(FrameKey, 0);

        if (EditorApplication.isPlaying)
        {
            return;
        }

        EditorApplication.EnterPlaymode();
    }

    private static void OnEditorUpdate()
    {
        if (SessionState.GetBool(PendingKey, false) == false)
        {
            return;
        }

        if (EditorApplication.isPlaying == false)
        {
            return;
        }

        int frame = SessionState.GetInt(FrameKey, 0) + 1;
        SessionState.SetInt(FrameKey, frame);

        GameManager gameManager = GameManager.Inst;
        if ((gameManager == null || gameManager.IsInitialized == false) && frame < MaxWaitFrames)
        {
            return;
        }

        try
        {
            RunPlayModeCheck(frame);
        }
        catch (Exception exception)
        {
            Debug.LogError("[Cinderkeep 5.30] boss gameover check failed.\n" + exception);
            Complete(false);
        }
    }

    private static void RunPlayModeCheck(int frame)
    {
        BossGameOverCheckState state = BossGameOverCheckState.Resolve();
        if (state.IsStarted == false)
        {
            state.Start();
            return;
        }

        if (state.TryPrepareLethalBossAttack())
        {
            return;
        }

        if (state.HasPassed())
        {
            Debug.Log("[Cinderkeep 5.30] boss gameover check passed.");
            Complete(true);
            return;
        }

        if (frame >= MaxWaitFrames)
        {
            throw new InvalidOperationException(state.BuildTimeoutMessage());
        }
    }

    private static T Require<T>(T value, string label)
        where T : class
    {
        if (value == null)
        {
            throw new InvalidOperationException(label + " is missing.");
        }

        return value;
    }

    private static void Complete(bool isSuccess)
    {
        BossGameOverCheckState.Clear();
        SessionState.SetBool(PendingKey, false);
        SessionState.SetInt(FrameKey, 0);
        EditorApplication.ExitPlaymode();

        if (Application.isBatchMode == false)
        {
            return;
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(isSuccess ? 0 : 1);
    }

    private sealed class BossGameOverCheckState
    {
        private static BossGameOverCheckState _current;

        private GameManager _gameManager;
        private GameFlowController _gameFlowController;
        private CinderHeart _cinderHeart;
        private EnemyStatus _bossStatus;
        private int _startedFrame;
        private bool _bossPrepared;

        public bool IsStarted
        {
            get { return _gameManager != null && _gameFlowController != null; }
        }

        public static BossGameOverCheckState Resolve()
        {
            if (_current == null)
            {
                _current = new BossGameOverCheckState();
            }

            return _current;
        }

        public static void Clear()
        {
            _current = null;
        }

        public void Start()
        {
            Time.timeScale = 1f;
            _gameManager = Require(GameManager.Inst, "GameManager.Inst");
            _gameFlowController = Require(_gameManager.GetGameFlowController(), "GameFlowController");
            _cinderHeart = Require(UnityEngine.Object.FindFirstObjectByType<CinderHeart>(), "CinderHeart");

            _gameManager.StartNewGame();
            _gameManager.GameRunModel.SetDay(GameRunModel.FinalDay);

            MethodInfo startBossApproach = typeof(GameFlowController).GetMethod(
                "StartBossApproach",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(startBossApproach, "GameFlowController.StartBossApproach");
            startBossApproach.Invoke(_gameFlowController, null);

            _startedFrame = SessionState.GetInt(FrameKey, 0);
        }

        public bool TryPrepareLethalBossAttack()
        {
            if (_bossPrepared)
            {
                return false;
            }

            _bossStatus = FindBossStatus();
            if (_bossStatus == null)
            {
                return false;
            }

            _bossStatus.transform.position = _cinderHeart.transform.position + new Vector3(0f, 0f, -2.75f);
            PrepareBossForCinderHeartAttack(_bossStatus);
            ReduceCinderHeartToOneHealth();
            _bossPrepared = true;
            return true;
        }

        public bool HasPassed()
        {
            return _gameManager != null
                && _gameManager.GameRunModel != null
                && _gameManager.GameRunModel.IsGameOver
                && _cinderHeart != null
                && _cinderHeart.IsDestroyed;
        }

        public string BuildTimeoutMessage()
        {
            string bossName = _bossStatus == null ? "null" : _bossStatus.gameObject.name;
            string phase = _gameManager == null || _gameManager.GameRunModel == null
                ? "null"
                : _gameManager.GameRunModel.Phase.ToString();
            float currentHealth = _cinderHeart == null ? -1f : _cinderHeart.GetCurrentHealth();

            return "boss attack did not trigger GameOver"
                + " / framesSinceStart=" + (SessionState.GetInt(FrameKey, 0) - _startedFrame)
                + " / phase=" + phase
                + " / boss=" + bossName
                + " / prepared=" + _bossPrepared
                + " / cinderHeartHealth=" + currentHealth.ToString("F1");
        }

        private void PrepareBossForCinderHeartAttack(EnemyStatus bossStatus)
        {
            EnemyBrain enemyBrain = Require(bossStatus.GetComponent<EnemyBrain>(), "spawned boss EnemyBrain");
            EnemyMovement enemyMovement = Require(bossStatus.GetComponent<EnemyMovement>(), "spawned boss EnemyMovement");
            EnemyAttack enemyAttack = Require(bossStatus.GetComponent<EnemyAttack>(), "spawned boss EnemyAttack");
            Damageable cinderHeartDamageable = Require(_cinderHeart.GetComponent<Damageable>(), "CinderHeart Damageable");
            BossData bossData = Require(_gameManager.GetGameDataManager().GetBoss("frost_colossus"), "BossData frost_colossus");

            enemyAttack.Initialize(bossData);
            enemyMovement.Initialize(bossData);
            enemyBrain.SetCinderHeartTarget(cinderHeartDamageable);
            enemyBrain.SetCinderHeartChaseEnabled(true);
            enemyBrain.SetPlayerDetectionPriorityEnabled(false);
            enemyBrain.SetTowerDetectionPriorityEnabled(false);
            enemyBrain.SetNightTime(true);
        }

        private void ReduceCinderHeartToOneHealth()
        {
            float currentHealth = _cinderHeart.GetCurrentHealth();
            if (currentHealth <= 1f)
            {
                return;
            }

            _cinderHeart.TakeDamage(currentHealth - 1f);
        }

        private EnemyStatus FindBossStatus()
        {
            EnemyStatus[] enemyStatuses = UnityEngine.Object.FindObjectsByType<EnemyStatus>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            for (int i = 0; i < enemyStatuses.Length; i++)
            {
                EnemyStatus enemyStatus = enemyStatuses[i];
                if (enemyStatus == null)
                {
                    continue;
                }

                if (enemyStatus.gameObject.name.StartsWith("Boss_", StringComparison.OrdinalIgnoreCase))
                {
                    return enemyStatus;
                }
            }

            return null;
        }
    }
}
