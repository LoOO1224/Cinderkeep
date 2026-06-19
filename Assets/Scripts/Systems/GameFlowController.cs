using Cinderkeep.Gameplay;
using UnityEngine;

// 3일 게임 루프의 시간표를 지휘하는 컨트롤러입니다.
// 실제 전투, UI 표시, 적 행동, 스폰 지점 제어는 각 전용 컴포넌트가 맡습니다.
// 이 클래스는 현재 페이즈와 다음 페이즈로 넘어가는 순서만 관리합니다.
public sealed class GameFlowController : MonoBehaviour, IGameInitializable
{
    [Header("Day Loop Time")]
    [Tooltip("낮 페이즈 지속 시간입니다. 180이면 3분입니다.")]
    [SerializeField] private float _dayDuration = 180f;
    [Tooltip("밤 페이즈 지속 시간입니다. 120이면 2분입니다.")]
    [SerializeField] private float _nightDuration = 120f;
    [Tooltip("밤이 끝난 뒤 다음 날로 넘어가기 전 보상 시간을 의미합니다.")]
    [SerializeField] private float _morningRewardDuration = 15f;

    [Header("Boss Flow Time")]
    [Tooltip("마지막 날 밤 이후 보스 접근 페이즈가 지속되는 시간입니다.")]
    [SerializeField] private float _bossApproachDuration = 180f;

    [Header("Enemy Spawn Director")]
    [Tooltip("현재 낮, 밤, 보스 페이즈에 맞춰 적 스폰 지점들을 켜고 끄는 컴포넌트입니다.")]
    [SerializeField] private GameFlowEnemySpawnDirector _enemySpawnDirector;

    private GameManager _gameManager;
    private GameRunModel _gameRunModel;
    private bool _isInitialized;
    private bool _isFlowRunning;

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
        StopEnemySpawn();
    }

    public void ClearFlow()
    {
        if (_gameRunModel == null)
        {
            return;
        }

        _isFlowRunning = false;
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
        _gameRunModel.SetRemainingTime(_dayDuration);
        StartEnemySpawn(EnemySpawnMode.Day);
    }

    private void StartNight()
    {
        _gameRunModel.SetPhase(GameRunPhase.Night);
        _gameRunModel.SetRemainingTime(_nightDuration);
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
        _gameRunModel.SetRemainingTime(_morningRewardDuration);
    }

    private void StartNextDay()
    {
        _gameRunModel.AdvanceDay();
        StartDay(_gameRunModel.Day);
    }

    private void StartBossApproach()
    {
        _gameRunModel.SetPhase(GameRunPhase.BossApproach);
        _gameRunModel.SetRemainingTime(_bossApproachDuration);
        StartEnemySpawn(EnemySpawnMode.Boss);
    }

    private void StartBossFight()
    {
        _gameRunModel.SetPhase(GameRunPhase.BossFight);
        _gameRunModel.SetRemainingTime(0f);
        StartEnemySpawn(EnemySpawnMode.Boss);
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

    private void OnValidate()
    {
        ClampDuration();
    }

    private void ClampDuration()
    {
        if (_dayDuration < 1f)
        {
            _dayDuration = 1f;
        }

        if (_nightDuration < 1f)
        {
            _nightDuration = 1f;
        }

        if (_morningRewardDuration < 1f)
        {
            _morningRewardDuration = 1f;
        }

        if (_bossApproachDuration < 1f)
        {
            _bossApproachDuration = 1f;
        }
    }
}
