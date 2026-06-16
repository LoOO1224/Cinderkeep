using System;
using System.Collections.Generic;
using UnityEngine;

namespace OODong.Cinderkeep
{
    // Static Data(JSON)를 GameDataBase 계열 클래스로 매핑하는 관리자.
    // 강사님 규칙 기준: 변하지 않는 기획 데이터는 Excel -> JSON -> GameDataManager 흐름으로 가져온다.
    // 현재 JSON 위치: Assets/Resources/Cinderkeep/Data/*.json
    public sealed class GameDataManager : MonoBehaviour
    {
        private const string EnemyDataResourcePath = "Cinderkeep/Data/enemies";
        private const string BuildingDataResourcePath = "Cinderkeep/Data/buildings";
        private const string RelicDataResourcePath = "Cinderkeep/Data/relics";
        private const string FlameUpgradeDataResourcePath = "Cinderkeep/Data/flame_upgrades";
        private const string RecipeDataResourcePath = "Cinderkeep/Data/recipes";
        private const string ResourceDataResourcePath = "Cinderkeep/Data/resources";
        private const string CraftingRecipeDataResourcePath = "Cinderkeep/Data/crafting_recipes";
        private const string DayRuleDataResourcePath = "Cinderkeep/Data/day_rules";

        [SerializeField] private bool _loadResourcesOnAwake = true;

        private readonly Dictionary<string, CinderkeepEnemyData> _enemyDataById = new Dictionary<string, CinderkeepEnemyData>();
        private readonly Dictionary<string, CinderkeepBuildingData> _buildingDataById = new Dictionary<string, CinderkeepBuildingData>();
        private readonly Dictionary<string, CinderkeepRelicData> _relicDataById = new Dictionary<string, CinderkeepRelicData>();
        private readonly Dictionary<string, CinderkeepFlameUpgradeData> _flameUpgradeDataById = new Dictionary<string, CinderkeepFlameUpgradeData>();
        private readonly Dictionary<string, CinderkeepRecipeData> _recipeDataById = new Dictionary<string, CinderkeepRecipeData>();
        private readonly Dictionary<string, CinderkeepResourceData> _resourceDataById = new Dictionary<string, CinderkeepResourceData>();
        private readonly Dictionary<string, CinderkeepCraftingRecipeData> _craftingRecipeDataById = new Dictionary<string, CinderkeepCraftingRecipeData>();
        private readonly Dictionary<string, CinderkeepDayRuleData> _dayRuleDataById = new Dictionary<string, CinderkeepDayRuleData>();

        public static GameDataManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            if (_loadResourcesOnAwake)
            {
                LoadStaticData();
            }
        }

        public void LoadStaticData()
        {
            // JSON이 없으면 발표가 막히지 않도록 코드 기본값을 사용한다.
            // TODO(팀원 작업 요청): 실제 협업 단계에서는 Excel 원본과 JSON Export 규칙을 팀 문서에 고정해 주세요.
            _enemyDataById.Clear();
            _buildingDataById.Clear();
            _relicDataById.Clear();
            _flameUpgradeDataById.Clear();
            _recipeDataById.Clear();
            _resourceDataById.Clear();
            _craftingRecipeDataById.Clear();
            _dayRuleDataById.Clear();

            AddData(_enemyDataById, LoadCatalog<EnemyDataCatalog, CinderkeepEnemyData>(EnemyDataResourcePath, catalog => catalog.Items), CreateDefaultEnemyData());
            AddData(_buildingDataById, LoadCatalog<BuildingDataCatalog, CinderkeepBuildingData>(BuildingDataResourcePath, catalog => catalog.Items), CreateDefaultBuildingData());
            AddData(_relicDataById, LoadCatalog<RelicDataCatalog, CinderkeepRelicData>(RelicDataResourcePath, catalog => catalog.Items), CreateDefaultRelicData());
            AddData(_flameUpgradeDataById, LoadCatalog<FlameUpgradeDataCatalog, CinderkeepFlameUpgradeData>(FlameUpgradeDataResourcePath, catalog => catalog.Items), CreateDefaultFlameUpgradeData());
            AddData(_recipeDataById, LoadCatalog<RecipeDataCatalog, CinderkeepRecipeData>(RecipeDataResourcePath, catalog => catalog.Items), CreateDefaultRecipeData());
            AddData(_resourceDataById, LoadCatalog<ResourceDataCatalog, CinderkeepResourceData>(ResourceDataResourcePath, catalog => catalog.Items), CreateDefaultResourceData());
            AddData(_craftingRecipeDataById, LoadCatalog<CraftingRecipeDataCatalog, CinderkeepCraftingRecipeData>(CraftingRecipeDataResourcePath, catalog => catalog.Items), CreateDefaultCraftingRecipeData());
            AddData(_dayRuleDataById, LoadCatalog<DayRuleDataCatalog, CinderkeepDayRuleData>(DayRuleDataResourcePath, catalog => catalog.Items), CreateDefaultDayRuleData());
        }

        public CinderkeepEnemyData GetEnemyData(string id)
        {
            return GetData(_enemyDataById, id, "plant");
        }

        public CinderkeepBuildingData GetBuildingData(string id)
        {
            return GetData(_buildingDataById, id, "wall");
        }

        public CinderkeepRelicData GetRelicData(string id)
        {
            return GetData(_relicDataById, id, "ember_speed");
        }

        public CinderkeepFlameUpgradeData GetFlameUpgradeData(string id)
        {
            return GetData(_flameUpgradeDataById, id, "beam");
        }

        public CinderkeepRecipeData GetRecipeData(string id)
        {
            return GetData(_recipeDataById, id, "arrow_bundle");
        }

        public CinderkeepResourceData GetResourceData(string id)
        {
            return GetData(_resourceDataById, id, "stone");
        }

        public CinderkeepCraftingRecipeData GetCraftingRecipeData(string id)
        {
            return GetData(_craftingRecipeDataById, id, "build_wall");
        }

        public List<CinderkeepCraftingRecipeData> GetCraftingRecipeDataByCategory(string category)
        {
            List<CinderkeepCraftingRecipeData> recipes = new List<CinderkeepCraftingRecipeData>();
            foreach (CinderkeepCraftingRecipeData recipeData in _craftingRecipeDataById.Values)
            {
                if (recipeData != null && recipeData.Category == category)
                {
                    recipes.Add(recipeData);
                }
            }

            return recipes;
        }

        public CinderkeepDayRuleData GetDayRuleData(int dayIndex)
        {
            string dayId = $"day_{Mathf.Max(1, dayIndex):00}";
            if (_dayRuleDataById.TryGetValue(dayId, out CinderkeepDayRuleData dayRuleData))
            {
                return dayRuleData;
            }

            foreach (CinderkeepDayRuleData ruleData in _dayRuleDataById.Values)
            {
                if (ruleData != null && ruleData.DayIndex == dayIndex)
                {
                    return ruleData;
                }
            }

            return GetData(_dayRuleDataById, "day_01", "day_01");
        }

        private static TData[] LoadCatalog<TCatalog, TData>(string resourcePath, Func<TCatalog, TData[]> getItems)
            where TCatalog : class
            where TData : GameDataBase
        {
            // Resources.Load는 MVP용 동기 로딩이다.
            // TODO(팀원 작업 요청): 데이터 크기가 커지면 Addressables 비동기 로딩으로 ResourceManager에 이관해 주세요.
            TextAsset json = Resources.Load<TextAsset>(resourcePath);
            if (json == null || string.IsNullOrWhiteSpace(json.text))
            {
                return Array.Empty<TData>();
            }

            TCatalog catalog = JsonUtility.FromJson<TCatalog>(json.text);
            return catalog != null ? getItems(catalog) ?? Array.Empty<TData>() : Array.Empty<TData>();
        }

        private static void AddData<TData>(Dictionary<string, TData> target, TData[] loadedData, TData[] fallbackData)
            where TData : GameDataBase
        {
            AddData(target, loadedData);

            if (target.Count == 0)
            {
                AddData(target, fallbackData);
            }
        }

        private static void AddData<TData>(Dictionary<string, TData> target, TData[] data)
            where TData : GameDataBase
        {
            if (data == null)
            {
                return;
            }

            for (int i = 0; i < data.Length; i++)
            {
                TData item = data[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    continue;
                }

                target[item.Id] = item;
            }
        }

        private static TData GetData<TData>(Dictionary<string, TData> dataById, string id, string fallbackId)
            where TData : GameDataBase
        {
            if (!string.IsNullOrWhiteSpace(id) && dataById.TryGetValue(id, out TData data))
            {
                return data;
            }

            return dataById.TryGetValue(fallbackId, out TData fallback) ? fallback : null;
        }

        private static CinderkeepEnemyData[] CreateDefaultEnemyData()
        {
            CinderkeepEnemyData plant = new CinderkeepEnemyData
            {
                Id = "plant",
                DisplayName = "Ash Vine",
                Health = 3,
                MoveSpeed = 2.2f,
                StopDistance = 1.5f,
                AttackDamage = 5,
                AttackInterval = 1.1f,
                VisualScale = 1f
            };

            CinderkeepEnemyData wolf = new CinderkeepEnemyData
            {
                Id = "wolf",
                DisplayName = "Cinder Wolf",
                Health = 5,
                MoveSpeed = 3.2f,
                StopDistance = 1.3f,
                AttackDamage = 7,
                AttackInterval = 0.9f,
                VisualScale = 1.05f
            };

            CinderkeepEnemyData boss = new CinderkeepEnemyData
            {
                Id = "boss",
                DisplayName = "Cinder Warden",
                Health = 55,
                MoveSpeed = 1.6f,
                StopDistance = 2f,
                AttackDamage = 15,
                AttackInterval = 1.25f,
                VisualScale = 2.4f
            };

            return new[] { plant, wolf, boss };
        }

        private static CinderkeepBuildingData[] CreateDefaultBuildingData()
        {
            CinderkeepBuildingData wall = new CinderkeepBuildingData
            {
                Id = "wall",
                DisplayName = "Stone Wall",
                CostItemId = "Stone",
                CostAmount = 2,
                MaxHealth = 35
            };

            CinderkeepBuildingData turret = new CinderkeepBuildingData
            {
                Id = "turret",
                DisplayName = "Ember Turret",
                CostItemId = "Ore",
                CostAmount = 2,
                MaxHealth = 25
            };

            CinderkeepBuildingData trap = new CinderkeepBuildingData
            {
                Id = "trap",
                DisplayName = "Spike Trap",
                CostItemId = "Stone",
                CostAmount = 3,
                MaxHealth = 18
            };

            return new[] { wall, turret, trap };
        }

        private static CinderkeepRelicData[] CreateDefaultRelicData()
        {
            CinderkeepRelicData speed = new CinderkeepRelicData
            {
                Id = "ember_speed",
                DisplayName = "Ember Pace",
                Description = "Presentation MVP relic: movement pressure reward.",
                Value = 1.1f
            };

            return new[] { speed };
        }

        private static CinderkeepFlameUpgradeData[] CreateDefaultFlameUpgradeData()
        {
            CinderkeepFlameUpgradeData beam = new CinderkeepFlameUpgradeData
            {
                Id = "beam",
                DisplayName = "Flame Beam",
                Description = "FlameHeart targets the nearest enemy.",
                Value = 1f
            };

            return new[] { beam };
        }

        private static CinderkeepRecipeData[] CreateDefaultRecipeData()
        {
            CinderkeepRecipeData arrows = new CinderkeepRecipeData
            {
                Id = "arrow_bundle",
                DisplayName = "Arrow Bundle",
                CostItemId = "Stone",
                CostAmount = 1,
                ResultItemId = "Arrow",
                ResultAmount = 6
            };

            return new[] { arrows };
        }

        private static CinderkeepResourceData[] CreateDefaultResourceData()
        {
            CinderkeepResourceData stone = new CinderkeepResourceData
            {
                Id = "stone",
                DisplayName = "Stone",
                Tier = 1
            };

            CinderkeepResourceData ore = new CinderkeepResourceData
            {
                Id = "ore",
                DisplayName = "Ore",
                Tier = 2
            };

            return new[] { stone, ore };
        }

        private static CinderkeepCraftingRecipeData[] CreateDefaultCraftingRecipeData()
        {
            return new[]
            {
                CreateCraftingRecipeData("build_wall", "Stone Wall", "building", "field", "wall", 1, 1, "Stone", 3, string.Empty, 0, string.Empty, "Fixed defense line. Can be repaired or rebuilt at the same site."),
                CreateCraftingRecipeData("build_turret", "Ember Turret", "building", "field", "turret", 1, 2, "Ore", 2, "Stone", 2, string.Empty, "Auto attacks enemies near the FlameHeart."),
                CreateCraftingRecipeData("weapon_ember_bow", "Ember Bow", "weapon", "workbench", "ember_bow", 1, 2, "Ore", 3, "Stone", 2, string.Empty, "Ranged weapon upgrade slot for the 30-minute expansion."),
                CreateCraftingRecipeData("tool_iron_pickaxe", "Iron Pickaxe", "gatheringTool", "workbench", "iron_pickaxe", 1, 2, "Ore", 3, "Stone", 2, "Pickaxe", "Faster ore gathering and higher-tier node access."),
                CreateCraftingRecipeData("equipment_fur_cloak", "Fur Cloak", "equipment", "workbench", "fur_cloak", 1, 3, "Apple", 2, "Ore", 1, string.Empty, "Cold gauge mitigation hook for later survival systems.")
            };
        }

        private static CinderkeepCraftingRecipeData CreateCraftingRecipeData(
            string id,
            string displayName,
            string category,
            string stationId,
            string resultItemId,
            int resultAmount,
            int unlockDay,
            string costItemIdA,
            int costAmountA,
            string costItemIdB,
            int costAmountB,
            string requiredToolId,
            string description)
        {
            List<CinderkeepCraftingCostData> costs = new List<CinderkeepCraftingCostData>();
            if (!string.IsNullOrWhiteSpace(costItemIdA) && costAmountA > 0)
            {
                costs.Add(new CinderkeepCraftingCostData { ItemId = costItemIdA, Amount = costAmountA });
            }

            if (!string.IsNullOrWhiteSpace(costItemIdB) && costAmountB > 0)
            {
                costs.Add(new CinderkeepCraftingCostData { ItemId = costItemIdB, Amount = costAmountB });
            }

            return new CinderkeepCraftingRecipeData
            {
                Id = id,
                DisplayName = displayName,
                Category = category,
                StationId = stationId,
                ResultItemId = resultItemId,
                ResultAmount = resultAmount,
                UnlockDay = unlockDay,
                Costs = costs.ToArray(),
                RequiredToolId = requiredToolId,
                Description = description
            };
        }

        private static CinderkeepDayRuleData[] CreateDefaultDayRuleData()
        {
            return new[]
            {
                CreateDayRuleData(1, 180f, 120f, 15f, 180f, 6, 10, 4.4f, 32f, "plant", false, "First gather route: stone, ore, first chest, first wall."),
                CreateDayRuleData(2, 180f, 120f, 15f, 180f, 8, 12, 3.7f, 36f, "wolf", false, "Upgrade route: second resource tier and turret repair rhythm."),
                CreateDayRuleData(3, 180f, 120f, 15f, 180f, 10, 14, 3.1f, 40f, "wolf", false, "MVP finale prep: hold structures and stock arrows before the target-day boss."),
                CreateDayRuleData(4, 210f, 150f, 15f, 210f, 12, 16, 2.8f, 46f, "wolf", false, "Extended map route: outer ruins and additional traps."),
                CreateDayRuleData(5, 210f, 150f, 15f, 210f, 14, 18, 2.5f, 52f, "wolf", false, "Softlock check: higher tier tools but base resources still accessible."),
                CreateDayRuleData(6, 240f, 180f, 15f, 240f, 16, 20, 2.2f, 58f, "wolf", false, "Long-run pressure: larger patrol radius and repair stress."),
                CreateDayRuleData(7, 240f, 180f, 15f, 240f, 18, 24, 2f, 64f, "wolf", true, "30-minute finale: optional second boss day for long mode.")
            };
        }

        private static CinderkeepDayRuleData CreateDayRuleData(
            int dayIndex,
            float dayDuration,
            float nightDuration,
            float morningRewardDuration,
            float bossNightDuration,
            int nightSpawnMax,
            int bossSpawnMax,
            float spawnInterval,
            float spawnRadius,
            string enemyDataId,
            bool isBossDay,
            string dayObjective)
        {
            return new CinderkeepDayRuleData
            {
                Id = $"day_{dayIndex:00}",
                DayIndex = dayIndex,
                DayDuration = dayDuration,
                NightDuration = nightDuration,
                MorningRewardDuration = morningRewardDuration,
                BossNightDuration = bossNightDuration,
                NightSpawnMax = nightSpawnMax,
                BossSpawnMax = bossSpawnMax,
                SpawnInterval = spawnInterval,
                SpawnRadius = spawnRadius,
                EnemyDataId = enemyDataId,
                BossDataId = "boss",
                IsBossDay = isBossDay,
                DayObjective = dayObjective,
                NightObjective = isBossDay ? "Boss is approaching the FlameHeart. Survive and kill it." : "Defend the FlameHeart until morning."
            };
        }

        [Serializable]
        private sealed class EnemyDataCatalog
        {
            public CinderkeepEnemyData[] Items;
        }

        [Serializable]
        private sealed class BuildingDataCatalog
        {
            public CinderkeepBuildingData[] Items;
        }

        [Serializable]
        private sealed class RelicDataCatalog
        {
            public CinderkeepRelicData[] Items;
        }

        [Serializable]
        private sealed class FlameUpgradeDataCatalog
        {
            public CinderkeepFlameUpgradeData[] Items;
        }

        [Serializable]
        private sealed class RecipeDataCatalog
        {
            public CinderkeepRecipeData[] Items;
        }

        [Serializable]
        private sealed class ResourceDataCatalog
        {
            public CinderkeepResourceData[] Items;
        }

        [Serializable]
        private sealed class CraftingRecipeDataCatalog
        {
            public CinderkeepCraftingRecipeData[] Items;
        }

        [Serializable]
        private sealed class DayRuleDataCatalog
        {
            public CinderkeepDayRuleData[] Items;
        }
    }
}
