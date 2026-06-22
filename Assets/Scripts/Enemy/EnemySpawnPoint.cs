using System;
using System.Collections.Generic;
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
// 살아 있는 적 추적은 EnemySpawnRuntimeTracker가 맡고, 위치 계산은 EnemySpawnPositionSelector가 맡습니다.
public sealed class EnemySpawnPoint : MonoBehaviour
{
    [Header("Managers")]
    [Tooltip("적 프리팹을 생성하고 InstanceId를 부여할 관리자입니다.")]
    [SerializeField] private GameObjectManager _gameObjectManager;
    [Tooltip("생성된 적의 EnemyStatus, EnemyAttack, EnemyMovement 같은 정식 컴포넌트를 초기화합니다.")]
    [SerializeField] private EnemyLoopConnector _enemyLoopConnector;

    [Header("Spawn Point")]
    [Tooltip("스폰 지점을 구분하기 위한 번호입니다. 디버그와 QA 기록용입니다.")]
    [SerializeField] private int _spawnPointId;
    [Tooltip("현재 이 스폰 지점이 적을 생성할 수 있는지 결정합니다.")]
    [SerializeField] private bool _isActive = true;

    [Header("Spawn Step")]
    [Tooltip("현재 스폰 지점이 사용하는 단계입니다. 1단계는 기본, 2단계는 강화, 3단계는 특수 후보입니다.")]
    [SerializeField] private EnemySpawnStep _spawnStep = EnemySpawnStep.Step1;
    [Tooltip("1단계 적에게 적용할 EnemyData ID입니다. enemies.json의 _id와 같아야 합니다.")]
    [SerializeField] private string _step1EnemyDataId = "ice_zombie";
    [Tooltip("2단계 적에게 적용할 EnemyData ID입니다. enemies.json의 _id와 같아야 합니다.")]
    [SerializeField] private string _step2EnemyDataId = "ice_zombie";
    [Tooltip("3단계 적에게 적용할 EnemyData ID입니다. enemies.json의 _id와 같아야 합니다.")]
    [SerializeField] private string _step3EnemyDataId = "ice_zombie";

    [Header("Enemy Prefabs")]
    [Tooltip("1단계에서 무작위로 뽑을 적 프리팹 목록입니다.")]
    [SerializeField] private GameObject[] _step1EnemyPrefabs;
    [Tooltip("2단계에서 무작위로 뽑을 적 프리팹 목록입니다.")]
    [SerializeField] private GameObject[] _step2EnemyPrefabs;
    [Tooltip("3단계에서 무작위로 뽑을 적 프리팹 목록입니다.")]
    [SerializeField] private GameObject[] _step3EnemyPrefabs;

    [Header("Day Spawn Rules")]
    [Tooltip("1일차 낮 스폰 fallback 규칙입니다. enemy_spawn_rules.json 데이터가 없을 때 사용합니다.")]
    [SerializeField] private EnemySpawnRule _day1Rule = new EnemySpawnRule(25f, 2, 9, true);
    [Tooltip("2일차 낮 스폰 fallback 규칙입니다. enemy_spawn_rules.json 데이터가 없을 때 사용합니다.")]
    [SerializeField] private EnemySpawnRule _day2Rule = new EnemySpawnRule(20f, 2, 12, true);
    [Tooltip("3일차 낮 스폰 fallback 규칙입니다. enemy_spawn_rules.json 데이터가 없을 때 사용합니다.")]
    [SerializeField] private EnemySpawnRule _day3Rule = new EnemySpawnRule(15f, 3, 16, true);

    [Header("Night Spawn Rules")]
    [Tooltip("1일차 밤 스폰 fallback 규칙입니다. enemy_spawn_rules.json 데이터가 없을 때 사용합니다.")]
    [SerializeField] private EnemySpawnRule _night1Rule = new EnemySpawnRule(12f, 2, 12, true);
    [Tooltip("2일차 밤 스폰 fallback 규칙입니다. enemy_spawn_rules.json 데이터가 없을 때 사용합니다.")]
    [SerializeField] private EnemySpawnRule _night2Rule = new EnemySpawnRule(8f, 3, 18, true);
    [Tooltip("3일차 밤 스폰 fallback 규칙입니다. enemy_spawn_rules.json 데이터가 없을 때 사용합니다.")]
    [SerializeField] private EnemySpawnRule _night3Rule = new EnemySpawnRule(6f, 4, 24, true);

    [Header("Boss Spawn Rule")]
    [Tooltip("보스 접근 또는 보스 전투 페이즈에서 사용하는 fallback 스폰 규칙입니다.")]
    [SerializeField] private EnemySpawnRule _bossRule = new EnemySpawnRule(5f, 4, 28, true);

    [Header("Spawn Rule Data")]
    [Tooltip("true이면 enemy_spawn_rules.json의 스폰 규칙을 우선 사용합니다. 데이터가 없으면 위 fallback 규칙을 사용합니다.")]
    [SerializeField] private bool _useSpawnRuleData = true;

    [Header("Spawn Position")]
    [Tooltip("한 웨이브에서 여러 적이 동시에 생성될 때 서로 벌어지는 간격입니다.")]
    [SerializeField] private float _spawnSpacing = 1.2f;
    [Tooltip("스폰 후보 지점이 비어 있을 때 기준으로 사용할 중심 위치입니다.")]
    [SerializeField] private Transform _centerTransform;
    [Tooltip("실제로 적이 생성될 수 있는 후보 위치입니다. 비어 있으면 Center Transform 위치를 사용합니다.")]
    [SerializeField] private Transform[] _spawnCandidatePoints;

    [Header("Gizmo")]
    [Tooltip("Scene View에서 스폰 후보 위치를 확인할지 결정합니다.")]
    [SerializeField] private bool _showGizmo = true;
    [Tooltip("Scene View에 표시할 스폰 위치 Gizmo 색상입니다.")]
    [SerializeField] private Color _gizmoColor = Color.red;

    private readonly EnemySpawnRuntimeTracker _runtimeTracker = new EnemySpawnRuntimeTracker();
    private readonly EnemySpawnPositionSelector _positionSelector = new EnemySpawnPositionSelector();
    private EnemySpawnRuleData _currentSpawnRuleData;
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
        SelectCurrentSpawnRuleData();
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
        SelectCurrentSpawnRuleData();
        ResetSpawnTime();

        _runtimeTracker.SetCinderHeartChaseEnabledForAliveEnemies(CanChaseCinderHeartInCurrentMode());

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
        SelectCurrentSpawnRuleData();
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
        int requestedCount = GetRequestedSpawnCount(spawnRule);
        int spawnCount = Mathf.Min(requestedCount, remainingCount);
        return Mathf.Max(0, spawnCount);
    }

    private int GetRequestedSpawnCount(EnemySpawnRule spawnRule)
    {
        if (_currentSpawnRuleData == null)
        {
            return spawnRule.SpawnCountPerWave;
        }

        int minCount = Mathf.Max(0, _currentSpawnRuleData.MinSpawnCount);
        int maxCount = Mathf.Max(minCount, _currentSpawnRuleData.MaxSpawnCount);
        return UnityEngine.Random.Range(minCount, maxCount + 1);
    }

    private GameObject GetRandomEnemyPrefab(GameObject[] enemyPrefabs)
    {
        int randomIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
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
        ApplySpawnModeToEnemy(createdEnemy);
    }

    private void ApplySpawnModeToEnemy(GameObject createdEnemy)
    {
        if (createdEnemy == null)
        {
            return;
        }

        EnemyMovement enemyMovement = createdEnemy.GetComponent<EnemyMovement>();
        if (enemyMovement == null)
        {
            return;
        }

        enemyMovement.SetCinderHeartChaseEnabled(CanChaseCinderHeartInCurrentMode());
    }

    private bool CanChaseCinderHeartInCurrentMode()
    {
        return _spawnMode != EnemySpawnMode.Day;
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
        if (_currentSpawnRuleData != null && string.IsNullOrEmpty(_currentSpawnRuleData.EnemyId) == false)
        {
            return _currentSpawnRuleData.EnemyId;
        }

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
        // enemy_spawn_rules.json에서 현재 일차/페이즈에 맞는 규칙을 찾으면 그 값을 우선 사용합니다.
        // 데이터가 없으면 Inspector의 낮/밤/보스 fallback 규칙을 사용합니다.
        EnemySpawnRule inspectorRule = GetInspectorSpawnRule();
        if (_currentSpawnRuleData == null)
        {
            return inspectorRule;
        }

        float spawnInterval = inspectorRule.SpawnInterval;
        if (_currentSpawnRuleData.SpawnInterval > 0f)
        {
            spawnInterval = _currentSpawnRuleData.SpawnInterval;
        }

        int spawnCountPerWave = inspectorRule.SpawnCountPerWave;
        if (_currentSpawnRuleData.MaxSpawnCount > 0)
        {
            spawnCountPerWave = _currentSpawnRuleData.MaxSpawnCount;
        }

        int maxAliveEnemyCount = inspectorRule.MaxAliveEnemyCount;
        if (_currentSpawnRuleData.MaxAliveCount > 0)
        {
            maxAliveEnemyCount = _currentSpawnRuleData.MaxAliveCount;
        }

        return new EnemySpawnRule(spawnInterval, spawnCountPerWave, maxAliveEnemyCount, inspectorRule.SpawnOnModeStart);
    }

    private EnemySpawnRule GetInspectorSpawnRule()
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

    private void SelectCurrentSpawnRuleData()
    {
        // 페이즈가 바뀔 때마다 enemy_spawn_rules.json 후보 중 weight 기반으로 하나를 선택합니다.
        // QA는 JSON의 day, phase, weight, spawn count 값을 조절해 난이도를 빠르게 실험할 수 있습니다.
        _currentSpawnRuleData = GetRandomSpawnRuleData(GetCurrentPhaseName());
    }

    private EnemySpawnRuleData GetRandomSpawnRuleData(string phase)
    {
        if (CanUseSpawnRuleData(phase) == false)
        {
            return null;
        }

        GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
        IReadOnlyDictionary<string, EnemySpawnRuleData> ruleDataList = gameDataManager.EnemySpawnRuleDataList;
        int totalWeight = GetTotalSpawnRuleWeight(ruleDataList, phase);
        if (totalWeight <= 0)
        {
            return null;
        }

        int selectedWeight = UnityEngine.Random.Range(1, totalWeight + 1);
        int accumulatedWeight = 0;

        foreach (KeyValuePair<string, EnemySpawnRuleData> pair in ruleDataList)
        {
            EnemySpawnRuleData spawnRuleData = pair.Value;
            if (IsSpawnRuleCandidate(spawnRuleData, phase) == false)
            {
                continue;
            }

            accumulatedWeight += GetRuleWeight(spawnRuleData);
            if (selectedWeight <= accumulatedWeight)
            {
                return spawnRuleData;
            }
        }

        return null;
    }

    private bool CanUseSpawnRuleData(string phase)
    {
        if (_useSpawnRuleData == false)
        {
            return false;
        }

        if (string.IsNullOrEmpty(phase))
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

    private int GetTotalSpawnRuleWeight(IReadOnlyDictionary<string, EnemySpawnRuleData> ruleDataList, string phase)
    {
        if (ruleDataList == null)
        {
            return 0;
        }

        int totalWeight = 0;
        foreach (KeyValuePair<string, EnemySpawnRuleData> pair in ruleDataList)
        {
            EnemySpawnRuleData spawnRuleData = pair.Value;
            if (IsSpawnRuleCandidate(spawnRuleData, phase) == true)
            {
                totalWeight += GetRuleWeight(spawnRuleData);
            }
        }

        return totalWeight;
    }

    private bool IsSpawnRuleCandidate(EnemySpawnRuleData spawnRuleData, string phase)
    {
        if (spawnRuleData == null)
        {
            return false;
        }

        if (spawnRuleData.Day != _currentDay)
        {
            return false;
        }

        return string.Equals(spawnRuleData.Phase, phase, StringComparison.OrdinalIgnoreCase);
    }

    private int GetRuleWeight(EnemySpawnRuleData spawnRuleData)
    {
        if (spawnRuleData == null)
        {
            return 0;
        }

        return Mathf.Max(0, spawnRuleData.SpawnWeight);
    }

    private string GetCurrentPhaseName()
    {
        if (_spawnMode == EnemySpawnMode.Night)
        {
            return "Night";
        }

        if (_spawnMode == EnemySpawnMode.Day)
        {
            return "Day";
        }

        return "Boss";
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
