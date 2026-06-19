using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 공용 계산과 데이터 로드 시작점을 모아두는 순수 도구 클래스입니다.
    // 오브젝트를 직접 찾지 않고, 필요한 값과 매니저를 파라미터로 받아 처리합니다.
    public static class GameUtil
    {
        public const string EnemyDataResourcePath = "Cinderkeep/data/enemies";
        public const string ResourceDataResourcePath = "Cinderkeep/data/resources";
        public const string HarvestNodeDataResourcePath = "Cinderkeep/data/harvest_nodes";
        public const string ToolDataResourcePath = "Cinderkeep/data/tools";
        public const string WeaponDataResourcePath = "Cinderkeep/data/weapons";
        public const string ArmorDataResourcePath = "Cinderkeep/data/armors";
        public const string BuildingDataResourcePath = "Cinderkeep/data/buildings";
        public const string CraftingRecipeDataResourcePath = "Cinderkeep/data/crafting_recipes";
        public const string EnemySpawnRuleDataResourcePath = "Cinderkeep/data/enemy_spawn_rules";
        public const string GameFlowPhaseDataResourcePath = "Cinderkeep/data/game_flow_phases";
        public const string LootDropDataResourcePath = "Cinderkeep/data/loot_drops";
        public const string CinderHeartUpgradeDataResourcePath = "Cinderkeep/data/cinderheart_upgrades";
        public const string StatusEffectDataResourcePath = "Cinderkeep/data/status_effects";
        public const string BossDataResourcePath = "Cinderkeep/data/bosses";
        public const string BossPatternDataResourcePath = "Cinderkeep/data/boss_patterns";
        public const string BuildingUpgradeDataResourcePath = "Cinderkeep/data/building_upgrades";

        public static void LoadFullData(GameDataManager gameDataManager)
        {
            if (gameDataManager == null)
            {
                Debug.LogWarning("GameUtil: GameDataManager reference is empty.");
                return;
            }

            gameDataManager.LoadEnemyData(gameDataManager.GetEnemyDataResourcePath());
            gameDataManager.LoadResourceData(gameDataManager.GetResourceDataResourcePath());
            gameDataManager.LoadHarvestNodeData(gameDataManager.GetHarvestNodeDataResourcePath());
            gameDataManager.LoadToolData(gameDataManager.GetToolDataResourcePath());
            gameDataManager.LoadWeaponData(gameDataManager.GetWeaponDataResourcePath());
            gameDataManager.LoadArmorData(gameDataManager.GetArmorDataResourcePath());
            gameDataManager.LoadBuildingData(gameDataManager.GetBuildingDataResourcePath());
            gameDataManager.LoadCraftingRecipeData(gameDataManager.GetCraftingRecipeDataResourcePath());
            gameDataManager.LoadEnemySpawnRuleData(gameDataManager.GetEnemySpawnRuleDataResourcePath());
            gameDataManager.LoadGameFlowPhaseData(gameDataManager.GetGameFlowPhaseDataResourcePath());
            gameDataManager.LoadLootDropData(gameDataManager.GetLootDropDataResourcePath());
            gameDataManager.LoadCinderHeartUpgradeData(gameDataManager.GetCinderHeartUpgradeDataResourcePath());
            gameDataManager.LoadStatusEffectData(gameDataManager.GetStatusEffectDataResourcePath());
            gameDataManager.LoadBossData(gameDataManager.GetBossDataResourcePath());
            gameDataManager.LoadBossPatternData(gameDataManager.GetBossPatternDataResourcePath());
            gameDataManager.LoadBuildingUpgradeData(gameDataManager.GetBuildingUpgradeDataResourcePath());
        }

        public static int GenerateNextInstanceId(int currentInstanceId)
        {
            return currentInstanceId + 1;
        }

        public static float GetRate(float currentValue, float maxValue)
        {
            if (maxValue <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(currentValue / maxValue);
        }
    }
}
