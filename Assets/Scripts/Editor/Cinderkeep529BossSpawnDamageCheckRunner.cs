using System;
using System.Reflection;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 실제 GameFlow가 스폰한 보스의 CinderHeart 공격 연결을 확인합니다.
// 먼 스폰 지점으로 인한 대기 시간을 줄이기 위해 생성된 보스를 공격 거리 안으로 옮겨 판정 경로를 검증합니다.
[InitializeOnLoad]
public static class Cinderkeep529BossSpawnDamageCheckRunner
{
    private const string MenuPath = "Cinderkeep/5.29 Check/Run Spawned Boss Damage Check";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string PendingKey = "Cinderkeep529BossSpawnDamageCheck.Pending";
    private const string FrameKey = "Cinderkeep529BossSpawnDamageCheck.Frame";
    private const int MaxWaitFrames = 900;

    static Cinderkeep529BossSpawnDamageCheckRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem(MenuPath)]
    public static void RunSpawnedBossDamageCheck()
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
            Debug.LogError("[Cinderkeep 5.29] spawned boss damage check failed.\n" + exception);
            Complete(false);
        }
    }

    private static void RunPlayModeCheck(int frame)
    {
        SpawnedBossDamageCheckState state = SpawnedBossDamageCheckState.Resolve();
        if (state.IsStarted == false)
        {
            state.Start();
            return;
        }

        if (state.TryPrepareSpawnedBoss())
        {
            return;
        }

        if (state.HasPassed())
        {
            Debug.Log("[Cinderkeep 5.29] spawned boss damage check passed.");
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
        SpawnedBossDamageCheckState.Clear();
        SessionState.SetBool(PendingKey, false);
        SessionState.SetInt(FrameKey, 0);
        EditorApplication.ExitPlaymode();

        if (Application.isBatchMode == false)
        {
            return;
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(isSuccess ? 0 : 1);
    }

    private sealed class SpawnedBossDamageCheckState
    {
        private static SpawnedBossDamageCheckState _current;

        private GameManager _gameManager;
        private GameFlowController _gameFlowController;
        private CinderHeart _cinderHeart;
        private EnemyStatus _bossStatus;
        private float _startHealth;
        private float _startDistance;
        private int _startedFrame;
        private bool _bossPrepared;

        public bool IsStarted
        {
            get { return _gameManager != null && _gameFlowController != null; }
        }

        public static SpawnedBossDamageCheckState Resolve()
        {
            if (_current == null)
            {
                _current = new SpawnedBossDamageCheckState();
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

        public bool TryPrepareSpawnedBoss()
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

            _startHealth = _cinderHeart.GetCurrentHealth();
            _bossStatus.transform.position = _cinderHeart.transform.position + new Vector3(0f, 0f, -2.75f);
            _startDistance = Vector3.Distance(_bossStatus.transform.position, _cinderHeart.transform.position);

            EnemyBrain enemyBrain = Require(_bossStatus.GetComponent<EnemyBrain>(), "spawned boss EnemyBrain");
            EnemyMovement enemyMovement = Require(_bossStatus.GetComponent<EnemyMovement>(), "spawned boss EnemyMovement");
            EnemyAttack enemyAttack = Require(_bossStatus.GetComponent<EnemyAttack>(), "spawned boss EnemyAttack");

            Damageable cinderHeartDamageable = Require(_cinderHeart.GetComponent<Damageable>(), "CinderHeart Damageable");
            BossData bossData = Require(_gameManager.GetGameDataManager().GetBoss("frost_colossus"), "BossData frost_colossus");
            enemyAttack.Initialize(bossData);
            enemyMovement.Initialize(bossData);
            enemyBrain.SetCinderHeartTarget(cinderHeartDamageable);
            enemyBrain.SetCinderHeartChaseEnabled(true);
            enemyBrain.SetPlayerDetectionPriorityEnabled(false);
            enemyBrain.SetTowerDetectionPriorityEnabled(false);
            enemyBrain.SetNightTime(true);

            _bossPrepared = true;
            return true;
        }

        public bool HasPassed()
        {
            if (_bossStatus == null || _cinderHeart == null)
            {
                return false;
            }

            bool damagedCinderHeart = _cinderHeart.GetCurrentHealth() < _startHealth;
            return damagedCinderHeart;
        }

        public string BuildTimeoutMessage()
        {
            string bossName = _bossStatus == null ? "null" : _bossStatus.gameObject.name;
            EnemyBrain enemyBrain = _bossStatus == null ? null : _bossStatus.GetComponent<EnemyBrain>();
            float currentHealth = _cinderHeart == null ? -1f : _cinderHeart.GetCurrentHealth();
            float currentDistance = _bossStatus == null || _cinderHeart == null
                ? -1f
                : Vector3.Distance(_bossStatus.transform.position, _cinderHeart.transform.position);

            return "spawned boss did not damage CinderHeart"
                + " / framesSinceStart=" + (SessionState.GetInt(FrameKey, 0) - _startedFrame)
                + " / boss=" + bossName
                + " / prepared=" + _bossPrepared
                + " / startDistance=" + _startDistance.ToString("F2")
                + " / currentDistance=" + currentDistance.ToString("F2")
                + " / startHealth=" + _startHealth.ToString("F1")
                + " / currentHealth=" + currentHealth.ToString("F1")
                + " / brainTarget=" + GetDamageableName(GetPrivateFieldValue(enemyBrain, "_currentAttackTarget") as Damageable)
                + " / buildingTarget=" + GetBuildingName(GetPrivateFieldValue(enemyBrain, "_currentBuildingAttackTarget") as BuildingHp)
                + " / cinderTarget=" + GetDamageableName(GetPrivateFieldValue(enemyBrain, "_cinderHeartDamageable") as Damageable)
                + " / playerPriority=" + GetPrivateFieldValue(enemyBrain, "_usePlayerDetectionAsPriority")
                + " / towerPriority=" + GetPrivateFieldValue(enemyBrain, "_useTowerDetectionAsPriority");
        }

        private static object GetPrivateFieldValue(object targetObject, string fieldName)
        {
            if (targetObject == null)
            {
                return "null";
            }

            FieldInfo fieldInfo = targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return "missing";
            }

            return fieldInfo.GetValue(targetObject);
        }

        private static string GetDamageableName(Damageable damageable)
        {
            if (damageable == null)
            {
                return "null";
            }

            return damageable.gameObject.name;
        }

        private static string GetBuildingName(BuildingHp buildingHp)
        {
            if (buildingHp == null)
            {
                return "null";
            }

            return buildingHp.gameObject.name;
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
