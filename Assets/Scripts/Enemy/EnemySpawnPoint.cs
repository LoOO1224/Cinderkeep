using Cinderkeep.Gameplay;
using UnityEngine;

public enum EnemySpawnStep
{
    Step1 = 1, // 1단계: 기본 몬스터 후보
    Step2 = 2, // 2단계: 강화 몬스터 후보
    Step3 = 3  // 3단계: 특수 몬스터 후보
}

// 씬에 배치되는 적 스폰 지점입니다.
// GameFlowEnemySpawnDirector가 낮/밤/보스 모드와 일차를 알려주면, 이 클래스가 실제 생성 숫자를 계산합니다.
// 생존 적 추적은 EnemySpawnRuntimeTracker가 맡고, 위치 계산은 EnemySpawnPositionSelector가 맡습니다.
public sealed class EnemySpawnPoint : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameObjectManager _gameObjectManager;
    [SerializeField] private EnemyLoopConnector _enemyLoopConnector;

    [Header("Spawn Point")]
    [SerializeField] private int _spawnPointId;
    [SerializeField] private bool _isActive = true;

    [Header("Spawn Step")]
    [SerializeField] private EnemySpawnStep _spawnStep = EnemySpawnStep.Step1;
    [SerializeField] private string _step1EnemyDataId = "ice_zombie";
    [SerializeField] private string _step2EnemyDataId = "ice_zombie";
    [SerializeField] private string _step3EnemyDataId = "ice_zombie";

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] _step1EnemyPrefabs;
    [SerializeField] private GameObject[] _step2EnemyPrefabs;
    [SerializeField] private GameObject[] _step3EnemyPrefabs;

    [Header("Day Spawn Rules")]
    [SerializeField] private EnemySpawnRule _day1Rule = new EnemySpawnRule(25f, 2, 9, true);
    [SerializeField] private EnemySpawnRule _day2Rule = new EnemySpawnRule(20f, 2, 12, true);
    [SerializeField] private EnemySpawnRule _day3Rule = new EnemySpawnRule(15f, 3, 16, true);

    [Header("Night Spawn Rules")]
    [SerializeField] private EnemySpawnRule _night1Rule = new EnemySpawnRule(12f, 2, 12, true);
    [SerializeField] private EnemySpawnRule _night2Rule = new EnemySpawnRule(8f, 3, 18, true);
    [SerializeField] private EnemySpawnRule _night3Rule = new EnemySpawnRule(6f, 4, 24, true);

    [Header("Boss Spawn Rule")]
    [SerializeField] private EnemySpawnRule _bossRule = new EnemySpawnRule(5f, 4, 28, true);

    [Header("Spawn Position")]
    [SerializeField] private float _spawnSpacing = 1.2f;
    [SerializeField] private Transform _centerTransform;
    [SerializeField] private Transform[] _spawnCandidatePoints;

    [Header("Gizmo")]
    [SerializeField] private bool _showGizmo = true;
    [SerializeField] private Color _gizmoColor = Color.red;

    private readonly EnemySpawnRuntimeTracker _runtimeTracker = new EnemySpawnRuntimeTracker();
    private readonly EnemySpawnPositionSelector _positionSelector = new EnemySpawnPositionSelector();
    private EnemySpawnMode _spawnMode = EnemySpawnMode.Day;
    private int _currentDay = 1;
    private float _lastSpawnTime;

    public int SpawnPointId
    {
        get
        {
            return _spawnPointId;
        }
    }

    public EnemySpawnStep SpawnStep
    {
        get
        {
            return _spawnStep;
        }
    }

    public EnemySpawnMode SpawnMode
    {
        get
        {
            return _spawnMode;
        }
    }

    public void Initialize(GameObjectManager gameObjectManager, EnemyLoopConnector enemyLoopConnector)
    {
        _gameObjectManager = gameObjectManager;
        _enemyLoopConnector = enemyLoopConnector;
    }

    private void Start()
    {
        InitializeCenter();
        ResetSpawnTime();
    }

    private void Update()
    {
        UpdateSpawn();
    }

    public void SetSpawnStep(EnemySpawnStep spawnStep)
    {
        _spawnStep = spawnStep;
    }

    public void SetSpawnMode(EnemySpawnMode spawnMode, int day)
    {
        _spawnMode = spawnMode;
        _currentDay = Mathf.Max(1, day);
        ResetSpawnTime();

        EnemySpawnRule spawnRule = GetCurrentSpawnRule();
        if (spawnRule.SpawnOnModeStart == true)
        {
            SpawnEnemiesOnce();
        }
    }

    public void SetSpawnPointActive(bool isActive)
    {
        _isActive = isActive;
    }

    public void ResetSpawnTime()
    {
        _lastSpawnTime = Time.time;
    }

    public void SpawnEnemiesOnce()
    {
        SpawnEnemiesByCurrentStep();
        ResetSpawnTime();
    }

    private void InitializeCenter()
    {
        if (_centerTransform == null)
        {
            _centerTransform = transform;
        }
    }

    private void UpdateSpawn()
    {
        if (CanSpawn() == false)
        {
            return;
        }

        SpawnEnemiesOnce();
    }

    private bool CanSpawn()
    {
        if (_isActive == false)
        {
            return false;
        }

        if (_gameObjectManager == null)
        {
            return false;
        }

        EnemySpawnRule spawnRule = GetCurrentSpawnRule();
        if (Time.time < _lastSpawnTime + spawnRule.SpawnInterval)
        {
            return false;
        }

        if (_runtimeTracker.GetAliveEnemyCount() >= spawnRule.MaxAliveEnemyCount)
        {
            return false;
        }

        return true;
    }

    private void SpawnEnemiesByCurrentStep()
    {
        GameObject[] enemyPrefabs = GetEnemyPrefabsByStep();
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            return;
        }

        EnemySpawnRule spawnRule = GetCurrentSpawnRule();
        int spawnCount = GetAllowedSpawnCount(spawnRule);
        string enemyDataId = GetEnemyDataIdByStep();

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject enemyPrefab = GetRandomEnemyPrefab(enemyPrefabs);
            SpawnEnemy(enemyPrefab, i, spawnCount, enemyDataId);
        }
    }

    private int GetAllowedSpawnCount(EnemySpawnRule spawnRule)
    {
        int aliveCount = _runtimeTracker.GetAliveEnemyCount();
        int remainingCount = spawnRule.MaxAliveEnemyCount - aliveCount;
        int spawnCount = Mathf.Min(spawnRule.SpawnCountPerWave, remainingCount);
        return Mathf.Max(0, spawnCount);
    }

    private GameObject GetRandomEnemyPrefab(GameObject[] enemyPrefabs)
    {
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        return enemyPrefabs[randomIndex];
    }

    private void SpawnEnemy(GameObject enemyPrefab, int index, int totalCount, string enemyDataId)
    {
        if (enemyPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = _positionSelector.GetSpawnPosition(
            _centerTransform,
            _spawnCandidatePoints,
            _spawnSpacing,
            index,
            totalCount);
        Quaternion spawnRotation = _positionSelector.GetSpawnRotation(_centerTransform);
        GameObject createdEnemy = _gameObjectManager.CreateGameObject(enemyPrefab, spawnPosition, spawnRotation);
        _runtimeTracker.RegisterEnemy(createdEnemy);
        InitializeCreatedEnemy(createdEnemy, enemyDataId);
    }

    private void InitializeCreatedEnemy(GameObject createdEnemy, string enemyDataId)
    {
        if (_enemyLoopConnector == null)
        {
            return;
        }

        _enemyLoopConnector.InitializeEnemyObject(createdEnemy, enemyDataId);
    }

    private GameObject[] GetEnemyPrefabsByStep()
    {
        switch (_spawnStep)
        {
            case EnemySpawnStep.Step1:
                return _step1EnemyPrefabs;
            case EnemySpawnStep.Step2:
                return _step2EnemyPrefabs;
            case EnemySpawnStep.Step3:
                return _step3EnemyPrefabs;
        }

        return _step1EnemyPrefabs;
    }

    private string GetEnemyDataIdByStep()
    {
        switch (_spawnStep)
        {
            case EnemySpawnStep.Step1:
                return _step1EnemyDataId;
            case EnemySpawnStep.Step2:
                return _step2EnemyDataId;
            case EnemySpawnStep.Step3:
                return _step3EnemyDataId;
        }

        return _step1EnemyDataId;
    }

    private EnemySpawnRule GetCurrentSpawnRule()
    {
        if (_spawnMode == EnemySpawnMode.Boss)
        {
            return _bossRule;
        }

        if (_spawnMode == EnemySpawnMode.Night)
        {
            return GetNightSpawnRule();
        }

        return GetDaySpawnRule();
    }

    private EnemySpawnRule GetDaySpawnRule()
    {
        if (_currentDay <= 1)
        {
            return _day1Rule;
        }

        if (_currentDay == 2)
        {
            return _day2Rule;
        }

        return _day3Rule;
    }

    private EnemySpawnRule GetNightSpawnRule()
    {
        if (_currentDay <= 1)
        {
            return _night1Rule;
        }

        if (_currentDay == 2)
        {
            return _night2Rule;
        }

        return _night3Rule;
    }

    private void OnValidate()
    {
        ClampSpawnRules();

        if (_spawnSpacing < 0f)
        {
            _spawnSpacing = 0f;
        }
    }

    private void ClampSpawnRules()
    {
        ClampSpawnRule(_day1Rule);
        ClampSpawnRule(_day2Rule);
        ClampSpawnRule(_day3Rule);
        ClampSpawnRule(_night1Rule);
        ClampSpawnRule(_night2Rule);
        ClampSpawnRule(_night3Rule);
        ClampSpawnRule(_bossRule);
    }

    private void ClampSpawnRule(EnemySpawnRule spawnRule)
    {
        if (spawnRule == null)
        {
            return;
        }

        spawnRule.ClampValues();
    }

    private void OnDrawGizmosSelected()
    {
        if (_showGizmo == false)
        {
            return;
        }

        Transform center = _centerTransform;
        if (center == null)
        {
            center = transform;
        }

        Gizmos.color = _gizmoColor;
        DrawSpawnPositionGizmo(center.position, _step1EnemyPrefabs);
        DrawSpawnPositionGizmo(center.position, _step2EnemyPrefabs);
        DrawSpawnPositionGizmo(center.position, _step3EnemyPrefabs);
    }

    private void DrawSpawnPositionGizmo(Vector3 centerPosition, GameObject[] enemyPrefabs)
    {
        if (enemyPrefabs == null)
        {
            return;
        }

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            Vector3 position = _positionSelector.GetGizmoPosition(centerPosition, _spawnSpacing, i, enemyPrefabs.Length);
            Gizmos.DrawWireSphere(position, 0.25f);
        }
    }
}
