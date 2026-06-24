using System.Collections.Generic;
using UnityEngine;

// 5.00 direction: Coordinates a focused slice of the 5.00 game loop from scene and runtime references.
// 5.01+ note: Keep this manager as a thin hub; move calculations and feature rules into smaller systems/helpers.
namespace Cinderkeep.Gameplay
{
    // 변하지 않는 기획 데이터를 JSON에서 읽어 보관하는 매니저입니다.
    // 적, 도구, 무기, 방어구, 제작법처럼 엑셀에서 JSON으로 옮길 데이터를 이곳에서 관리합니다.
    public sealed class GameDataManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private string _enemyDataResourcePath = GameUtil.EnemyDataResourcePath;
        [SerializeField] private string _resourceDataResourcePath = GameUtil.ResourceDataResourcePath;
        [SerializeField] private string _harvestNodeDataResourcePath = GameUtil.HarvestNodeDataResourcePath;
        [SerializeField] private string _toolDataResourcePath = GameUtil.ToolDataResourcePath;
        [SerializeField] private string _weaponDataResourcePath = GameUtil.WeaponDataResourcePath;
        [SerializeField] private string _armorDataResourcePath = GameUtil.ArmorDataResourcePath;
        [SerializeField] private string _buildingDataResourcePath = GameUtil.BuildingDataResourcePath;
        [SerializeField] private string _craftingRecipeDataResourcePath = GameUtil.CraftingRecipeDataResourcePath;
        [SerializeField] private string _craftingStationDataResourcePath = GameUtil.CraftingStationDataResourcePath;
        [SerializeField] private string _smeltingRecipeDataResourcePath = GameUtil.SmeltingRecipeDataResourcePath;
        [SerializeField] private string _enemySpawnRuleDataResourcePath = GameUtil.EnemySpawnRuleDataResourcePath;
        [SerializeField] private string _gameFlowPhaseDataResourcePath = GameUtil.GameFlowPhaseDataResourcePath;
        [SerializeField] private string _lootDropDataResourcePath = GameUtil.LootDropDataResourcePath;
        [SerializeField] private string _cinderHeartUpgradeDataResourcePath = GameUtil.CinderHeartUpgradeDataResourcePath;
        [SerializeField] private string _cinderHeartSkillDataResourcePath = GameUtil.CinderHeartSkillDataResourcePath;
        [SerializeField] private string _statusEffectDataResourcePath = GameUtil.StatusEffectDataResourcePath;
        [SerializeField] private string _bossDataResourcePath = GameUtil.BossDataResourcePath;
        [SerializeField] private string _bossPatternDataResourcePath = GameUtil.BossPatternDataResourcePath;
        [SerializeField] private string _buildingUpgradeDataResourcePath = GameUtil.BuildingUpgradeDataResourcePath;

        private readonly Dictionary<string, EnemyData> _enemyDataList = new Dictionary<string, EnemyData>();
        private readonly Dictionary<string, ResourceData> _resourceDataList = new Dictionary<string, ResourceData>();
        private readonly Dictionary<string, HarvestNodeData> _harvestNodeDataList = new Dictionary<string, HarvestNodeData>();
        private readonly Dictionary<string, ToolData> _toolDataList = new Dictionary<string, ToolData>();
        private readonly Dictionary<string, WeaponData> _weaponDataList = new Dictionary<string, WeaponData>();
        private readonly Dictionary<string, ArmorData> _armorDataList = new Dictionary<string, ArmorData>();
        private readonly Dictionary<string, BuildingData> _buildingDataList = new Dictionary<string, BuildingData>();
        private readonly Dictionary<string, CraftingRecipeData> _craftingRecipeDataList = new Dictionary<string, CraftingRecipeData>();
        private readonly Dictionary<string, CraftingStationData> _craftingStationDataList = new Dictionary<string, CraftingStationData>();
        private readonly Dictionary<string, SmeltingRecipeData> _smeltingRecipeDataList = new Dictionary<string, SmeltingRecipeData>();
        private readonly Dictionary<string, EnemySpawnRuleData> _enemySpawnRuleDataList = new Dictionary<string, EnemySpawnRuleData>();
        private readonly Dictionary<string, GameFlowPhaseData> _gameFlowPhaseDataList = new Dictionary<string, GameFlowPhaseData>();
        private readonly Dictionary<string, LootDropData> _lootDropDataList = new Dictionary<string, LootDropData>();
        private readonly Dictionary<string, CinderHeartUpgradeData> _cinderHeartUpgradeDataList = new Dictionary<string, CinderHeartUpgradeData>();
        private readonly Dictionary<string, CinderHeartSkillData> _cinderHeartSkillDataList = new Dictionary<string, CinderHeartSkillData>();
        private readonly Dictionary<string, StatusEffectData> _statusEffectDataList = new Dictionary<string, StatusEffectData>();
        private readonly Dictionary<string, BossData> _bossDataList = new Dictionary<string, BossData>();
        private readonly Dictionary<string, BossPatternData> _bossPatternDataList = new Dictionary<string, BossPatternData>();
        private readonly Dictionary<string, BuildingUpgradeData> _buildingUpgradeDataList = new Dictionary<string, BuildingUpgradeData>();
        private bool _isInitialized;

        public IReadOnlyDictionary<string, EnemyData> EnemyDataList
        {
            get
            {
                return _enemyDataList;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        public IReadOnlyDictionary<string, ResourceData> ResourceDataList
        {
            get
            {
                return _resourceDataList;
            }
        }

        public IReadOnlyDictionary<string, HarvestNodeData> HarvestNodeDataList
        {
            get
            {
                return _harvestNodeDataList;
            }
        }

        public IReadOnlyDictionary<string, ToolData> ToolDataList
        {
            get
            {
                return _toolDataList;
            }
        }

        public IReadOnlyDictionary<string, WeaponData> WeaponDataList
        {
            get
            {
                return _weaponDataList;
            }
        }

        public IReadOnlyDictionary<string, ArmorData> ArmorDataList
        {
            get
            {
                return _armorDataList;
            }
        }

        public IReadOnlyDictionary<string, BuildingData> BuildingDataList
        {
            get
            {
                return _buildingDataList;
            }
        }

        public IReadOnlyDictionary<string, CraftingRecipeData> CraftingRecipeDataList
        {
            get
            {
                return _craftingRecipeDataList;
            }
        }

        public IReadOnlyDictionary<string, CraftingStationData> CraftingStationDataList
        {
            get
            {
                return _craftingStationDataList;
            }
        }

        public IReadOnlyDictionary<string, SmeltingRecipeData> SmeltingRecipeDataList
        {
            get
            {
                return _smeltingRecipeDataList;
            }
        }

        public IReadOnlyDictionary<string, EnemySpawnRuleData> EnemySpawnRuleDataList
        {
            get
            {
                return _enemySpawnRuleDataList;
            }
        }

        public IReadOnlyDictionary<string, GameFlowPhaseData> GameFlowPhaseDataList
        {
            get
            {
                return _gameFlowPhaseDataList;
            }
        }

        public IReadOnlyDictionary<string, LootDropData> LootDropDataList
        {
            get
            {
                return _lootDropDataList;
            }
        }

        public IReadOnlyDictionary<string, CinderHeartUpgradeData> CinderHeartUpgradeDataList
        {
            get
            {
                return _cinderHeartUpgradeDataList;
            }
        }

        public IReadOnlyDictionary<string, CinderHeartSkillData> CinderHeartSkillDataList
        {
            get
            {
                return _cinderHeartSkillDataList;
            }
        }

        public IReadOnlyDictionary<string, StatusEffectData> StatusEffectDataList
        {
            get
            {
                return _statusEffectDataList;
            }
        }

        public IReadOnlyDictionary<string, BossData> BossDataList
        {
            get
            {
                return _bossDataList;
            }
        }

        public IReadOnlyDictionary<string, BossPatternData> BossPatternDataList
        {
            get
            {
                return _bossPatternDataList;
            }
        }

        public IReadOnlyDictionary<string, BuildingUpgradeData> BuildingUpgradeDataList
        {
            get
            {
                return _buildingUpgradeDataList;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            GameUtil.LoadFullData(this);
            _isInitialized = true;
        }

        public void LoadEnemyData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<EnemyData, EnemyDataCatalog>(_enemyDataList, resourcePath, "enemy", catalog => catalog.Items);
        }

        public EnemyData GetEnemy(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_enemyDataList.ContainsKey(id))
            {
                return null;
            }

            return _enemyDataList[id];
        }

        public void LoadResourceData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<ResourceData, ResourceDataCatalog>(_resourceDataList, resourcePath, "resource", catalog => catalog.Items);
        }

        public ResourceData GetResource(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_resourceDataList.ContainsKey(id))
            {
                return null;
            }

            return _resourceDataList[id];
        }

        public void LoadHarvestNodeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<HarvestNodeData, HarvestNodeDataCatalog>(_harvestNodeDataList, resourcePath, "harvest node", catalog => catalog.Items);
        }

        public HarvestNodeData GetHarvestNode(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_harvestNodeDataList.ContainsKey(id))
            {
                return null;
            }

            return _harvestNodeDataList[id];
        }

        public void LoadToolData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<ToolData, ToolDataCatalog>(_toolDataList, resourcePath, "tool", catalog => catalog.Items);
        }

        public ToolData GetTool(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_toolDataList.ContainsKey(id))
            {
                return null;
            }

            return _toolDataList[id];
        }

        public void LoadWeaponData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<WeaponData, WeaponDataCatalog>(_weaponDataList, resourcePath, "weapon", catalog => catalog.Items);
        }

        public WeaponData GetWeapon(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_weaponDataList.ContainsKey(id))
            {
                return null;
            }

            return _weaponDataList[id];
        }

        public void LoadArmorData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<ArmorData, ArmorDataCatalog>(_armorDataList, resourcePath, "armor", catalog => catalog.Items);
        }

        public ArmorData GetArmor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_armorDataList.ContainsKey(id))
            {
                return null;
            }

            return _armorDataList[id];
        }

        public void LoadBuildingData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BuildingData, BuildingDataCatalog>(_buildingDataList, resourcePath, "building", catalog => catalog.Items);
        }

        public BuildingData GetBuilding(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_buildingDataList.ContainsKey(id))
            {
                return null;
            }

            return _buildingDataList[id];
        }

        public void LoadCraftingRecipeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<CraftingRecipeData, CraftingRecipeDataCatalog>(_craftingRecipeDataList, resourcePath, "crafting recipe", catalog => catalog.Items);
        }

        public CraftingRecipeData GetCraftingRecipe(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_craftingRecipeDataList.ContainsKey(id))
            {
                return null;
            }

            return _craftingRecipeDataList[id];
        }

        public void LoadCraftingStationData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<CraftingStationData, CraftingStationDataCatalog>(_craftingStationDataList, resourcePath, "crafting station", catalog => catalog.Items);
        }

        public CraftingStationData GetCraftingStation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_craftingStationDataList.ContainsKey(id))
            {
                return null;
            }

            return _craftingStationDataList[id];
        }

        public void LoadSmeltingRecipeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<SmeltingRecipeData, SmeltingRecipeDataCatalog>(_smeltingRecipeDataList, resourcePath, "smelting recipe", catalog => catalog.Items);
        }

        public SmeltingRecipeData GetSmeltingRecipe(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_smeltingRecipeDataList.ContainsKey(id))
            {
                return null;
            }

            return _smeltingRecipeDataList[id];
        }

        public void LoadEnemySpawnRuleData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<EnemySpawnRuleData, EnemySpawnRuleDataCatalog>(_enemySpawnRuleDataList, resourcePath, "enemy spawn rule", catalog => catalog.Items);
        }

        public EnemySpawnRuleData GetEnemySpawnRule(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_enemySpawnRuleDataList.ContainsKey(id))
            {
                return null;
            }

            return _enemySpawnRuleDataList[id];
        }

        public void LoadGameFlowPhaseData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<GameFlowPhaseData, GameFlowPhaseDataCatalog>(_gameFlowPhaseDataList, resourcePath, "game flow phase", catalog => catalog.Items);
        }

        public GameFlowPhaseData GetGameFlowPhase(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_gameFlowPhaseDataList.ContainsKey(id))
            {
                return null;
            }

            return _gameFlowPhaseDataList[id];
        }

        public void LoadLootDropData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<LootDropData, LootDropDataCatalog>(_lootDropDataList, resourcePath, "loot drop", catalog => catalog.Items);
        }

        public LootDropData GetLootDrop(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_lootDropDataList.ContainsKey(id))
            {
                return null;
            }

            return _lootDropDataList[id];
        }

        public void LoadCinderHeartUpgradeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<CinderHeartUpgradeData, CinderHeartUpgradeDataCatalog>(_cinderHeartUpgradeDataList, resourcePath, "CinderHeart upgrade", catalog => catalog.Items);
        }

        public CinderHeartUpgradeData GetCinderHeartUpgrade(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_cinderHeartUpgradeDataList.ContainsKey(id))
            {
                return null;
            }

            return _cinderHeartUpgradeDataList[id];
        }

        public void LoadCinderHeartSkillData(string resourcePath)
        {
            // 아침 보상 스킬 목록은 cinderheart_skills.json에서 로드합니다.
            // 새 보상은 Data 클래스보다 JSON의 _id, effectType, value를 먼저 맞추는 방식으로 추가합니다.
            GameDataCatalogLoader.LoadCatalog<CinderHeartSkillData, CinderHeartSkillDataCatalog>(_cinderHeartSkillDataList, resourcePath, "CinderHeart skill", catalog => catalog.Items);
        }

        public CinderHeartSkillData GetCinderHeartSkill(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_cinderHeartSkillDataList.ContainsKey(id))
            {
                return null;
            }

            return _cinderHeartSkillDataList[id];
        }

        public void LoadStatusEffectData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<StatusEffectData, StatusEffectDataCatalog>(_statusEffectDataList, resourcePath, "status effect", catalog => catalog.Items);
        }

        public StatusEffectData GetStatusEffect(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_statusEffectDataList.ContainsKey(id))
            {
                return null;
            }

            return _statusEffectDataList[id];
        }

        public void LoadBossData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BossData, BossDataCatalog>(_bossDataList, resourcePath, "boss", catalog => catalog.Items);
        }

        public BossData GetBoss(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_bossDataList.ContainsKey(id))
            {
                return null;
            }

            return _bossDataList[id];
        }

        public void LoadBossPatternData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BossPatternData, BossPatternDataCatalog>(_bossPatternDataList, resourcePath, "boss pattern", catalog => catalog.Items);
        }

        public BossPatternData GetBossPattern(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_bossPatternDataList.ContainsKey(id))
            {
                return null;
            }

            return _bossPatternDataList[id];
        }

        public void LoadBuildingUpgradeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BuildingUpgradeData, BuildingUpgradeDataCatalog>(_buildingUpgradeDataList, resourcePath, "building upgrade", catalog => catalog.Items);
        }

        public BuildingUpgradeData GetBuildingUpgrade(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_buildingUpgradeDataList.ContainsKey(id))
            {
                return null;
            }

            return _buildingUpgradeDataList[id];
        }

        public string GetEnemyDataResourcePath()
        {
            return _enemyDataResourcePath;
        }

        public string GetResourceDataResourcePath()
        {
            return _resourceDataResourcePath;
        }

        public string GetHarvestNodeDataResourcePath()
        {
            return _harvestNodeDataResourcePath;
        }

        public string GetToolDataResourcePath()
        {
            return _toolDataResourcePath;
        }

        public string GetWeaponDataResourcePath()
        {
            return _weaponDataResourcePath;
        }

        public string GetArmorDataResourcePath()
        {
            return _armorDataResourcePath;
        }

        public string GetBuildingDataResourcePath()
        {
            return _buildingDataResourcePath;
        }

        public string GetCraftingRecipeDataResourcePath()
        {
            return _craftingRecipeDataResourcePath;
        }

        public string GetCraftingStationDataResourcePath()
        {
            return _craftingStationDataResourcePath;
        }

        public string GetSmeltingRecipeDataResourcePath()
        {
            return _smeltingRecipeDataResourcePath;
        }

        public string GetEnemySpawnRuleDataResourcePath()
        {
            return _enemySpawnRuleDataResourcePath;
        }

        public string GetGameFlowPhaseDataResourcePath()
        {
            return _gameFlowPhaseDataResourcePath;
        }

        public string GetLootDropDataResourcePath()
        {
            return _lootDropDataResourcePath;
        }

        public string GetCinderHeartUpgradeDataResourcePath()
        {
            return _cinderHeartUpgradeDataResourcePath;
        }

        public string GetCinderHeartSkillDataResourcePath()
        {
            return _cinderHeartSkillDataResourcePath;
        }

        public string GetStatusEffectDataResourcePath()
        {
            return _statusEffectDataResourcePath;
        }

        public string GetBossDataResourcePath()
        {
            return _bossDataResourcePath;
        }

        public string GetBossPatternDataResourcePath()
        {
            return _bossPatternDataResourcePath;
        }

        public string GetBuildingUpgradeDataResourcePath()
        {
            return _buildingUpgradeDataResourcePath;
        }

    }
}
