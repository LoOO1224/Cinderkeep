using System;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 제작법 실행을 담당하는 컴포넌트입니다.
    // 비용 확인, 자원 차감, 결과 지급만 담당하고 UI 표시는 다른 컴포넌트가 맡습니다.
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
                    Debug.LogWarning("CraftingRecipeExecutor: 제작 비용 차감 중 문제가 생겼습니다. recipe=" + recipeData.Id);
                    return false;
                }
            }

            return true;
        }

        private bool TryGrantRecipeResult(CraftingRecipeData recipeData, PlayerModel playerModel)
        {
            if (IsResourceResult(recipeData) == false)
            {
                Debug.LogWarning("CraftingRecipeExecutor: 아직 Resource 결과만 실제 지급합니다. recipe=" + recipeData.Id);
                return false;
            }

            playerModel.AddResource(recipeData.ResultItemId, recipeData.ResultCount);
            Debug.Log("CraftingRecipeExecutor: " + recipeData.DisplayName + " 제작을 완료했습니다.");
            return true;
        }

        private bool CanGrantRecipeResult(CraftingRecipeData recipeData)
        {
            return IsResourceResult(recipeData);
        }

        private bool IsResourceResult(CraftingRecipeData recipeData)
        {
            if (recipeData == null)
            {
                return false;
            }

            return string.Equals(recipeData.ResultDataType, "Resource", StringComparison.OrdinalIgnoreCase);
        }
    }
}
