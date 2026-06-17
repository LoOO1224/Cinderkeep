using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainHub.Audio
{
    // MainHub BGM ON/OFF 버튼을 관리합니다.
    // 별도 Canvas에 올려서 씬이 바뀌어도 작업 중 계속 사용할 수 있게 둡니다.
    public sealed class MainHub_BgmToggleButton : MonoBehaviour
    {
        private const string PersistentCanvasName = "MainHub_BgmToggleCanvas";

        [SerializeField] private Button Button_Toggle;
        [SerializeField] private Text Text_Label;
        [SerializeField] private MainHub_BgmController MainHubBgmController_BgmController;
        [SerializeField] private bool _keepAcrossScenes = true;

        private static MainHub_BgmToggleButton _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                DestroyDuplicateRoot();
                return;
            }

            _instance = this;
            PreserveToggleCanvas();
            RegisterButtonEvent();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            RefreshLabel();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnregisterButtonEvent();

            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void SetReferences(Button toggleButton, Text label, MainHub_BgmController bgmController)
        {
            Button_Toggle = toggleButton;
            Text_Label = label;
            MainHubBgmController_BgmController = bgmController;
            RefreshLabel();
        }

        public void SetKeepAcrossScenes(bool keepAcrossScenes)
        {
            _keepAcrossScenes = keepAcrossScenes;
        }

        public void ToggleBgm()
        {
            ResolveReferences();
            MainHubBgmController_BgmController?.ToggleBgm();
            RefreshLabel();
        }

        public void RefreshLabel()
        {
            ResolveReferences();
            if (Text_Label == null)
            {
                return;
            }

            bool isEnabled = MainHubBgmController_BgmController == null || MainHubBgmController_BgmController.IsBgmEnabled;
            Text_Label.text = isEnabled ? "\u266a BGM ON" : "\u266a BGM OFF";
        }

        private void RegisterButtonEvent()
        {
            ResolveReferences();
            if (Button_Toggle != null)
            {
                Button_Toggle.onClick.AddListener(ToggleBgm);
            }
        }

        private void UnregisterButtonEvent()
        {
            if (Button_Toggle != null)
            {
                Button_Toggle.onClick.RemoveListener(ToggleBgm);
            }
        }

        private void ResolveReferences()
        {
            if (Button_Toggle == null)
            {
                Button_Toggle = GetComponent<Button>();
            }

            if (Text_Label == null)
            {
                Text_Label = GetComponentInChildren<Text>(true);
            }

            if (MainHubBgmController_BgmController == null)
            {
                MainHubBgmController_BgmController = MainHub_BgmController.Instance;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshLabel();
        }

        private void PreserveToggleCanvas()
        {
            if (!_keepAcrossScenes)
            {
                return;
            }

            Transform root = GetPersistentRoot();
            root.SetParent(null);
            DontDestroyOnLoad(root.gameObject);
        }

        private void DestroyDuplicateRoot()
        {
            Transform root = GetPersistentRoot();
            Destroy(root.gameObject);
        }

        private Transform GetPersistentRoot()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.gameObject.name == PersistentCanvasName)
            {
                return canvas.transform;
            }

            return transform;
        }
    }
}
