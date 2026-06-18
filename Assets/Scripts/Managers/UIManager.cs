using UnityEngine;

using UnityEngine.Serialization;

namespace Cinderkeep.Gameplay
{
    // 게임 씬 전용 UI 매니저입니다.
    // 현재는 3일 게임 루프에 필요한 HUD, 인벤토리, 게임오버 UI부터 관리합니다.
    // UI는 코드에서 새로 만들지 않고, 씬이나 프리팹에 준비된 오브젝트를 켜고 끄는 방식으로 관리합니다.
    public sealed class UIManager : MonoBehaviour, IGameInitializable
    {
        [FormerlySerializedAs("_hudRoot")]
        [SerializeField] private GameObject GameObject_HudRoot;
        [FormerlySerializedAs("_inventoryRoot")]
        [SerializeField] private GameObject GameObject_InventoryRoot;
        [FormerlySerializedAs("_gameOverPanel")]
        [SerializeField] private GameObject GameObject_GameOverPanel;

        private bool _isInitialized;

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            CloseHud();
            CloseInventory();
            CloseGameOverPanel();
            _isInitialized = true;
        }

        public void OpenHud()
        {
            SetActive(GameObject_HudRoot, true);
        }

        public void CloseHud()
        {
            SetActive(GameObject_HudRoot, false);
        }

        public void OpenInventory()
        {
            SetActive(GameObject_InventoryRoot, true);
        }

        public void CloseInventory()
        {
            SetActive(GameObject_InventoryRoot, false);
        }

        public void OpenGameOverPanel()
        {
            SetActive(GameObject_GameOverPanel, true);
        }

        public void CloseGameOverPanel()
        {
            SetActive(GameObject_GameOverPanel, false);
        }

        private void SetActive(GameObject targetObject, bool isActive)
        {
            if (targetObject == null)
            {
                return;
            }

            targetObject.SetActive(isActive);
        }
    }
}
