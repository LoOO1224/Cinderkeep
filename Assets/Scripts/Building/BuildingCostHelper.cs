using System.Text;
using UnityEngine;

// 5.00 direction: Supports base construction, defense objects, and building damage in the 5.00 loop.
// 5.01+ note: Keep placement, cost, health, tower attack, and upgrade rules split so 5.01+ defenses can expand.
namespace Cinderkeep.Gameplay
{
    // 건축 비용 확인/차감을 담당하는 클래스입니다.
    public static class BuildingCostHelper
    {
        /// <summary>
        /// BuildingData의 CraftingRecipeId로 제작법(건축비용표)을 조회합니다.
        /// </summary>
        /// <param name="buildingData"></param>
        /// <param name="gameDataManager"></param>
        /// <returns></returns>
        public static CraftingRecipeData GetBuildRecipe(
            BuildingData buildingData,
            GameDataManager gameDataManager)
        {
            if (buildingData == null || gameDataManager == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(buildingData.CraftingRecipeId))
            {
                return null;
            }

            return gameDataManager.GetCraftingRecipe(buildingData.CraftingRecipeId);
        }

        /// <summary>
        /// 플레이어 보유 자원이 충분한지 확인합니다. (차감은 이루어지지 않습니다.)
        /// </summary>
        /// <param name="buildingData"></param>
        /// <param name="playerModel"></param>
        /// <param name="gameDataManager"></param>
        /// <returns></returns>
        public static bool CanPayBuildCost(
            BuildingData buildingData,
            PlayerModel playerModel,
            GameDataManager gameDataManager)
        {
            CraftingRecipeData recipeData = GetBuildRecipe(buildingData, gameDataManager);
            if (recipeData == null || playerModel == null)
            {
                return false;
            }

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

        /// <summary>
        /// 충분한 자원이 있을 때만 PlayerModel.UseResource로 실제 차감합니다.
        /// </summary>
        /// <param name="buildingData"></param>
        /// <param name="playerModel"></param>
        /// <param name="gameDataManager"></param>
        /// <returns></returns>
        public static bool TryPayBuildCost(
            BuildingData buildingData,
            PlayerModel playerModel,
            GameDataManager gameDataManager)
        {
            if (CanPayBuildCost(buildingData, playerModel, gameDataManager) == false)
            {
                return false;
            }

            CraftingRecipeData recipeData = GetBuildRecipe(buildingData, gameDataManager);
            for (int i = 0; i < recipeData.Costs.Count; i++)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null)
                {
                    continue;
                }

                if (playerModel.UseResource(costData.ResourceId, costData.Amount) == false)
                {
                    Debug.LogWarning("BuildingCostHelper: 건축 비용 차감 중 문제가 생겼습니다. building=" + buildingData.Id);

                    return false;
                }
            }

            return true;
        }

        public static bool CanPayUpgradeCostDifference(
            BuildingData fromBuildingData,
            BuildingData toBuildingData,
            PlayerModel playerModel,
            GameDataManager gameDataManager)
        {
            if (toBuildingData == null || playerModel == null || gameDataManager == null)
            {
                return false;
            }

            CraftingRecipeData toRecipeData = GetBuildRecipe(toBuildingData, gameDataManager);
            if (toRecipeData == null)
            {
                return false;
            }

            for (int i = 0; i < toRecipeData.Costs.Count; i++)
            {
                CraftingCostData toCost = toRecipeData.Costs[i];
                if (toCost == null)
                {
                    continue;
                }

                int requiredAmount = GetUpgradeDifferenceAmount(fromBuildingData, toCost.ResourceId, toCost.Amount, gameDataManager);
                if (requiredAmount > 0 && playerModel.HasResource(toCost.ResourceId, requiredAmount) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryPayUpgradeCostDifference(
            BuildingData fromBuildingData,
            BuildingData toBuildingData,
            PlayerModel playerModel,
            GameDataManager gameDataManager)
        {
            if (CanPayUpgradeCostDifference(fromBuildingData, toBuildingData, playerModel, gameDataManager) == false)
            {
                return false;
            }

            CraftingRecipeData toRecipeData = GetBuildRecipe(toBuildingData, gameDataManager);
            for (int i = 0; i < toRecipeData.Costs.Count; i++)
            {
                CraftingCostData toCost = toRecipeData.Costs[i];
                if (toCost == null)
                {
                    continue;
                }

                int requiredAmount = GetUpgradeDifferenceAmount(fromBuildingData, toCost.ResourceId, toCost.Amount, gameDataManager);
                if (requiredAmount <= 0)
                {
                    continue;
                }

                if (playerModel.UseResource(toCost.ResourceId, requiredAmount) == false)
                {
                    Debug.LogWarning("BuildingCostHelper: 건축 업그레이드 차액 차감에 실패했습니다. building=" + toBuildingData.Id);
                    return false;
                }
            }

            return true;
        }

        private static int GetUpgradeDifferenceAmount(
            BuildingData fromBuildingData,
            string resourceId,
            int toAmount,
            GameDataManager gameDataManager)
        {
            int fromAmount = GetRecipeCostAmount(fromBuildingData, resourceId, gameDataManager);
            return Mathf.Max(0, toAmount - fromAmount);
        }

        private static int GetRecipeCostAmount(
            BuildingData buildingData,
            string resourceId,
            GameDataManager gameDataManager)
        {
            CraftingRecipeData recipeData = GetBuildRecipe(buildingData, gameDataManager);
            if (recipeData == null || string.IsNullOrEmpty(resourceId))
            {
                return 0;
            }

            int totalAmount = 0;
            for (int i = 0; i < recipeData.Costs.Count; i++)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null || string.Equals(costData.ResourceId, resourceId, System.StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                totalAmount += costData.Amount;
            }

            return totalAmount;
        }

        // 부족한 재료를 사람이 읽기 쉬운 형태로 만든 뒤 반환합니다.
        public static string GetNotEnoughResourceLog(
            BuildingData buildingData,
            PlayerModel playerModel,
            GameDataManager gameDataManager)
        {
            if (buildingData == null)
            {
                return "건축 실패: 건축 데이터가 없습니다.";
            }

            if (playerModel == null)
            {
                return "건축 실패: 플레이어 데이터가 없습니다.";
            }

            CraftingRecipeData recipeData = GetBuildRecipe(buildingData, gameDataManager);
            if (recipeData == null)
            {
                return "건축 실패: 건축 비용 데이터를 찾을 수 없습니다.";
            }

            StringBuilder logBuilder = new StringBuilder();
            logBuilder.Append("건축 실패: 자원이 부족합니다.");

            for (int i = 0; i < recipeData.Costs.Count; ++i)
            {
                CraftingCostData costData = recipeData.Costs[i];
                if (costData == null)
                {
                    continue;
                }

                int ownedAmount = playerModel.GetResourceAmount(costData.ResourceId);
                if (ownedAmount < costData.Amount)
                {
                    logBuilder.Append(" [");
                    logBuilder.Append(costData.ResourceId);
                    logBuilder.Append(" 필요 ");
                    logBuilder.Append(costData.Amount);
                    logBuilder.Append(", 보유 ");
                    logBuilder.Append(ownedAmount);
                    logBuilder.Append("]");
                }
            }

            return logBuilder.ToString();
        }
    }
}
