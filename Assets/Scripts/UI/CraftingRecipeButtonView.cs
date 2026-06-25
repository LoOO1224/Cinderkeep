using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이 상태를 화면에 표시하거나 사용자의 UI 요청을 전달합니다.
// UI는 규칙을 소유하지 않고 모델을 읽고 시스템에 요청을 보내는 계층으로 유지합니다.
namespace Cinderkeep.Gameplay
{
    // 제작 목록의 한 줄을 표시하는 UI 컴포넌트입니다.
    // 실제 제작 처리는 CraftingUI가 담당하고, 이 클래스는 버튼 표시와 클릭 전달만 맡습니다.
    public sealed class CraftingRecipeButtonView : MonoBehaviour
    {
        [Header("Text UI")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _stateText;

        [Header("Button UI")]
        [SerializeField] private Button _craftButton;

        private CraftingUI _ownerCraftingUI;
        private string _recipeId;

        private void OnEnable()
        {
            ConnectButton();
        }

        private void OnDisable()
        {
            DisconnectButton();
        }

        public void SetRecipe(CraftingRecipeData recipeData, bool canCraft, string stateText, CraftingUI ownerCraftingUI)
        {
            if (recipeData == null)
            {
                Clear();
                return;
            }

            _ownerCraftingUI = ownerCraftingUI;
            _recipeId = recipeData.Id;

            SetVisible(true);
            RefreshName(recipeData);
            RefreshCost(recipeData);
            RefreshState(canCraft, stateText);
            RefreshButton(canCraft);
        }

        public void Clear()
        {
            _ownerCraftingUI = null;
            _recipeId = string.Empty;

            RefreshText(_nameText, string.Empty);
            RefreshText(_costText, string.Empty);
            RefreshText(_stateText, string.Empty);
            RefreshButton(false);
            SetVisible(false);
        }

        private void HandleCraftButtonClicked()
        {
            if (_ownerCraftingUI == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_recipeId))
            {
                return;
            }

            _ownerCraftingUI.TryCraftRecipe(_recipeId);
        }

        private void RefreshName(CraftingRecipeData recipeData)
        {
            if (string.IsNullOrEmpty(recipeData.DisplayName))
            {
                RefreshText(_nameText, recipeData.Id);
                return;
            }

            RefreshText(_nameText, recipeData.DisplayName);
        }

        private void RefreshCost(CraftingRecipeData recipeData)
        {
            if (_costText == null)
            {
                return;
            }

            _costText.text = BuildCostText(recipeData);
        }

        private void RefreshState(bool canCraft, string stateText)
        {
            string safeStateText = string.IsNullOrEmpty(stateText)
                ? (canCraft ? "제작 가능" : "제작 불가")
                : stateText;

            if (canCraft)
            {
                RefreshText(_stateText, safeStateText);
                RefreshStateColor(new Color(0.35f, 1f, 0.55f, 1f));
            }
            else
            {
                RefreshText(_stateText, safeStateText);
                RefreshStateColor(new Color(1f, 0.45f, 0.32f, 1f));
            }
        }

        private void RefreshButton(bool canCraft)
        {
            if (_craftButton == null)
            {
                return;
            }

            _craftButton.interactable = canCraft;
        }

        private string BuildCostText(CraftingRecipeData recipeData)
        {
            if (recipeData.Costs == null || recipeData.Costs.Count <= 0)
            {
                return "비용 없음";
            }

            string costText = "";
            for (int i = 0; i < recipeData.Costs.Count; i++)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(costText) == false)
                {
                    costText += " / ";
                }

                costText += UiItemDisplayFormatter.GetItemName(costData.ResourceId, InventoryItemType.Resource)
                    + " "
                    + costData.Amount;
            }

            return costText;
        }

        private void RefreshStateColor(Color color)
        {
            if (_stateText == null)
            {
                return;
            }

            _stateText.color = color;
        }

        private void RefreshText(TMP_Text targetText, string text)
        {
            if (targetText == null)
            {
                return;
            }

            targetText.text = text;
        }

        private void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        private void ConnectButton()
        {
            if (_craftButton == null)
            {
                return;
            }

            _craftButton.onClick.RemoveListener(HandleCraftButtonClicked);
            _craftButton.onClick.AddListener(HandleCraftButtonClicked);
        }

        private void DisconnectButton()
        {
            if (_craftButton == null)
            {
                return;
            }

            _craftButton.onClick.RemoveListener(HandleCraftButtonClicked);
        }
    }
}
