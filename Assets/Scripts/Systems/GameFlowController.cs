using System;
using System.Collections.Generic;
using Cinderkeep.Gameplay;
using UnityEngine;

// 3일 게임 루프의 페이즈 전환을 지휘하는 컨트롤러입니다.
// 실제 전투, UI 표시, 적 행동, 스폰 지점 제어는 각 전용 컴포넌트가 담당합니다.
// 이 클래스는 현재 페이즈에서 다음 페이즈로 넘어가는 순서만 관리합니다.
public sealed class GameFlowController : MonoBehaviour, IGameInitializable
{
    [Header("Flow Settings")]
    [Tooltip("낮, 밤, 아침 보상, 보스 접근 시간의 fallback 설정입니다.")]
    [SerializeField] private GameFlowSettings _gameFlowSettings = new GameFlowSettings();

    [Header("Flow Data")]
    [Tooltip("true이면 game_flow_phases.json의 페이즈 시간을 우선 사용합니다.")]
    [SerializeField] private bool _useGameFlowPhaseData = true;

    [Header("Enemy Spawn Director")]
    [Tooltip("현재 낮, 밤, 보스 페이즈에 맞춰 적 스폰 지점들을 켜고 끄는 컴포넌트입니다.")]
    [SerializeField] private GameFlowEnemySpawnDirector _enemySpawnDirector;

    [Header("CinderHeart Reward")]
    [Tooltip("아침 보상 페이즈에 CinderHeart 스킬 선택창을 엽니다.")]
    [SerializeField] private bool _openCinderHeartSkillOnMorningReward = true;
    [Tooltip("아침 보상에서 고정 노출할 CinderHeart 스킬 ID입니다. 랜덤 선택은 후속 작업에서 분리합니다.")]
    [SerializeField] private string[] _morningRewardSkillIds =
    {
        "cinderheart_attack_damage_5",
        "cinderheart_max_health_100",
        "cinderheart_player_heal_30"
    };

    private GameManager _gameManager;
    private GameRunModel _gameRunModel;
    private bool _isInitialized;
    private bool _isFlowRunning;
    private bool _isWaitingForCinderHeartSkillSelection;
    private float _previousTimeScale = 1f;

    public bool IsInitialized
    {
        get
        {
            return _isInitialized;
        }
    }

    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
        if (_gameManager == null)
        {
            _gameRunModel = null;
            return;
        }

        _gameRunModel = _gameManager.GameRunModel;
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
        StartDay(GameRunModel.FirstDay);
    }

    public void StopFlowAsGameOver()
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
    }

    private void UpdateFlowTime()
    {
        if (CanUpdateFlow() == false)
        {
            return;
        }

        float remainingTime = _gameRunModel.RemainingTime - Time.deltaTime;
        _gameRunModel.SetRemainingTime(remainingTime);

        if (_gameRunModel.RemainingTime > 0f)
        {
            return;
        }

        AdvancePhase();
    }

    private bool CanUpdateFlow()
    {
        if (_isFlowRunning == false)
        {
            return false;
        }

        if (_gameRunModel == null)
        {
            return false;
        }

        if (_gameRunModel.IsPlaying == false)
        {
            return false;
        }

        if (_gameRunModel.Phase == GameRunPhase.BossFight)
        {
            return false;
        }

        if (_isWaitingForCinderHeartSkillSelection == true)
        {
            return false;
        }

        return true;
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
        _gameRunModel.SetDay(day);
        _gameRunModel.SetPhase(GameRunPhase.Day);
        _gameRunModel.SetPhaseTime(GetPhaseDuration(GameRunPhase.Day, day, _gameFlowSettings.DayDuration));
        StartEnemySpawn(EnemySpawnMode.Day);
    }

    private void StartNight()
    {
        _gameRunModel.SetPhase(GameRunPhase.Night);
        _gameRunModel.SetPhaseTime(GetPhaseDuration(GameRunPhase.Night, _gameRunModel.Day, _gameFlowSettings.NightDuration));
        StartEnemySpawn(EnemySpawnMode.Night);
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
        TryOpenCinderHeartSkillSelection();
    }

    private void StartNextDay()
    {
        _gameRunModel.AdvanceDay();
        StartDay(_gameRunModel.Day);
    }

    private void StartBossApproach()
    {
        _gameRunModel.SetPhase(GameRunPhase.BossApproach);
        _gameRunModel.SetPhaseTime(GetPhaseDuration(
            GameRunPhase.BossApproach,
            _gameRunModel.Day,
            _gameFlowSettings.BossApproachDuration));
        StartEnemySpawn(EnemySpawnMode.Boss);
    }

    private void StartBossFight()
    {
        _gameRunModel.SetPhase(GameRunPhase.BossFight);
        _gameRunModel.SetPhaseTime(0f);
        StartEnemySpawn(EnemySpawnMode.Boss);
    }

    private float GetPhaseDuration(GameRunPhase phase, int day, float fallbackDuration)
    {
        // 낮/밤/아침/보스 시간은 game_flow_phases.json에서 먼저 가져옵니다.
        // JSON 값이 비어 있거나 잘못되면 Inspector의 GameFlowSettings 값을 fallback으로 사용합니다.
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
            if (IsPhaseDataMatched(phaseData, day, phaseName) == true)
            {
                return phaseData;
            }
        }

        return null;
    }

    private bool CanUseGameFlowPhaseData()
    {
        if (_useGameFlowPhaseData == false)
        {
            return false;
        }

        if (GameManager.Inst == null)
        {
            return false;
        }

        if (GameManager.Inst.GetGameDataManager() == null)
        {
            return false;
        }

        return true;
    }

    private bool IsPhaseDataMatched(GameFlowPhaseData phaseData, int day, string phaseName)
    {
        if (phaseData == null)
        {
            return false;
        }

        if (phaseData.Day != day)
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

    private void StopEnemySpawn()
    {
        if (_enemySpawnDirector == null)
        {
            return;
        }

        _enemySpawnDirector.StopSpawn();
    }

    private void TryOpenCinderHeartSkillSelection()
    {
        // MorningReward 페이즈에서 cinderheart_skills.json의 스킬 ID를 읽어 선택 UI를 엽니다.
        // 선택 후보 랜덤화는 후속 작업이며, 현재는 _morningRewardSkillIds 배열 순서를 사용합니다.
        if (_openCinderHeartSkillOnMorningReward == false)
        {
            return;
        }

        if (_gameManager == null)
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

        if (_morningRewardSkillIds == null)
        {
            return skillOptions;
        }

        for (int i = 0; i < _morningRewardSkillIds.Length; i++)
        {
            CinderHeartSkillData skillData = gameDataManager.GetCinderHeartSkill(_morningRewardSkillIds[i]);
            if (skillData == null)
            {
                continue;
            }

            skillOptions.Add(skillData);
        }

        return skillOptions;
    }

    private void PauseGameForCinderHeartSkillSelection()
    {
        // 보상 선택 중에는 전투/스폰/타이머가 흐르지 않게 Time.timeScale을 잠시 멈춥니다.
        // UI를 닫을 때 RestoreTimeScaleIfRewardSelectionIsOpen에서 반드시 이전 값으로 복구합니다.
        if (_isWaitingForCinderHeartSkillSelection == true)
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

        if (_previousTimeScale <= 0f)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = _previousTimeScale;
        }

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
