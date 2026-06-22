using Cinderkeep.Gameplay;
using System;
using UnityEngine;
using UnityEngine.AI;

// 적 관련 런타임 초기화와 기준 검증만 담당하는 컴포넌트입니다.
// 4.35 기준 적 프리팹의 기준 컴포넌트는 Damageable, EnemyStatus, EnemyAttack, EnemyDetector, EnemyMovement, EnemyBrain입니다.
// EnemyLoopConnector는 이 컴포넌트들에 EnemyData와 CinderHeart 타깃을 연결합니다.
// 정식 적 프리팹은 위 컴포넌트를 미리 가지고 있어야 하며, 런타임 AddComponent는 테스트 후보 프리팹을 임시로 살릴 때만 쓰는 예외입니다.
public sealed class EnemyLoopConnector : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameDataManager _gameDataManager;

    [Header("World")]
    [SerializeField] private Transform _cinderHeartTarget;
    [SerializeField] private Camera _gameCamera;

    [Header("Enemies")]
    [SerializeField] private EnemyRuntimeSet[] _enemyRuntimeSets;

    [Header("Fallback")]
    [Tooltip("정식 적 프리팹에 컴포넌트가 빠졌을 때 임시 런타임 추가를 허용할지 결정합니다. 정식 작업에서는 false를 유지합니다.")]
    [SerializeField] private bool _allowFallbackComponentAdd = false;

    public void Initialize(
        GameDataManager gameDataManager,
        Transform cinderHeartTarget,
        Camera gameCamera,
        EnemyRuntimeSet[] enemyRuntimeSets)
    {
        _gameDataManager = gameDataManager;
        _cinderHeartTarget = cinderHeartTarget;
        _gameCamera = gameCamera;
        _enemyRuntimeSets = enemyRuntimeSets;
        ConnectGameDataManagerIfNeeded();
    }

    public void InitializeEnemies()
    {
        if (_enemyRuntimeSets == null)
        {
            return;
        }

        for (int i = 0; i < _enemyRuntimeSets.Length; i++)
        {
            InitializeEnemyRuntimeSet(_enemyRuntimeSets[i]);
        }
    }

    public void InitializeEnemyObject(GameObject enemyObject, string enemyDataId)
    {
        if (enemyObject == null)
        {
            return;
        }

        EnemyData enemyData = GetEnemyData(enemyDataId);
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyLoopConnector: EnemyData가 없습니다. id=" + enemyDataId);
            return;
        }

        PrepareEnemyPhysics(enemyObject);

        Damageable damageable = GetRuntimeComponent<Damageable>(enemyObject, "Damageable");
        EnemyStatus enemyStatus = GetRuntimeComponent<EnemyStatus>(enemyObject, "EnemyStatus");
        EnemyAttack enemyAttack = GetRuntimeComponent<EnemyAttack>(enemyObject, "EnemyAttack");
        EnemyDetector enemyDetector = GetRuntimeComponent<EnemyDetector>(enemyObject, "EnemyDetector");
        EnemyMovement enemyMovement = GetRuntimeComponent<EnemyMovement>(enemyObject, "EnemyMovement");
        EnemyBrain enemyBrain = GetRuntimeComponent<EnemyBrain>(enemyObject, "EnemyBrain");
        EnemyHud enemyHud = enemyObject.GetComponentInChildren<EnemyHud>();

        if (damageable == null)
        {
            Debug.LogWarning("EnemyLoopConnector: Damageable 연결에 실패했습니다. object=" + enemyObject.name);
        }

        InitializeEnemyComponents(enemyData, enemyStatus, enemyAttack, enemyDetector, enemyMovement, enemyBrain, enemyHud);
    }

    private void InitializeEnemyRuntimeSet(EnemyRuntimeSet enemyRuntimeSet)
    {
        if (enemyRuntimeSet == null)
        {
            return;
        }

        EnemyData enemyData = GetEnemyData(enemyRuntimeSet.EnemyDataId);
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyLoopConnector: EnemyData가 없습니다. id=" + enemyRuntimeSet.EnemyDataId);
            return;
        }

        InitializeEnemyComponents(
            enemyData,
            enemyRuntimeSet.EnemyStatus,
            enemyRuntimeSet.EnemyAttack,
            enemyRuntimeSet.EnemyDetector,
            enemyRuntimeSet.EnemyMovement,
            enemyRuntimeSet.EnemyBrain,
            enemyRuntimeSet.EnemyHud);
    }

    private EnemyData GetEnemyData(string enemyDataId)
    {
        ConnectGameDataManagerIfNeeded();

        if (_gameDataManager == null)
        {
            return null;
        }

        _gameDataManager.Initialize();
        return _gameDataManager.GetEnemy(enemyDataId);
    }

    private void ConnectGameDataManagerIfNeeded()
    {
        if (_gameDataManager != null)
        {
            return;
        }

        if (GameManager.Inst == null)
        {
            return;
        }

        _gameDataManager = GameManager.Inst.GetGameDataManager();
    }

    private void PrepareEnemyPhysics(GameObject enemyObject)
    {
        Rigidbody rigidbody = enemyObject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        NavMeshAgent navMeshAgent = enemyObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            return;
        }

        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;
    }

    private void InitializeEnemyComponents(
        EnemyData enemyData,
        EnemyStatus enemyStatus,
        EnemyAttack enemyAttack,
        EnemyDetector enemyDetector,
        EnemyMovement enemyMovement,
        EnemyBrain enemyBrain,
        EnemyHud enemyHud)
    {
        if (enemyStatus != null)
        {
            enemyStatus.Initialize(enemyData);
        }

        if (enemyAttack != null)
        {
            enemyAttack.Initialize(enemyData);
        }

        if (enemyDetector != null)
        {
            enemyDetector.Initialize(enemyData);
        }

        if (enemyMovement != null)
        {
            enemyMovement.Initialize(enemyData, enemyDetector);
        }

        if (enemyBrain != null)
        {
            enemyBrain.SetCinderHeartTarget(GetCinderHeartDamageable());
        }

        if (enemyHud != null)
        {
            enemyHud.SetTargetCamera(_gameCamera);
        }
    }

    private TComponent GetRuntimeComponent<TComponent>(GameObject targetObject, string componentName)
        where TComponent : Component
    {
        TComponent component = targetObject.GetComponent<TComponent>();
        if (component != null)
        {
            return component;
        }

        return AddFallbackComponent<TComponent>(targetObject, componentName);
    }

    private TComponent AddFallbackComponent<TComponent>(GameObject targetObject, string componentName)
        where TComponent : Component
    {
        // 정식 적 프리팹은 EnemyStatus, EnemyAttack, EnemyDetector, EnemyMovement, EnemyBrain을 미리 가져야 합니다.
        // 이 fallback은 오래된 팀원 브랜치 프리팹을 main에서 흡수할 때만 게임 루프가 멈추지 않게 쓰는 안전장치입니다.
        if (_allowFallbackComponentAdd == false)
        {
            LogMissingComponent(targetObject, componentName);
            return null;
        }

        LogFallbackComponentAdded(targetObject, componentName);
        return targetObject.AddComponent<TComponent>();
    }

    private void LogMissingComponent(GameObject targetObject, string componentName)
    {
        Debug.LogWarning("EnemyLoopConnector: " + targetObject.name + " 프리팹에 " + componentName + " 컴포넌트가 없습니다. 정식 적 프리팹에 미리 추가해주세요.");
    }

    private void LogFallbackComponentAdded(GameObject targetObject, string componentName)
    {
        Debug.LogWarning("EnemyLoopConnector: " + targetObject.name + " 프리팹에 " + componentName + " 컴포넌트가 없어 대체 연결로 추가했습니다. 정식 적 프리팹에는 미리 붙여주세요.");
    }

    private Damageable GetCinderHeartDamageable()
    {
        if (_cinderHeartTarget == null)
        {
            return null;
        }

        Damageable damageable = _cinderHeartTarget.GetComponent<Damageable>();
        if (damageable != null)
        {
            return damageable;
        }

        return _cinderHeartTarget.GetComponentInParent<Damageable>();
    }
}

[Serializable]
public sealed class EnemyRuntimeSet
{
    [Tooltip("이 적 오브젝트에 적용할 EnemyData ID입니다. enemies.json의 _id와 같아야 합니다.")]
    [SerializeField] private string _enemyDataId = "ice_zombie";
    [Tooltip("적 체력 원본 컴포넌트입니다.")]
    [SerializeField] private EnemyStatus _enemyStatus;
    [Tooltip("적 공격 실행 컴포넌트입니다.")]
    [SerializeField] private EnemyAttack _enemyAttack;
    [Tooltip("플레이어 감지 컴포넌트입니다.")]
    [SerializeField] private EnemyDetector _enemyDetector;
    [Tooltip("CinderHeart 또는 플레이어를 향해 이동하는 컴포넌트입니다.")]
    [SerializeField] private EnemyMovement _enemyMovement;
    [Tooltip("공격 대상을 판단하는 컴포넌트입니다.")]
    [SerializeField] private EnemyBrain _enemyBrain;
    [Tooltip("적 머리 위 HP UI 컴포넌트입니다.")]
    [SerializeField] private EnemyHud _enemyHud;

    public string EnemyDataId
    {
        get { return _enemyDataId; }
    }

    public EnemyStatus EnemyStatus
    {
        get { return _enemyStatus; }
    }

    public EnemyAttack EnemyAttack
    {
        get { return _enemyAttack; }
    }

    public EnemyDetector EnemyDetector
    {
        get { return _enemyDetector; }
    }

    public EnemyMovement EnemyMovement
    {
        get { return _enemyMovement; }
    }

    public EnemyBrain EnemyBrain
    {
        get { return _enemyBrain; }
    }

    public EnemyHud EnemyHud
    {
        get { return _enemyHud; }
    }
}
