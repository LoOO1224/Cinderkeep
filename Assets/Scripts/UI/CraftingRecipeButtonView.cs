using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public void SetRecipe(CraftingRecipeData recipeData, bool canCraft, CraftingUI ownerCraftingUI)
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
            RefreshState(canCraft);
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

        private void RefreshState(bool canCraft)
        {
            if (canCraft)
            {
                RefreshText(_stateText, "제작 가능");
            }
            else
            {
                RefreshText(_stateText, "자원 부족");
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

                costText += costData.ResourceId + " " + costData.Amount;
            }

            return costText;
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
