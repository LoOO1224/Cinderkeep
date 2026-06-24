using System;
using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 게임플레이와 에디터 검증이 함께 사용하는 데이터 규칙 모음입니다.
    // 제작 결과 타입과 구현된 CinderHeart 보상 EffectType 기준을 한 곳에서 관리합니다.
    public static class GameDataValidationRules
    {
        public const string RecipeResultTypeResource = "Resource";
        public const string RecipeResultTypeTool = "Tool";
        public const string RecipeResultTypeWeapon = "Weapon";
        public const string RecipeResultTypeArmor = "Armor";
        public const string RecipeResultTypeBuilding = "Building";
        public const string RecipeResultTypeCinderHeartUpgrade = "CinderHeartUpgrade";

        public const string RewardEffectCinderHeartAttackDamageAdd = "CinderHeartAttackDamageAdd";
        public const string RewardEffectCinderHeartMaxHealthAdd = "CinderHeartMaxHealthAdd";
        public const string RewardEffectPlayerHealRate = "PlayerHealRate";
        public const string RewardEffectPlayerReviveRate = "PlayerReviveRate";

        private static readonly HashSet<string> SupportedCraftingRecipeResultTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            RecipeResultTypeResource,
            RecipeResultTypeTool,
            RecipeResultTypeWeapon,
            RecipeResultTypeArmor,
            RecipeResultTypeBuilding,
            RecipeResultTypeCinderHeartUpgrade
        };

        private static readonly HashSet<string> ImplementedCinderHeartRewardEffectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            RewardEffectCinderHeartAttackDamageAdd,
            RewardEffectCinderHeartMaxHealthAdd,
            RewardEffectPlayerHealRate,
            RewardEffectPlayerReviveRate
        };

        public static bool IsSupportedCraftingRecipeResultType(string resultDataType)
        {
            return string.IsNullOrEmpty(resultDataType) == false
                && SupportedCraftingRecipeResultTypes.Contains(resultDataType);
        }

        public static bool IsImplementedCinderHeartRewardEffect(string effectType)
        {
            return string.IsNullOrEmpty(effectType) == false
                && ImplementedCinderHeartRewardEffectTypes.Contains(effectType);
        }
    }
}
