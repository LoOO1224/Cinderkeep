using System;
using UnityEngine;

// 제작 비용 차감과 결과 지급을 담당합니다.
// CinderHeart 보상은 아침 3택 보상 시스템에서만 처리하고, 제작 결과로는 지급하지 않습니다.
namespace Cinderkeep.Gameplay
{
    // Crafting UI와 제작대가 이 컴포넌트를 통해 제작 가능 여부와 실제 제작을 요청합니다.
    public sealed class CraftingRecipeExecutor : MonoBehaviour
    {
        public static event Action<CraftingRecipeData> RecipeCraftedGlobal;

        public bool CanCraft(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            if (recipeData == null || playerModel == null)
            {
                return false;
            }

            if (CanGrantRecipeResult(recipeData) == false)
            {
                return false;
            }

            return CanPayRecipeCost(recipeData, playerModel);
        }

        public string GetCraftStateText(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            if (recipeData == null)
            {
                return "제작법 없음";
            }

            if (playerModel == null)
            {
                return "플레이어 정보 없음";
            }

            if (CanGrantRecipeResult(recipeData) == false)
            {
                return GetResultBlockText(recipeData);
            }

            string missingResourceId;
            int currentAmount;
            int requiredAmount;
            if (TryGetFirstMissingCost(recipeData, playerModel, out missingResourceId, out currentAmount, out requiredAmount))
            {
                string resourceName = UiItemDisplayFormatter.GetItemName(missingResourceId, InventoryItemType.Resource);
                return resourceName + " 부족 " + currentAmount + "/" + requiredAmount;
            }

            return "제작 가능";
        }

        public bool TryCraft(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            if (CanCraft(recipeData, playerModel) == false)
            {
                return false;
            }

            if (TryPayRecipeCost(recipeData, playerModel) == false)
            {
                return false;
            }

            bool isGranted = TryGrantRecipeResult(recipeData, playerModel);
            if (isGranted)
            {
                NotifyRecipeCrafted(recipeData);
                return true;
            }

            RefundRecipeCost(recipeData, playerModel);
            return false;
        }

        private bool CanPayRecipeCost(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            for (int i = 0; i < recipeData.Costs.Count; i++)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null)
                {
                    continue;
                }

                if (playerModel.HasResource(costData.ResourceId, costData.Amount) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryGetFirstMissingCost(
            CraftingRecipeData recipeData,
            PlayerModel playerModel,
            out string resourceId,
            out int currentAmount,
            out int requiredAmount)
        {
            resourceId = string.Empty;
            currentAmount = 0;
            requiredAmount = 0;

            if (recipeData == null || playerModel == null)
            {
                return false;
            }

            for (int i = 0; i < recipeData.Costs.Count; i++)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null || costData.Amount <= 0)
                {
                    continue;
                }

                int playerAmount = playerModel.GetResourceAmount(costData.ResourceId);
                if (playerAmount >= costData.Amount)
                {
                    continue;
                }

                resourceId = costData.ResourceId;
                currentAmount = playerAmount;
                requiredAmount = costData.Amount;
                return true;
            }

            return false;
        }

        private bool TryPayRecipeCost(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            for (int i = 0; i < recipeData.Costs.Count; i++)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null)
                {
                    continue;
                }

                if (playerModel.UseResource(costData.ResourceId, costData.Amount) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryGrantRecipeResult(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            if (IsResourceResult(recipeData))
            {
                playerModel.AddResource(recipeData.ResultItemId, recipeData.ResultCount);
                return true;
            }

            if (IsBuildingResult(recipeData))
            {
                return TryGrantPreparedBuilding(recipeData);
            }

            InventoryItemType itemType;
            if (TryConvertToItemType(recipeData.ResultDataType, recipeData.ResultItemId, out itemType) == false)
            {
                return false;
            }

            if (GameManager.Inst == null)
            {
                return false;
            }

            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
            if (inventoryModel == null)
            {
                return false;
            }

            return inventoryModel.TryAddItem(recipeData.ResultItemId, itemType, recipeData.ResultCount);
        }

        private bool CanGrantRecipeResult(CraftingRecipeData recipeData)
        {
            if (IsResourceResult(recipeData))
            {
                return true;
            }

            if (IsBuildingResult(recipeData))
            {
                return CanGrantPreparedBuilding();
            }

            if (IsCinderHeartUpgradeResult(recipeData))
            {
                return false;
            }

            InventoryItemType itemType;
            if (TryConvertToItemType(recipeData.ResultDataType, recipeData.ResultItemId, out itemType) == false)
            {
                return false;
            }

            return CanAddInventoryItem(recipeData.ResultItemId, itemType, recipeData.ResultCount);
        }

        private string GetResultBlockText(CraftingRecipeData recipeData)
        {
            if (IsCinderHeartUpgradeResult(recipeData))
            {
                return "아침 보상 전용";
            }

            if (IsBuildingResult(recipeData))
            {
                return "건축 준비 불가";
            }

            return "인벤토리 공간 부족";
        }

        public bool IsRecipeVisibleInCraftingUI(CraftingRecipeData recipeData)
        {
            return recipeData != null
                && GameDataCheckRules.IsImplementedCraftingRecipeResultType(recipeData.ResultDataType);
        }

        private bool IsResourceResult(CraftingRecipeData recipeData)
        {
            if (recipeData == null)
            {
                return false;
            }

            return string.Equals(
                recipeData.ResultDataType,
                GameDataCheckRules.RecipeResultTypeResource,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsBuildingResult(CraftingRecipeData recipeData)
        {
            if (recipeData == null)
            {
                return false;
            }

            return string.Equals(
                recipeData.ResultDataType,
                GameDataCheckRules.RecipeResultTypeBuilding,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsCinderHeartUpgradeResult(CraftingRecipeData recipeData)
        {
            if (recipeData == null)
            {
                return false;
            }

            return string.Equals(
                recipeData.ResultDataType,
                GameDataCheckRules.RecipeResultTypeCinderHeartUpgrade,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool TryGrantPreparedBuilding(CraftingRecipeData recipeData)
        {
            if (GameManager.Inst == null)
            {
                return false;
            }

            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
            if (inventoryModel == null)
            {
                return false;
            }

            return inventoryModel.TryAddPreparedBuilding(recipeData.ResultItemId, recipeData.ResultCount);
        }

        private bool CanGrantPreparedBuilding()
        {
            return GameManager.Inst != null && GameManager.Inst.PlayerInventoryModel != null;
        }

        private bool CanAddInventoryItem(string itemId, InventoryItemType itemType, int amount)
        {
            if (GameManager.Inst == null)
            {
                return false;
            }

            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
            return inventoryModel != null && inventoryModel.CanAddItem(itemId, itemType, amount);
        }

        private void RefundRecipeCost(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            if (recipeData == null || playerModel == null)
            {
                return;
            }

            for (int i = 0; i < recipeData.Costs.Count; i++)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null)
                {
                    continue;
                }

                playerModel.AddResource(costData.ResourceId, costData.Amount);
            }
        }

        private bool TryConvertToItemType(string resultDataType, string resultItemId, out InventoryItemType itemType)
        {
            itemType = InventoryItemType.Tool;

            if (string.Equals(resultDataType, GameDataCheckRules.RecipeResultTypeTool, StringComparison.OrdinalIgnoreCase))
            {
                itemType = InventoryItemType.Tool;
                return true;
            }

            if (string.Equals(resultDataType, GameDataCheckRules.RecipeResultTypeWeapon, StringComparison.OrdinalIgnoreCase))
            {
                itemType = InventoryItemType.Weapon;
                return true;
            }

            if (string.Equals(resultDataType, GameDataCheckRules.RecipeResultTypeArmor, StringComparison.OrdinalIgnoreCase))
            {
                itemType = ResolveArmorItemType(resultItemId);
                return true;
            }

            return false;
        }

        private InventoryItemType ResolveArmorItemType(string resultItemId)
        {
            if (string.IsNullOrEmpty(resultItemId))
            {
                return InventoryItemType.Armor;
            }

            if (resultItemId.IndexOf("helmet", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return InventoryItemType.Helmet;
            }

            if (resultItemId.IndexOf("boots", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return InventoryItemType.Boots;
            }

            return InventoryItemType.Armor;
        }

        private void NotifyRecipeCrafted(CraftingRecipeData recipeData)
        {
            if (RecipeCraftedGlobal == null)
            {
                return;
            }

            RecipeCraftedGlobal(recipeData);
        }
    }
}
