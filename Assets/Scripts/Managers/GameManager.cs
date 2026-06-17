using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 게임 전체 생성 주기를 잡는 최상위 매니저입니다.
    // 현재 목표는 3일/15분 MVP 루프를 안정적으로 시작하고 끝내는 것입니다.
    // 1일차는 채집/기초 제작, 2일차는 FlameHeart 방어, 3일차는 보스 클리어가 기준입니다.
    // 이후 7일 루프 확장은 이 초기화 흐름을 유지한 채 Model과 매니저 기능을 늘립니다.
    // 규칙: 게임 영역에서는 GameManager만 싱글톤으로 둡니다.
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameDataManager GameDataManager_GameDataManager;
        [SerializeField] private GameObjectManager GameObjectManager_GameObjectManager;
        [SerializeField] private UIManager UIManager_UIManager;
        [SerializeField] private SoundManager SoundManager_SoundManager;

        private PlayerModel _playerModel = new PlayerModel();
        private GameRunModel _gameRunModel = new GameRunModel();
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
            // 1. 3일 MVP 루프에 필요한 Static Data 로드
            // 2. 몬스터, 자원, 건축물 생성을 맡을 관리자 준비
            // 3. BGM과 효과음 관리자 준비
            // 4. HUD, 인벤토리, 게임오버 UI 관리자 준비
            // 5. 저장이 필요한 Instance Model 기본값 준비
            InitializeManager(GameDataManager_GameDataManager, "GameDataManager");
            InitializeManager(GameObjectManager_GameObjectManager, "GameObjectManager");
            InitializeManager(SoundManager_SoundManager, "SoundManager");
            InitializeManager(UIManager_UIManager, "UIManager");

            _playerModel.InitializeDefault();
            _gameRunModel.InitializeDefault();
            _isInitialized = true;
        }

        public void StartNewGame()
        {
            Initialize();
            _gameRunModel.StartRun();

            if (UIManager_UIManager != null)
            {
                UIManager_UIManager.OpenHud();
            }
        }

        public void EndGame()
        {
            if (!_gameRunModel.IsPlaying)
            {
                return;
            }

            _gameRunModel.EndRun();

            if (UIManager_UIManager != null)
            {
                UIManager_UIManager.OpenGameOverPanel();
            }
        }

        public GameDataManager GetGameDataManager()
        {
            return GameDataManager_GameDataManager;
        }

        public GameObjectManager GetGameObjectManager()
        {
            return GameObjectManager_GameObjectManager;
        }

        public UIManager GetUIManager()
        {
            return UIManager_UIManager;
        }

        public SoundManager GetSoundManager()
        {
            return SoundManager_SoundManager;
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
