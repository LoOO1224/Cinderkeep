using UnityEngine;

using UnityEngine.Serialization;

namespace Cinderkeep.Gameplay
{
    // 게임 씬 전용 UI 매니저입니다.
    // 현재는 3일 게임 루프에 필요한 HUD, 인벤토리, 게임오버 UI부터 관리합니다.
    // UI는 코드에서 새로 만들지 않고, 씬이나 프리팹에 준비된 오브젝트를 켜고 끄는 방식으로 관리합니다.
    public sealed class UIManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private GameObject _hudRoot;
        [SerializeField] private GameObject _inventoryRoot;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private CraftingUI _craftingUI;
        [SerializeField] private FurnaceUI _furnaceUI;

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
            CloseCraftingUI();
            CloseFurnaceUI();
            _isInitialized = true;
        }

        public void OpenHud()
        {
            SetActive(_hudRoot, true);
        }

        public void CloseHud()
        {
            SetActive(_hudRoot, false);
        }

        public void OpenInventory()
        {
            SetActive(_inventoryRoot, true);
        }

        public void CloseInventory()
        {
            SetActive(_inventoryRoot, false);
        }

        public void OpenGameOverPanel()
        {
            SetActive(_gameOverPanel, true);
        }

        public void CloseGameOverPanel()
        {
            SetActive(_gameOverPanel, false);
        }

        public void OpenCraftingUI(CraftingStation craftingStation, GameObject interactor)
        {
            if (_craftingUI == null)
            {
                return;
            }

            CloseFurnaceUI();
            _craftingUI.OpenStation(craftingStation, interactor);
        }

        public void CloseCraftingUI()
        {
            if (_craftingUI == null)
            {
                return;
            }

            _craftingUI.Close();
        }

        public void OpenFurnaceUI(FurnaceStation furnaceStation, GameObject interactor)
        {
            if (_furnaceUI == null)
            {
                return;
            }

            CloseCraftingUI();
            _furnaceUI.OpenFurnace(furnaceStation);
        }

        public void CloseFurnaceUI()
        {
            if (_furnaceUI == null)
            {
                return;
            }

            _furnaceUI.Close();
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
