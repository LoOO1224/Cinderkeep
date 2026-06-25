using System;
using System.Reflection;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 3일차 보스가 CinderHeart를 목표로 접근하고 피해를 주는지 확인합니다.
// 에디터 전용 Check 러너이며 실제 빌드에는 포함되지 않습니다.
[InitializeOnLoad]
public static class Cinderkeep527BossCinderHeartLoopCheckRunner
{
    private const string MenuPath = "Cinderkeep/5.27 Check/Run Boss CinderHeart Loop Check";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string PendingKey = "Cinderkeep527BossCinderHeartLoopCheck.Pending";
    private const string FrameKey = "Cinderkeep527BossCinderHeartLoopCheck.Frame";
    private const string BossObjectName = "Check_Boss_FrozenGolem_CinderHeart";
    private const string BossDataId = "frost_colossus";
    private const int MaxWaitFrames = 900;

    static Cinderkeep527BossCinderHeartLoopCheckRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem(MenuPath)]
    public static void RunBossCinderHeartLoopCheck()
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
            Debug.LogError("[Cinderkeep 5.27] boss cinderheart loop check failed.\n" + exception);
            Complete(false);
        }
    }

    private static void RunPlayModeCheck(int frame)
    {
        GameManager gameManager = Require(GameManager.Inst, "GameManager.Inst");
        Require(gameManager.IsInitialized, "GameManager initialized");

        BossCheckState state = BossCheckState.Resolve();
        if (state.IsStarted == false)
        {
            state.Start(gameManager);
            return;
        }

        if (state.HasPassed())
        {
            Debug.Log("[Cinderkeep 5.27] boss cinderheart loop check passed.");
            Complete(true);
            return;
        }

        if (frame >= MaxWaitFrames)
        {
            throw new InvalidOperationException(state.BuildTimeoutMessage());
        }
    }

    private static GameObject CreateRuntimeBoss(BossData bossData, CinderHeart cinderHeart, Damageable cinderHeartDamageable)
    {
        GameObject bossObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bossObject.name = BossObjectName;
        bossObject.SetActive(false);
        bossObject.transform.localScale = new Vector3(2.3f, 3f, 2.3f);
        bossObject.transform.position = cinderHeart.transform.position + new Vector3(0f, 0f, -4.5f);
        RuntimePrimitiveMaterial.ApplyColor(bossObject, new Color(0.38f, 0.72f, 0.95f, 1f), "MAT_Check_FrozenGolem");

        Rigidbody rigidbody = bossObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        EnemyStatus enemyStatus = bossObject.AddComponent<EnemyStatus>();
        bossObject.AddComponent<Damageable>();
        EnemyAttack enemyAttack = bossObject.AddComponent<EnemyAttack>();
        EnemyMovement enemyMovement = bossObject.AddComponent<EnemyMovement>();
        EnemyBrain enemyBrain = bossObject.AddComponent<EnemyBrain>();

        enemyStatus.Initialize(bossData);
        enemyAttack.Initialize(bossData);
        enemyMovement.Initialize(bossData);
        enemyBrain.SetCinderHeartTarget(cinderHeartDamageable);
        enemyBrain.SetCinderHeartChaseEnabled(true);
        enemyBrain.SetNightTime(true);

        bossObject.SetActive(true);
        enemyStatus.Initialize(bossData);
        enemyAttack.Initialize(bossData);
        enemyMovement.Initialize(bossData);
        enemyBrain.SetCinderHeartTarget(cinderHeartDamageable);
        enemyBrain.SetCinderHeartChaseEnabled(true);
        enemyBrain.SetNightTime(true);
        return bossObject;
    }

    private static Damageable ResolveCinderHeartDamageable(CinderHeart cinderHeart)
    {
        Damageable damageable = cinderHeart.GetComponent<Damageable>();
        if (damageable != null)
        {
            return damageable;
        }

        return cinderHeart.GetComponentInParent<Damageable>();
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
        BossCheckState.Clear();
        SessionState.SetBool(PendingKey, false);
        SessionState.SetInt(FrameKey, 0);
        EditorApplication.ExitPlaymode();

        if (Application.isBatchMode == false)
        {
            return;
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(isSuccess ? 0 : 1);
    }

    private sealed class BossCheckState
    {
        private static BossCheckState _current;

        private GameObject _bossObject;
        private CinderHeart _cinderHeart;
        private EnemyBrain _enemyBrain;
        private EnemyMovement _enemyMovement;
        private float _startHealth;
        private float _startDistance;
        private int _startedFrame;

        public bool IsStarted
        {
            get { return _bossObject != null && _cinderHeart != null; }
        }

        public static BossCheckState Resolve()
        {
            if (_current == null)
            {
                _current = new BossCheckState();
            }

            return _current;
        }

        public static void Clear()
        {
            _current = null;
        }

        public void Start(GameManager gameManager)
        {
            Time.timeScale = 1f;

            GameDataManager gameDataManager = Require(gameManager.GetGameDataManager(), "GameDataManager");
            gameDataManager.Initialize();

            BossData bossData = Require(gameDataManager.GetBoss(BossDataId), "BossData " + BossDataId);
            _cinderHeart = Require(UnityEngine.Object.FindFirstObjectByType<CinderHeart>(), "CinderHeart");
            Damageable cinderHeartDamageable = Require(ResolveCinderHeartDamageable(_cinderHeart), "CinderHeart Damageable");

            _startHealth = _cinderHeart.GetCurrentHealth();
            _bossObject = CreateRuntimeBoss(bossData, _cinderHeart, cinderHeartDamageable);
            _enemyBrain = Require(_bossObject.GetComponent<EnemyBrain>(), "runtime boss EnemyBrain");
            _enemyMovement = Require(_bossObject.GetComponent<EnemyMovement>(), "runtime boss EnemyMovement");
            _startDistance = Vector3.Distance(_bossObject.transform.position, _cinderHeart.transform.position);
            _startedFrame = SessionState.GetInt(FrameKey, 0);
        }

        public bool HasPassed()
        {
            if (_bossObject == null || _cinderHeart == null)
            {
                return false;
            }

            float currentDistance = Vector3.Distance(_bossObject.transform.position, _cinderHeart.transform.position);
            bool movedTowardCinderHeart = currentDistance < _startDistance - 0.15f;
            bool damagedCinderHeart = _cinderHeart.GetCurrentHealth() < _startHealth;
            return movedTowardCinderHeart && damagedCinderHeart;
        }

        public string BuildTimeoutMessage()
        {
            if (_bossObject == null || _cinderHeart == null)
            {
                return "runtime boss or CinderHeart was not created.";
            }

            float currentDistance = Vector3.Distance(_bossObject.transform.position, _cinderHeart.transform.position);
            return "boss did not damage CinderHeart"
                + " / framesSinceStart=" + (SessionState.GetInt(FrameKey, 0) - _startedFrame)
                + " / startDistance=" + _startDistance.ToString("F2")
                + " / currentDistance=" + currentDistance.ToString("F2")
                + " / startHealth=" + _startHealth.ToString("F1")
                + " / currentHealth=" + _cinderHeart.GetCurrentHealth().ToString("F1")
                + " / brainNight=" + GetPrivateFieldValue(_enemyBrain, "_isNightTime")
                + " / brainCanChase=" + GetPrivateFieldValue(_enemyBrain, "_canChaseCinderHeart")
                + " / brainTarget=" + GetTargetName(GetPrivateFieldValue(_enemyBrain, "_currentAttackTarget") as Damageable)
                + " / cinderTarget=" + GetTargetName(GetPrivateFieldValue(_enemyBrain, "_cinderHeartDamageable") as Damageable)
                + " / movementInitialized=" + GetPrivateFieldValue(_enemyMovement, "_isInitialized")
                + " / nextDecisionTime=" + GetPrivateFieldValue(_enemyBrain, "_nextDecisionTime")
                + " / timeScale=" + Time.timeScale.ToString("F2")
                + " / time=" + Time.time.ToString("F2");
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

        private static string GetTargetName(Damageable damageable)
        {
            if (damageable == null)
            {
                return "null";
            }

            return damageable.gameObject.name;
        }
    }
}
