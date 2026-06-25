using System;
using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 제작 UI 노출, 보상 후보 필터, 에디터 Check 도구가 함께 사용하는 데이터 구현 기준입니다.
    // JSON 후보가 실제 구현보다 앞서갈 때 플레이어에게 미완성 기능이 노출되지 않게 막습니다.
    public static class GameDataCheckRules
    {
        public const string RecipeResultTypeResource = "Resource";
        public const string RecipeResultTypeTool = "Tool";
        public const string RecipeResultTypeWeapon = "Weapon";
        public const string RecipeResultTypeArmor = "Armor";
        public const string RecipeResultTypeBuilding = "Building";
        public const string RecipeResultTypeCinderHeartUpgrade = "CinderHeartUpgrade";

        public const string RewardEffectCinderHeartAttackDamageAdd = "CinderHeartAttackDamageAdd";
        public const string RewardEffectCinderHeartMaxHealthAdd = "CinderHeartMaxHealthAdd";
        public const string RewardEffectCinderHeartHealFlat = "CinderHeartHealFlat";
        public const string RewardEffectCinderHeartHealRate = "CinderHeartHealRate";
        public const string RewardEffectPlayerHealRate = "PlayerHealRate";
        public const string RewardEffectPlayerReviveRate = "PlayerReviveRate";
        public const string RewardEffectPlayerMaxHealthAdd = "PlayerMaxHealthAdd";
        public const string RewardEffectPlayerMaxStaminaAdd = "PlayerMaxStaminaAdd";
        public const string RewardEffectPlayerStaminaRecoveryAdd = "PlayerStaminaRecoveryAdd";
        public const string RewardEffectPlayerMaxSatietyAdd = "PlayerMaxSatietyAdd";
        public const string RewardEffectPlayerAttackDamageAdd = "PlayerAttackDamageAdd";

        private static readonly HashSet<string> SupportedCraftingRecipeResultTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            RecipeResultTypeResource,
            RecipeResultTypeTool,
            RecipeResultTypeWeapon,
            RecipeResultTypeArmor,
            RecipeResultTypeBuilding,
            RecipeResultTypeCinderHeartUpgrade
        };

        private static readonly HashSet<string> ImplementedCraftingRecipeResultTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            RecipeResultTypeResource,
            RecipeResultTypeTool,
            RecipeResultTypeWeapon,
            RecipeResultTypeArmor,
            RecipeResultTypeBuilding
        };

        private static readonly HashSet<string> ImplementedCinderHeartRewardEffectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            RewardEffectCinderHeartAttackDamageAdd,
            RewardEffectCinderHeartMaxHealthAdd,
            RewardEffectCinderHeartHealFlat,
            RewardEffectCinderHeartHealRate,
            RewardEffectPlayerHealRate,
            RewardEffectPlayerReviveRate,
            RewardEffectPlayerMaxHealthAdd,
            RewardEffectPlayerMaxStaminaAdd,
            RewardEffectPlayerStaminaRecoveryAdd,
            RewardEffectPlayerMaxSatietyAdd,
            RewardEffectPlayerAttackDamageAdd
        };

        public static bool IsSupportedCraftingRecipeResultType(string resultDataType)
        {
            return string.IsNullOrEmpty(resultDataType) == false
                && SupportedCraftingRecipeResultTypes.Contains(resultDataType);
        }

        public static bool IsImplementedCraftingRecipeResultType(string resultDataType)
        {
            return string.IsNullOrEmpty(resultDataType) == false
                && ImplementedCraftingRecipeResultTypes.Contains(resultDataType);
        }

        public static bool IsImplementedCinderHeartRewardEffect(string effectType)
        {
            return string.IsNullOrEmpty(effectType) == false
                && ImplementedCinderHeartRewardEffectTypes.Contains(effectType);
        }
    }
}
