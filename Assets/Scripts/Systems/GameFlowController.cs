using System;
using System.Collections.Generic;
using Cinderkeep.Gameplay;
using UnityEngine;

// 낮, 밤, 아침 보상, 보스전, 클리어/게임오버로 이어지는 한 판의 흐름을 관리합니다.
// 스폰 실행은 GameFlowEnemySpawnDirector에 맡기고, 이 클래스는 페이즈 전환과 보상 선택 타이밍을 결정합니다.
public sealed class GameFlowController : MonoBehaviour, IGameInitializable
{
    public static event Action BossApproachStartedGlobal;

    private const int MorningRewardOptionCount = 3;

    [Header("Flow Settings")]
    [SerializeField] private GameFlowSettings _gameFlowSettings = new GameFlowSettings();

    [Header("Flow Data")]
    [SerializeField] private bool _useGameFlowPhaseData = true;

    [Header("Enemy Spawn Director")]
    [SerializeField] private GameFlowEnemySpawnDirector _enemySpawnDirector;

    [Header("CinderHeart Reward")]
    [SerializeField] private bool _openCinderHeartSkillOnMorningReward = true;
    [SerializeField] private string[] _morningRewardSkillIds =
    {
        "cinder_spark_arrow",
        "rekindled_core",
        "warm_pulse"
    };

    private GameManager _gameManager;
    private GameRunModel _gameRunModel;
    private bool _isInitialized;
    private bool _isFlowRunning;
    private bool _isWaitingForCinderHeartSkillSelection;
    private bool _isBossClearHandled;
    private float _previousTimeScale = 1f;

    public bool IsInitialized
    {
        get { return _isInitialized; }
    }

    public bool IsWaitingForCinderHeartSkillSelection
    {
        get { return _isWaitingForCinderHeartSkillSelection; }
    }

    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
        _gameRunModel = _gameManager == null ? null : _gameManager.GameRunModel;
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        StopEnemySpawn();
        _isFlowRunning = false;
        _isInitialized = true;
    }

    public void InitializeEnemySpawnPoints(GameObjectManager gameObjectManager, EnemyLoopConnector enemyLoopConnector)
    {
        if (_enemySpawnDirector == null)
        {
            return;
        }

        _enemySpawnDirector.Initialize(gameObjectManager, enemyLoopConnector);
    }

    private void Update()
    {
        UpdateFlowTime();
    }

    public void StartFlow()
    {
        if (_gameRunModel == null)
        {
            Debug.LogWarning("GameFlowController: GameRunModel reference is empty.");
            return;
        }

        _gameRunModel.StartRun();
        _isFlowRunning = true;
        _isBossClearHandled = false;
        StartDay(GameRunModel.FirstDay);
    }

    public void StopFlowAsGameOver()
    {
        StopFlow();
    }

    public void StopFlow()
    {
        _isFlowRunning = false;
        RestoreTimeScaleIfRewardSelectionIsOpen();
        StopEnemySpawn();
    }

    public void ClearFlow()
    {
        if (_gameRunModel == null)
        {
            return;
        }

        _isFlowRunning = false;
        RestoreTimeScaleIfRewardSelectionIsOpen();
        StopEnemySpawn();
        _gameRunModel.ClearRun();

        if (_gameManager == null || _gameManager.GetUIManager() == null)
        {
            return;
        }

        _gameManager.GetUIManager().OpenClearPanel();
    }

    private void UpdateFlowTime()
    {
        if (CanUpdateFlow() == false)
        {
            return;
        }

        _gameRunModel.SetRemainingTime(_gameRunModel.RemainingTime - Time.deltaTime);
        if (_gameRunModel.RemainingTime > 0f)
        {
            return;
        }

        AdvancePhase();
    }

    private bool CanUpdateFlow()
    {
        if (_isFlowRunning == false || _gameRunModel == null || _gameRunModel.IsPlaying == false)
        {
            return false;
        }

        if (_gameRunModel.Phase == GameRunPhase.BossFight)
        {
            return false;
        }

        return _isWaitingForCinderHeartSkillSelection == false;
    }

    private void AdvancePhase()
    {
        switch (_gameRunModel.Phase)
        {
            case GameRunPhase.Day:
                StartNight();
                return;
            case GameRunPhase.Night:
                FinishNight();
                return;
            case GameRunPhase.MorningReward:
                StartNextDay();
                return;
            case GameRunPhase.BossApproach:
                StartBossFight();
                return;
        }
    }

    private void StartDay(int day)
    {
        global::HandStonePickupSceneBootstrap.ResetDailyPickups();
        global::FoodPickupSceneBootstrap.ResetDailyFoodPickups();
        _gameRunModel.SetDay(day);
        _gameRunModel.SetPhase(GameRunPhase.Day);
        _gameRunModel.SetPhaseTime(GetPhaseDuration(GameRunPhase.Day, day, _gameFlowSettings.DayDuration));
        StartEnemySpawn(EnemySpawnMode.Day);
        PlayPhaseBgm(GameRunPhase.Day);
    }

    private void StartNight()
    {
        _gameRunModel.SetPhase(GameRunPhase.Night);
        _gameRunModel.SetPhaseTime(GetPhaseDuration(GameRunPhase.Night, _gameRunModel.Day, _gameFlowSettings.NightDuration));
        StartEnemySpawn(EnemySpawnMode.Night);
        PlayPhaseBgm(GameRunPhase.Night);
    }

    private void FinishNight()
    {
        StopEnemySpawn();

        if (_gameRunModel.IsFinalDay())
        {
            StartBossApproach();
            return;
        }

        StartMorningReward();
    }

    private void StartMorningReward()
    {
        _gameRunModel.SetPhase(GameRunPhase.MorningReward);
        _gameRunModel.SetPhaseTime(GetPhaseDuration(
            GameRunPhase.MorningReward,
            _gameRunModel.Day,
            _gameFlowSettings.MorningRewardDuration));
        PlayPhaseBgm(GameRunPhase.MorningReward);
        TryOpenCinderHeartSkillSelection();
    }

    private void StartNextDay()
    {
        _gameRunModel.AdvanceDay();
        StartDay(_gameRunModel.Day);
    }

    private void StartBossApproach()
    {
        _isBossClearHandled = false;
        _gameRunModel.SetPhase(GameRunPhase.BossApproach);
        _gameRunModel.SetPhaseTime(GetPhaseDuration(
            GameRunPhase.BossApproach,
            _gameRunModel.Day,
            _gameFlowSettings.BossApproachDuration));
        StartBossSpawn();
        NotifyBossApproachStarted();
        PlayPhaseBgm(GameRunPhase.BossApproach);
    }

    private void NotifyBossApproachStarted()
    {
        if (BossApproachStartedGlobal == null)
        {
            return;
        }

        BossApproachStartedGlobal();
    }

    private void StartBossFight()
    {
        _gameRunModel.SetPhase(GameRunPhase.BossFight);
        _gameRunModel.SetPhaseTime(0f);
        PlayPhaseBgm(GameRunPhase.BossFight);
    }

    private void PlayPhaseBgm(GameRunPhase phase)
    {
        if (_gameManager == null || _gameRunModel == null)
        {
            return;
        }

        SoundManager soundManager = _gameManager.GetSoundManager();
        if (soundManager == null)
        {
            return;
        }

        GameFlowPhaseData phaseData = GetPhaseData(phase, _gameRunModel.Day);
        string bgmKey = phaseData == null ? null : phaseData.BgmKey;
        soundManager.PlayBgmForPhase(phase, bgmKey);
    }

    private float GetPhaseDuration(GameRunPhase phase, int day, float fallbackDuration)
    {
        if (GameLaunchSettings.TryGetDurationOverride(phase, out float launchModeDuration))
        {
            return launchModeDuration;
        }

        GameFlowPhaseData phaseData = GetPhaseData(phase, day);
        if (phaseData != null && phaseData.DurationSeconds > 0f)
        {
            return phaseData.DurationSeconds;
        }

        return fallbackDuration;
    }

    private GameFlowPhaseData GetPhaseData(GameRunPhase phase, int day)
    {
        if (CanUseGameFlowPhaseData() == false)
        {
            return null;
        }

        GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
        IReadOnlyDictionary<string, GameFlowPhaseData> phaseDataList = gameDataManager.GameFlowPhaseDataList;
        if (phaseDataList == null)
        {
            return null;
        }

        string phaseName = phase.ToString();
        foreach (KeyValuePair<string, GameFlowPhaseData> pair in phaseDataList)
        {
            GameFlowPhaseData phaseData = pair.Value;
            if (IsPhaseDataMatched(phaseData, day, phaseName))
            {
                return phaseData;
            }
        }

        return null;
    }

    private bool CanUseGameFlowPhaseData()
    {
        return _useGameFlowPhaseData
            && GameManager.Inst != null
            && GameManager.Inst.GetGameDataManager() != null;
    }

    private bool IsPhaseDataMatched(GameFlowPhaseData phaseData, int day, string phaseName)
    {
        if (phaseData == null || phaseData.Day != day)
        {
            return false;
        }

        return string.Equals(phaseData.Phase, phaseName, StringComparison.OrdinalIgnoreCase);
    }

    private void StartEnemySpawn(EnemySpawnMode spawnMode)
    {
        if (_enemySpawnDirector == null)
        {
            return;
        }

        _enemySpawnDirector.StartSpawn(spawnMode, _gameRunModel.Day);
    }

    private void StartBossSpawn()
    {
        if (_enemySpawnDirector == null)
        {
            Debug.LogWarning("GameFlowController: 보스 스폰 디렉터가 없어 3일차 보스를 시작할 수 없습니다.");
            return;
        }

        bool isBossSpawnStarted = _enemySpawnDirector.StartSpawn(EnemySpawnMode.Boss, _gameRunModel.Day, HandleBossDefeated);
        if (isBossSpawnStarted == false)
        {
            Debug.LogWarning("GameFlowController: 활성화 가능한 보스 스폰 지점이 없습니다.");
        }
    }

    private void StopEnemySpawn()
    {
        if (_enemySpawnDirector == null)
        {
            return;
        }

        _enemySpawnDirector.StopSpawn();
    }

    private void HandleBossDefeated(EnemyStatus enemyStatus)
    {

        RequestClearByBossDefeated();

    }

    public void RequestClearByBossDefeated(BossStatus bossStatus)
    {
        RequestClearByBossDefeated();
    }

    private void RequestClearByBossDefeated()
    {
        if (_isBossClearHandled)
        {
            return;
        }

        if (_gameRunModel == null)
        {
            return;
        }

        if (_gameRunModel.Phase != GameRunPhase.BossApproach && _gameRunModel.Phase != GameRunPhase.BossFight)
        {
            return;
        }

        _isBossClearHandled = true;

        if (RunResultTracker.Instance != null)
        {
            RunResultTracker.Instance.RecordBossDefeated();
        }

        ClearFlow();
    }

    private void TryOpenCinderHeartSkillSelection()
    {
        if (_openCinderHeartSkillOnMorningReward == false || _gameManager == null)
        {
            return;
        }

        UIManager uiManager = _gameManager.GetUIManager();
        if (uiManager == null)
        {
            return;
        }

        List<CinderHeartSkillData> skillOptions = GetMorningRewardSkillOptions();
        if (skillOptions.Count <= 0)
        {
            return;
        }

        PauseGameForCinderHeartSkillSelection();
        uiManager.OpenCinderHeartSkillSelectionUI(skillOptions, HandleCinderHeartSkillSelectionClosed);
    }

    private List<CinderHeartSkillData> GetMorningRewardSkillOptions()
    {
        List<CinderHeartSkillData> skillOptions = new List<CinderHeartSkillData>();
        if (_gameManager == null)
        {
            return skillOptions;
        }

        GameDataManager gameDataManager = _gameManager.GetGameDataManager();
        if (gameDataManager == null)
        {
            return skillOptions;
        }

        CinderHeartRewardOptionSelector selector = new CinderHeartRewardOptionSelector(
            _morningRewardSkillIds,
            MorningRewardOptionCount);
        int currentDay = _gameRunModel == null ? GameRunModel.FirstDay : _gameRunModel.Day;
        return selector.SelectOptions(gameDataManager, currentDay, IsPlayerDead());
    }

    private bool IsPlayerDead()
    {
        PlayerStatus playerStatus = UnityEngine.Object.FindFirstObjectByType<PlayerStatus>();
        return playerStatus != null && playerStatus.IsDead();
    }

    private void PauseGameForCinderHeartSkillSelection()
    {
        if (_isWaitingForCinderHeartSkillSelection)
        {
            return;
        }

        _isWaitingForCinderHeartSkillSelection = true;
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private void HandleCinderHeartSkillSelectionClosed()
    {
        if (_isWaitingForCinderHeartSkillSelection == false)
        {
            return;
        }

        RestoreTimeScaleIfRewardSelectionIsOpen();
    }

    private void RestoreTimeScaleIfRewardSelectionIsOpen()
    {
        if (_isWaitingForCinderHeartSkillSelection == false)
        {
            return;
        }

        Time.timeScale = _previousTimeScale <= 0f ? 1f : _previousTimeScale;
        _isWaitingForCinderHeartSkillSelection = false;
    }

    private void OnValidate()
    {
        if (_gameFlowSettings == null)
        {
            _gameFlowSettings = new GameFlowSettings();
        }

        _gameFlowSettings.ClampValues();
    }
}
