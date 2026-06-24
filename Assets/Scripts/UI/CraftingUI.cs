using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 5.00 direction: Displays or controls UI for the 5.00 playable loop without owning gameplay rules.
// 5.01+ note: Keep UI as a view/controller layer; read models and dispatch requests instead of duplicating game logic.
namespace Cinderkeep.Gameplay
{
    // 제작대 UI를 갱신하는 컴포넌트입니다.
    // UI 오브젝트는 씬이나 프리팹에 미리 만들어두고, 이 클래스는 표시와 제작 요청만 담당합니다.
    public sealed class CraftingUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _rootObject;

        [Header("Text UI")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;

        [Header("Recipe Slots")]
        [SerializeField] private CraftingRecipeButtonView[] _recipeSlots;

        [Header("Connected Components")]
        [SerializeField] private CraftingRecipeExecutor _recipeExecutor;
        [SerializeField] private InventoryUI _inventoryUI;

        private readonly List<CraftingRecipeData> _availableRecipes = new List<CraftingRecipeData>();
        private CraftingStation _currentStation;
        private GameDataManager _gameDataManager;
        private PlayerModel _playerModel;

        private bool _isOpen;

        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
        }

        private void Start()
        {
            Close();
        }

        public void OpenStation(CraftingStation craftingStation, GameObject interactor)
        {
            if (craftingStation == null)
            {
                return;
            }

            _currentStation = craftingStation;

            ConnectGameState();
            ConnectRecipeExecutor();
            SetVisible(true);
            RefreshUI();

            if (_inventoryUI != null)
            {
                _inventoryUI.OpenEmbedded();
            }
        }

        public void Close()
        {
            ClearRecipeSlots();
            SetVisible(false);

            if (_inventoryUI != null)
            {
                _inventoryUI.Close();
            }
        }

        public void TryCraftRecipe(string recipeId)
        {
            ConnectGameState();
            ConnectRecipeExecutor();

            if (_gameDataManager == null || _playerModel == null || _recipeExecutor == null)
            {
                PlayCraftFailSfx();
                RefreshMessage("제작 연결이 아직 준비되지 않았습니다.");
                return;
            }

            CraftingRecipeData recipeData = _gameDataManager.GetCraftingRecipe(recipeId);
            if (recipeData == null)
            {
                PlayCraftFailSfx();
                RefreshMessage("제작법을 찾을 수 없습니다.");
                return;
            }

            if (_recipeExecutor.TryCraft(recipeData, _playerModel))
            {
                PlayCraftSuccessSfx();
                RefreshMessage(recipeData.DisplayName + " 제작 완료");
                RefreshUI();
                if (_inventoryUI != null)
                {
                    _inventoryUI.RefreshUI();
                }
                return;
            }

            PlayCraftFailSfx();
            RefreshMessage("자원이 부족하거나 아직 제작할 수 없습니다.");
            RefreshUI();
        }

        private void RefreshUI()
        {
            RefreshTitle();
            RefreshRecipes();
        }

        private void RefreshTitle()
        {
            if (_titleText == null)
            {
                return;
            }

            if (_currentStation == null)
            {
                _titleText.text = "제작";
                return;
            }

            CraftingStationData stationData = _currentStation.GetStationData();
            if (stationData != null && string.IsNullOrEmpty(stationData.DisplayName) == false)
            {
                _titleText.text = stationData.DisplayName;
                return;
            }

            _titleText.text = _currentStation.StationType;
        }

        private void RefreshRecipes()
        {
            ClearRecipeSlots();
            ConnectRecipeExecutor();
            if (_currentStation == null || _gameDataManager == null)
            {
                return;
            }

            if (_recipeSlots == null)
            {
                return;
            }

            int currentDay = GetCurrentDay();
            _currentStation.GetAvailableRecipes(_gameDataManager, currentDay, _availableRecipes);
            if (_recipeExecutor != null)
            {
                _availableRecipes.RemoveAll(recipeData => _recipeExecutor.IsRecipeVisibleInCraftingUI(recipeData) == false);
            }

            for (int i = 0; i < _availableRecipes.Count; i++)
            {
                if (i >= _recipeSlots.Length)
                {
                    RefreshMessage("표시 가능한 제작 슬롯이 부족합니다.");
                    return;
                }

                if (_recipeSlots[i] == null)
                {
                    continue;
                }

                CraftingRecipeData recipeData = _availableRecipes[i];
                bool canCraft = CanCraftRecipe(recipeData);
                _recipeSlots[i].SetRecipe(recipeData, canCraft, this);
            }
        }

        private bool CanCraftRecipe(CraftingRecipeData recipeData)
        {
            if (_recipeExecutor == null || _playerModel == null)
            {
                return false;
            }
            return _recipeExecutor.CanCraft(recipeData, _playerModel);
        }

        private int GetCurrentDay()
        {
            if (GameManager.Inst == null)
            {
                return GameRunModel.FirstDay;
            }

            return GameManager.Inst.GameRunModel.Day;
        }

        private void ConnectGameState()
        {
            if (GameManager.Inst == null)
            {
                return;
            }

            _gameDataManager = GameManager.Inst.GetGameDataManager();
            _playerModel = GameManager.Inst.PlayerModel;
        }

        private void ConnectRecipeExecutor()
        {
            if (_recipeExecutor != null)
            {
                return;
            }

            if (_currentStation == null)
            {
                return;
            }

            _recipeExecutor = _currentStation.GetComponent<CraftingRecipeExecutor>();
        }

        private void ClearRecipeSlots()
        {
            if (_recipeSlots == null)
            {
                return;
            }

            for (int i = 0; i < _recipeSlots.Length; i++)
            {
                if (_recipeSlots[i] == null)
                {
                    continue;
                }

                _recipeSlots[i].Clear();
            }
        }

        private void RefreshMessage(string message)
        {
            if (_messageText == null)
            {
                return;
            }

            _messageText.text = message;
        }

        private void SetVisible(bool isVisible)
        {
            _isOpen = isVisible;

            if (_rootObject == null)
            {
                gameObject.SetActive(isVisible);
                return;
            }

            _rootObject.SetActive(isVisible);
        }

        private void PlayCraftSuccessSfx()
        {
            SoundManager soundManager = GetSoundManager();
            if (soundManager == null)
            {
                return;
            }

            soundManager.PlayCraftSuccess();
        }

        private void PlayCraftFailSfx()
        {
            SoundManager soundManager = GetSoundManager();
            if (soundManager == null)
            {
                return;
            }

            soundManager.PlayCraftFail();
        }

        private SoundManager GetSoundManager()
        {
            if (GameManager.Inst == null)
            {
                return null;
            }

            return GameManager.Inst.GetSoundManager();
        }

        // 제작대 상호작용 키를 다시 눌렀을 때 같은 제작 UI를 닫기 위해 사용합니다.
        public void Toggle(CraftingStation craftingStation, GameObject interactor)
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            OpenStation(craftingStation, interactor);
        }
    }
}
