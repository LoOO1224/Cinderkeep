using System;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // Crafting UI and stations ask this component to validate cost, pay cost, and grant the result.
    public sealed class CraftingRecipeExecutor : MonoBehaviour
    {
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

            return TryGrantRecipeResult(recipeData, playerModel);
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

            InventoryItemType itemType;
            return TryConvertToItemType(recipeData.ResultDataType, recipeData.ResultItemId, out itemType);
        }

        private bool IsResourceResult(CraftingRecipeData recipeData)
        {
            if (recipeData == null)
            {
                return false;
            }

            return string.Equals(recipeData.ResultDataType, "Resource", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryConvertToItemType(string resultDataType, string resultItemId, out InventoryItemType itemType)
        {
            itemType = InventoryItemType.Tool;

            if (string.Equals(resultDataType, "Tool", StringComparison.OrdinalIgnoreCase))
            {
                itemType = InventoryItemType.Tool;
                return true;
            }

            if (string.Equals(resultDataType, "Weapon", StringComparison.OrdinalIgnoreCase))
            {
                itemType = InventoryItemType.Weapon;
                return true;
            }

            if (string.Equals(resultDataType, "Armor", StringComparison.OrdinalIgnoreCase))
            {
                itemType = ResolveArmorItemType(resultItemId);
                return true;
            }

            if (string.Equals(resultDataType, "Building", StringComparison.OrdinalIgnoreCase))
            {
                itemType = InventoryItemType.Building;
                return true;
            }

            if (string.Equals(resultDataType, "CinderHeartUpgrade", StringComparison.OrdinalIgnoreCase))
            {
                itemType = InventoryItemType.CinderHeartUpgrade;
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
    }
}
