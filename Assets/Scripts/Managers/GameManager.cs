using UnityEngine;

using UnityEngine.Serialization;

namespace Cinderkeep.Gameplay
{
    // 게임 전체 생성 주기를 잡는 최상위 매니저입니다.
    // 현재 목표는 3일/15분 게임 루프를 안정적으로 시작하고 끝내는 것입니다.
    // 1일차는 채집/기초 제작, 2일차는 CinderHeart 방어, 3일차는 보스 클리어가 기준입니다.
    // 이후 7일 루프 확장은 이 초기화 흐름을 유지한 채 Model과 매니저 기능을 늘립니다.
    // 규칙: 게임 영역에서는 GameManager만 싱글톤으로 둡니다.
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

        private PlayerModel _playerModel = new PlayerModel();
        private GameRunModel _gameRunModel = new GameRunModel();
        private PlayerInventoryModel _playerInventoryModel = new PlayerInventoryModel();
        private PlayerEquipmentModel _playerEquipmentModel = new PlayerEquipmentModel();
        private bool _isInitialized;

        public static GameManager Inst { get; private set; }

        public PlayerModel PlayerModel
        {
            get
            {
                return _playerModel;
            }
        }

        public GameRunModel GameRunModel
        {
            get
            {
                return _gameRunModel;
            }
        }

        public PlayerInventoryModel PlayerInventoryModel
        {
            get
            {
                return _playerInventoryModel;
            }
        }

        public PlayerEquipmentModel PlayerEquipmentModel
        {
            get
            {
                return _playerEquipmentModel;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
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
            if (_isInitialized)
            {
                return;
            }

            // 생성 주기 순서:
            // 1. 3일 게임 루프에 필요한 Static Data 로드
            // 2. 프리팹, 사운드, UI 프리팹 같은 에셋 로딩 관리자 준비
            // 3. 몬스터, 자원, 건축물 생성을 맡을 관리자 준비
            // 4. 맵 생성과 모듈형 맵 확장을 맡을 관리자 준비
            // 5. BGM과 효과음 관리자 준비
            // 6. HUD, 인벤토리, 게임오버 UI 관리자 준비
            // 7. 저장이 필요한 Instance Model 기본값 준비
            // 8. 3일 게임 루프 진행 컨트롤러 준비
            InitializeManager(_gameDataManager, "GameDataManager");
            InitializeManager(_resourceManager, "ResourceManager");
            InitializeManager(_gameObjectManager, "GameObjectManager");
            ConnectBuildingManager();
            InitializeBuildingManagerIfExists();
            InitializeManager(_mapManager, "MapManager");
            InitializeManager(_soundManager, "SoundManager");
            InitializeManager(_uiManager, "UIManager");

            _playerModel.InitializeDefault();
            _gameRunModel.InitializeDefault();
            _playerInventoryModel.InitializeDefault();
            _playerEquipmentModel.InitializeDefault();
            ConnectGameFlowController();
            InitializeGameFlowControllerIfExists();
            _isInitialized = true;
        }

        public void StartNewGame()
        {
            Initialize();
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
                _gameFlowController.StopFlowAsGameOver();
            }

            if (_uiManager != null)
            {
                _uiManager.OpenGameOverPanel();
            }
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
