using System;
using System.Reflection;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 1일차 낮부터 3일차 보스 페이즈까지 GameFlow 전환이 끊기지 않는지 확인합니다.
// 시간을 직접 0으로 밀어 실제 Update 기반 전환 경로를 빠르게 검증합니다.
[InitializeOnLoad]
public static class Cinderkeep531ThreeDayPhaseFlowCheckRunner
{
    private const string MenuPath = "Cinderkeep/5.31 Check/Run Three Day Phase Flow Check";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string PendingKey = "Cinderkeep531ThreeDayPhaseFlowCheck.Pending";
    private const string FrameKey = "Cinderkeep531ThreeDayPhaseFlowCheck.Frame";
    private const int MaxWaitFrames = 1800;

    static Cinderkeep531ThreeDayPhaseFlowCheckRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem(MenuPath)]
    public static void RunThreeDayPhaseFlowCheck()
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
            Debug.LogError("[Cinderkeep 5.31] three day phase flow check failed.\n" + exception);
            Complete(false);
        }
    }

    private static void RunPlayModeCheck(int frame)
    {
        ThreeDayPhaseFlowCheckState state = ThreeDayPhaseFlowCheckState.Resolve();
        if (state.IsStarted == false)
        {
            state.Start();
            return;
        }

        state.Tick();
        if (state.HasPassed())
        {
            Debug.Log("[Cinderkeep 5.31] three day phase flow check passed.");
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
        ThreeDayPhaseFlowCheckState.Clear();
        SessionState.SetBool(PendingKey, false);
        SessionState.SetInt(FrameKey, 0);
        EditorApplication.ExitPlaymode();

        if (Application.isBatchMode == false)
        {
            return;
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(isSuccess ? 0 : 1);
    }

    private sealed class ThreeDayPhaseFlowCheckState
    {
        private static readonly ExpectedPhase[] ExpectedPhases =
        {
            new ExpectedPhase(1, GameRunPhase.Day),
            new ExpectedPhase(1, GameRunPhase.Night),
            new ExpectedPhase(1, GameRunPhase.MorningReward),
            new ExpectedPhase(2, GameRunPhase.Day),
            new ExpectedPhase(2, GameRunPhase.Night),
            new ExpectedPhase(2, GameRunPhase.MorningReward),
            new ExpectedPhase(3, GameRunPhase.Day),
            new ExpectedPhase(3, GameRunPhase.Night),
            new ExpectedPhase(3, GameRunPhase.BossApproach),
            new ExpectedPhase(3, GameRunPhase.BossFight)
        };

        private static ThreeDayPhaseFlowCheckState _current;

        private GameManager _gameManager;
        private GameFlowController _gameFlowController;
        private int _expectedIndex;
        private bool _currentPhaseForced;
        private int _startedFrame;

        public bool IsStarted
        {
            get { return _gameManager != null && _gameFlowController != null; }
        }

        public static ThreeDayPhaseFlowCheckState Resolve()
        {
            if (_current == null)
            {
                _current = new ThreeDayPhaseFlowCheckState();
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
            DisableMorningRewardPopup(_gameFlowController);
            _gameManager.StartNewGame();
            _startedFrame = SessionState.GetInt(FrameKey, 0);
        }

        public void Tick()
        {
            if (HasPassed())
            {
                return;
            }

            GameRunModel runModel = _gameManager.GameRunModel;
            ExpectedPhase expectedPhase = ExpectedPhases[_expectedIndex];
            if (expectedPhase.Matches(runModel))
            {
                ForceCurrentPhaseToFinish(runModel);
                return;
            }

            if (_expectedIndex + 1 < ExpectedPhases.Length && ExpectedPhases[_expectedIndex + 1].Matches(runModel))
            {
                _expectedIndex++;
                _currentPhaseForced = false;
                return;
            }

            throw new InvalidOperationException("unexpected phase transition"
                + " / expected=" + expectedPhase
                + " / actualDay=" + runModel.Day
                + " / actualPhase=" + runModel.Phase);
        }

        public bool HasPassed()
        {
            if (_gameManager == null || _gameManager.GameRunModel == null)
            {
                return false;
            }

            GameRunModel runModel = _gameManager.GameRunModel;
            bool reachedBossFight = _expectedIndex >= ExpectedPhases.Length - 1
                && runModel.Day == 3
                && runModel.Phase == GameRunPhase.BossFight;
            return reachedBossFight && FindBossStatus() != null;
        }

        public string BuildTimeoutMessage()
        {
            GameRunModel runModel = _gameManager == null ? null : _gameManager.GameRunModel;
            string actual = runModel == null ? "null" : runModel.Day + " / " + runModel.Phase;
            string expected = _expectedIndex >= ExpectedPhases.Length ? "complete" : ExpectedPhases[_expectedIndex].ToString();

            return "three day phase flow did not reach boss fight"
                + " / framesSinceStart=" + (SessionState.GetInt(FrameKey, 0) - _startedFrame)
                + " / expected=" + expected
                + " / actual=" + actual
                + " / boss=" + (FindBossStatus() == null ? "null" : FindBossStatus().gameObject.name);
        }

        private void ForceCurrentPhaseToFinish(GameRunModel runModel)
        {
            if (_currentPhaseForced)
            {
                return;
            }

            if (runModel.Phase == GameRunPhase.BossFight)
            {
                _expectedIndex = ExpectedPhases.Length - 1;
                _currentPhaseForced = true;
                return;
            }

            runModel.SetRemainingTime(0f);
            _currentPhaseForced = true;
        }

        private static void DisableMorningRewardPopup(GameFlowController gameFlowController)
        {
            FieldInfo fieldInfo = typeof(GameFlowController).GetField(
                "_openCinderHeartSkillOnMorningReward",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(gameFlowController, false);
            }
        }

        private static EnemyStatus FindBossStatus()
        {
            EnemyStatus[] enemyStatuses = UnityEngine.Object.FindObjectsByType<EnemyStatus>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            for (int i = 0; i < enemyStatuses.Length; i++)
            {
                EnemyStatus enemyStatus = enemyStatuses[i];
                if (enemyStatus != null && enemyStatus.gameObject.name.StartsWith("Boss_", StringComparison.OrdinalIgnoreCase))
                {
                    return enemyStatus;
                }
            }

            return null;
        }

        private readonly struct ExpectedPhase
        {
            private readonly int _day;
            private readonly GameRunPhase _phase;

            public ExpectedPhase(int day, GameRunPhase phase)
            {
                _day = day;
                _phase = phase;
            }

            public bool Matches(GameRunModel runModel)
            {
                return runModel != null && runModel.Day == _day && runModel.Phase == _phase;
            }

            public override string ToString()
            {
                return _day + " / " + _phase;
            }
        }
    }
}
