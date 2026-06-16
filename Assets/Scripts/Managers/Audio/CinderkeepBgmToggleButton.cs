using UnityEngine;
using UnityEngine.UI;

namespace OODong.Cinderkeep
{
    // Main_Lobby에서 BGM ON/OFF를 담당하는 얇은 UI 컴포넌트입니다.
    // 실제 재생 정책은 CinderkeepBgmController가 가지고, 버튼은 요청만 전달합니다.
    public sealed class CinderkeepBgmToggleButton : MonoBehaviour
    {
        [SerializeField] private Button Button_Toggle;
        [SerializeField] private Text Text_Label;
        [SerializeField] private CinderkeepBgmController CinderkeepBgmController_BgmController;

        private void Awake()
        {
            ResolveReferences();
            if (Button_Toggle != null)
            {
                Button_Toggle.onClick.AddListener(ToggleBgm);
            }
        }

        private void Start()
        {
            RefreshLabel();
        }

        private void OnDestroy()
        {
            if (Button_Toggle != null)
            {
                Button_Toggle.onClick.RemoveListener(ToggleBgm);
            }
        }

        public void SetReferences(Button toggleButton, Text label, CinderkeepBgmController bgmController)
        {
            Button_Toggle = toggleButton;
            Text_Label = label;
            CinderkeepBgmController_BgmController = bgmController;
            RefreshLabel();
        }

        public void ToggleBgm()
        {
            ResolveReferences();
            CinderkeepBgmController_BgmController?.ToggleBgm();
            RefreshLabel();
        }

        public void RefreshLabel()
        {
            ResolveReferences();
            if (Text_Label == null)
            {
                return;
            }

            bool isEnabled = CinderkeepBgmController_BgmController == null || CinderkeepBgmController_BgmController.IsBgmEnabled;
            Text_Label.text = isEnabled ? "BGM ON" : "BGM OFF";
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

            if (CinderkeepBgmController_BgmController == null)
            {
                CinderkeepBgmController_BgmController = CinderkeepBgmController.Instance != null
                    ? CinderkeepBgmController.Instance
                    : FindFirstObjectByType<CinderkeepBgmController>();
            }
        }
    }
}
