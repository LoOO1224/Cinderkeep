using System;
using System.Reflection;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 실제 GameFlow 경로에서 3일차 보스 생성과 보스 사망 후 ClearFlow 연결을 확인합니다.
// 에디터 전용 Check 러너이며 실제 빌드에는 포함되지 않습니다.
[InitializeOnLoad]
public static class Cinderkeep528BossClearFlowCheckRunner
{
    private const string MenuPath = "Cinderkeep/5.28 Check/Run Boss Clear Flow Check";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string PendingKey = "Cinderkeep528BossClearFlowCheck.Pending";
    private const string FrameKey = "Cinderkeep528BossClearFlowCheck.Frame";
    private const int MaxWaitFrames = 900;

    static Cinderkeep528BossClearFlowCheckRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem(MenuPath)]
    public static void RunBossClearFlowCheck()
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
            Debug.LogError("[Cinderkeep 5.28] boss clear flow check failed.\n" + exception);
            Complete(false);
        }
    }

    private static void RunPlayModeCheck(int frame)
    {
        BossClearFlowCheckState state = BossClearFlowCheckState.Resolve();
        if (state.IsStarted == false)
        {
            state.Start();
            return;
        }

        if (state.TryKillSpawnedBoss())
        {
            return;
        }

        if (state.HasPassed())
        {
            Debug.Log("[Cinderkeep 5.28] boss clear flow check passed.");
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

    private static void Require(bool condition, string label)
    {
        if (condition == false)
        {
            throw new InvalidOperationException(label + " check failed.");
        }
    }

    private static void Complete(bool isSuccess)
    {
        BossClearFlowCheckState.Clear();
        SessionState.SetBool(PendingKey, false);
        SessionState.SetInt(FrameKey, 0);
        EditorApplication.ExitPlaymode();

        if (Application.isBatchMode == false)
        {
            return;
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(isSuccess ? 0 : 1);
    }

    private sealed class BossClearFlowCheckState
    {
        private static BossClearFlowCheckState _current;

        private GameManager _gameManager;
        private GameFlowController _gameFlowController;
        private EnemyStatus _bossStatus;
        private int _startedFrame;
        private bool _bossKillRequested;

        public bool IsStarted
        {
            get { return _gameManager != null && _gameFlowController != null; }
        }

        public static BossClearFlowCheckState Resolve()
        {
            if (_current == null)
            {
                _current = new BossClearFlowCheckState();
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
            Require(_gameManager.IsInitialized, "GameManager initialized");

            _gameFlowController = Require(_gameManager.GetGameFlowController(), "GameFlowController");
            _gameManager.StartNewGame();
            _gameManager.GameRunModel.SetDay(GameRunModel.FinalDay);

            MethodInfo startBossApproach = typeof(GameFlowController).GetMethod(
                "StartBossApproach",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(startBossApproach != null, "GameFlowController.StartBossApproach");
            startBossApproach.Invoke(_gameFlowController, null);

            _startedFrame = SessionState.GetInt(FrameKey, 0);
        }

        public bool TryKillSpawnedBoss()
        {
            if (_bossKillRequested)
            {
                return false;
            }

            _bossStatus = FindBossStatus();
            if (_bossStatus == null)
            {
                return false;
            }

            _bossKillRequested = true;
            _bossStatus.TakeDamage(_bossStatus.GetMaxHealth() + 999f);
            return true;
        }

        public bool HasPassed()
        {
            return _gameManager != null
                && _gameManager.GameRunModel != null
                && _gameManager.GameRunModel.IsClear;
        }

        public string BuildTimeoutMessage()
        {
            string bossName = _bossStatus == null ? "null" : _bossStatus.gameObject.name;
            string phase = _gameManager == null || _gameManager.GameRunModel == null
                ? "null"
                : _gameManager.GameRunModel.Phase.ToString();

            return "boss clear flow did not complete"
                + " / framesSinceStart=" + (SessionState.GetInt(FrameKey, 0) - _startedFrame)
                + " / phase=" + phase
                + " / boss=" + bossName
                + " / killRequested=" + _bossKillRequested;
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
