using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 5.00 direction: Coordinates a focused slice of the 5.00 game loop from scene and runtime references.
// 5.01+ note: Keep this manager as a thin hub; move calculations and feature rules into smaller systems/helpers.
namespace Cinderkeep.Gameplay
{
    // 게임 전체 생성 주기를 잡는 최상위 매니저입니다.
    // 규칙: 게임 영역에서는 GameManager만 싱글톤으로 둡니다.
    // 5.00 기준으로 이 클래스는 Composition Root 역할만 맡고, 실제 계산은 각 전용 컴포넌트가 담당합니다.
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameDataManager _gameDataManager;
        [SerializeField] private ResourceManager _resourceManager;
        [SerializeField] private GameObjectManager _gameObjectManager;
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private SoundManager _soundManager;
        [SerializeField] private MapManager _mapManager;
        [SerializeField] private GameFlowController _gameFlowController;
        [SerializeField] private string _mainMenuSceneName = "Main_Lobby";

        private PlayerModel _playerModel = new PlayerModel();
        private GameRunModel _gameRunModel = new GameRunModel();
        private PlayerInventoryModel _playerInventoryModel = new PlayerInventoryModel();
        private PlayerEquipmentModel _playerEquipmentModel = new PlayerEquipmentModel();
        private bool _isInitialized;
        private bool _isInitializing;
        private bool _isStartRequested;

        public static GameManager Inst { get; private set; }

        public PlayerModel PlayerModel
        {
            get { return _playerModel; }
        }

        public GameRunModel GameRunModel
        {
            get { return _gameRunModel; }
        }

        public PlayerInventoryModel PlayerInventoryModel
        {
            get { return _playerInventoryModel; }
        }

        public PlayerEquipmentModel PlayerEquipmentModel
        {
            get { return _playerEquipmentModel; }
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public bool IsInitializing
        {
            get { return _isInitializing; }
        }

        private void Awake()
        {
            RegisterSingleton();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (Inst == this)
            {
                Inst = null;
            }
        }

        public void Initialize()
        {
            if (_isInitialized || _isInitializing)
            {
                return;
            }

            StartCoroutine(InitializeRoutine());
        }

        public void StartNewGame()
        {
            if (QueueStartUntilInitialized())
            {
                return;
            }

            BeginNewGameFlow();
        }

        public void RestartRun()
        {
            if (QueueStartUntilInitialized())
            {
                return;
            }

            ResetRunState();
            BeginNewGameFlow();
        }

        public void EndGame()
        {
            if (!_gameRunModel.IsPlaying)
            {
                return;
            }

            _gameRunModel.EndRun();
            if (_gameFlowController != null)
            {
                _gameFlowController.StopFlow();
            }

            if (_uiManager != null)
            {
                _uiManager.OpenGameOverPanel();
            }
        }

        public void ReturnToMainLobby()
        {
            Time.timeScale = 1f;

            if (string.IsNullOrEmpty(_mainMenuSceneName))
            {
                Debug.LogWarning("GameManager: main menu scene name is empty.");
                return;
            }

            SceneManager.LoadScene(_mainMenuSceneName);
        }

        public GameDataManager GetGameDataManager()
        {
            return _gameDataManager;
        }

        public ResourceManager GetResourceManager()
        {
            return _resourceManager;
        }

        public GameObjectManager GetGameObjectManager()
        {
            return _gameObjectManager;
        }

        public BuildingManager GetBuildingManager()
        {
            return _buildingManager;
        }

        public UIManager GetUIManager()
        {
            return _uiManager;
        }

        public SoundManager GetSoundManager()
        {
            return _soundManager;
        }

        public MapManager GetMapManager()
        {
            return _mapManager;
        }

        public GameFlowController GetGameFlowController()
        {
            return _gameFlowController;
        }

        private IEnumerator InitializeRoutine()
        {
            _isInitializing = true;

            // Data -> Model -> Resource/Object/Map/Sound -> UI -> GameFlow 순서를 고정합니다.
            InitializeManager(_gameDataManager, "GameDataManager");
            InitializeRuntimeModels();
            yield return null;

            InitializeManager(_resourceManager, "ResourceManager");
            InitializeManager(_gameObjectManager, "GameObjectManager");
            ConnectBuildingManager();
            InitializeBuildingManagerIfExists();
            yield return null;

            InitializeManager(_mapManager, "MapManager");
            InitializeManager(_soundManager, "SoundManager");
            InitializeManager(_uiManager, "UIManager");
            yield return null;

            ConnectGameFlowController();
            InitializeGameFlowControllerIfExists();

            _isInitialized = true;
            _isInitializing = false;

            if (_isStartRequested)
            {
                _isStartRequested = false;
                BeginNewGameFlow();
            }
        }

        private bool QueueStartUntilInitialized()
        {
            if (_isInitialized)
            {
                return false;
            }

            _isStartRequested = true;
            Initialize();
            return true;
        }

        private void BeginNewGameFlow()
        {
            Time.timeScale = 1f;
            ResetPlayerSceneState();
            RunResultTracker.EnsureSceneTracker().BeginRun(_gameRunModel);
            global::PlayerEquipmentStatApplier.EnsureSceneApplier();

            if (_gameFlowController != null)
            {
                _gameFlowController.StartFlow();
            }
            else
            {
                _gameRunModel.StartRun();
            }

            if (_uiManager != null)
            {
                _uiManager.OpenHud();
            }

            global::HandStonePickupSceneBootstrap.EnsureHandStonePickup();
            global::FoodPickupSceneBootstrap.EnsureFoodPickups();
            global::TrapZoneSceneBootstrap.EnsureTrapZones();
            global::CinderHeartPlayerRecoveryAura.EnsureSceneAura();
            global::CinderHeartFoodCooker.EnsureSceneCooker();
        }

        private void ResetRunState()
        {
            Time.timeScale = 1f;

            if (_gameFlowController != null)
            {
                _gameFlowController.StopFlow();
            }

            ResetRuntimeSceneObjects();
            InitializeRuntimeModels();
            ResetCinderHeartState();
            ResetPlayerSceneState();
            global::PlayerEquipmentStatApplier.EnsureSceneApplier();
            global::HandStonePickupSceneBootstrap.EnsureHandStonePickup();
            global::FoodPickupSceneBootstrap.EnsureFoodPickups();
            global::TrapZoneSceneBootstrap.EnsureTrapZones();
            global::CinderHeartPlayerRecoveryAura.EnsureSceneAura();
            global::CinderHeartFoodCooker.EnsureSceneCooker();

            if (_uiManager != null)
            {
                _uiManager.CloseGameOverPanel();
                _uiManager.CloseInventory();
                _uiManager.CloseCraftingUI();
                _uiManager.CloseFurnaceUI();
                _uiManager.CloseCinderHeartSkillSelectionUI();
            }
        }

        private void InitializeRuntimeModels()
        {
            _playerModel.InitializeDefault();
            _gameRunModel.InitializeDefault();
            _playerInventoryModel.InitializeDefault();
            _playerEquipmentModel.InitializeDefault();
        }

        private void ResetCinderHeartState()
        {
            GameObject cinderHeartObject = FindCinderHeartObject();
            if (cinderHeartObject == null)
            {
                return;
            }

            CinderHeart cinderHeart = cinderHeartObject.GetComponent<CinderHeart>();
            if (cinderHeart == null)
            {
                return;
            }

            cinderHeart.ResetForNewRun();
        }

        private void ResetPlayerSceneState()
        {
            global::PlayerStatus playerStatus = Object.FindFirstObjectByType<global::PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.ResetStatusForNewRun();
            }

            global::PlayerAttack playerAttack = Object.FindFirstObjectByType<global::PlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.ResetRunBonuses();
            }
        }

        private void ResetRuntimeSceneObjects()
        {
            ResetFurnaceStations();

            if (_buildingManager != null)
            {
                _buildingManager.ResetRuntimeBuildings();
            }

            if (_gameObjectManager != null)
            {
                _gameObjectManager.DestroyAllRegisteredGameObjects();
            }
        }

        private void ResetFurnaceStations()
        {
            FurnaceStation[] furnaceStations = Object.FindObjectsByType<FurnaceStation>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < furnaceStations.Length; i++)
            {
                if (furnaceStations[i] == null)
                {
                    continue;
                }

                furnaceStations[i].ResetFurnaceState();
            }
        }

        private GameObject FindCinderHeartObject()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("CinderHeart");
            }
            catch (UnityException)
            {
                return GameObject.Find("CinderHeart");
            }
        }

        private void RegisterSingleton()
        {
            if (Inst == null)
            {
                Inst = this;
                return;
            }

            if (Inst == this)
            {
                return;
            }

            Debug.LogWarning("GameManager duplicate was found. Only the first GameManager will stay active.");
            Destroy(gameObject);
        }

        private void ConnectBuildingManager()
        {
            if (_buildingManager == null)
            {
                return;
            }

            _buildingManager.SetGameObjectManager(_gameObjectManager);
        }

        private void InitializeBuildingManagerIfExists()
        {
            if (_buildingManager == null)
            {
                return;
            }

            _buildingManager.Initialize();
        }

        private void ConnectGameFlowController()
        {
            if (_gameFlowController == null)
            {
                return;
            }

            _gameFlowController.SetGameManager(this);
        }

        private void InitializeGameFlowControllerIfExists()
        {
            if (_gameFlowController == null)
            {
                return;
            }

            _gameFlowController.Initialize();
        }

        private void InitializeManager<TManager>(TManager manager, string managerName)
            where TManager : MonoBehaviour, IGameInitializable
        {
            if (manager == null)
            {
                Debug.LogWarning(managerName + " reference is empty. Assign it in the Inspector before gameplay work.");
                return;
            }

            manager.Initialize();
        }
    }
}
