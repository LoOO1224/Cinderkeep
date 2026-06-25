using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 변하지 않는 JSON 게임 데이터를 로드하고 외부 조회 창구를 제공하는 허브입니다.
    // 실제 Dictionary 소유권은 기능별 카탈로그가 갖고, 이 클래스는 기존 API 호환과 초기화 순서만 담당합니다.
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

        private readonly GameResourceDataCatalog _resourceCatalog = new GameResourceDataCatalog();
        private readonly GameCombatDataCatalog _combatCatalog = new GameCombatDataCatalog();
        private readonly GameCraftingDataCatalog _craftingCatalog = new GameCraftingDataCatalog();
        private readonly GameFlowDataCatalog _flowCatalog = new GameFlowDataCatalog();
        private readonly GameCinderHeartDataCatalog _cinderHeartCatalog = new GameCinderHeartDataCatalog();
        private bool _isInitialized;

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public IReadOnlyDictionary<string, EnemyData> EnemyDataList
        {
            get { return _combatCatalog.EnemyDataList; }
        }

        public IReadOnlyDictionary<string, ResourceData> ResourceDataList
        {
            get { return _resourceCatalog.ResourceDataList; }
        }

        public IReadOnlyDictionary<string, HarvestNodeData> HarvestNodeDataList
        {
            get { return _resourceCatalog.HarvestNodeDataList; }
        }

        public IReadOnlyDictionary<string, ToolData> ToolDataList
        {
            get { return _resourceCatalog.ToolDataList; }
        }

        public IReadOnlyDictionary<string, WeaponData> WeaponDataList
        {
            get { return _combatCatalog.WeaponDataList; }
        }

        public IReadOnlyDictionary<string, ArmorData> ArmorDataList
        {
            get { return _combatCatalog.ArmorDataList; }
        }

        public IReadOnlyDictionary<string, BuildingData> BuildingDataList
        {
            get { return _craftingCatalog.BuildingDataList; }
        }

        public IReadOnlyDictionary<string, CraftingRecipeData> CraftingRecipeDataList
        {
            get { return _craftingCatalog.CraftingRecipeDataList; }
        }

        public IReadOnlyDictionary<string, CraftingStationData> CraftingStationDataList
        {
            get { return _craftingCatalog.CraftingStationDataList; }
        }

        public IReadOnlyDictionary<string, SmeltingRecipeData> SmeltingRecipeDataList
        {
            get { return _craftingCatalog.SmeltingRecipeDataList; }
        }

        public IReadOnlyDictionary<string, EnemySpawnRuleData> EnemySpawnRuleDataList
        {
            get { return _flowCatalog.EnemySpawnRuleDataList; }
        }

        public IReadOnlyDictionary<string, GameFlowPhaseData> GameFlowPhaseDataList
        {
            get { return _flowCatalog.GameFlowPhaseDataList; }
        }

        public IReadOnlyDictionary<string, LootDropData> LootDropDataList
        {
            get { return _combatCatalog.LootDropDataList; }
        }

        public IReadOnlyDictionary<string, CinderHeartUpgradeData> CinderHeartUpgradeDataList
        {
            get { return _cinderHeartCatalog.CinderHeartUpgradeDataList; }
        }

        public IReadOnlyDictionary<string, CinderHeartSkillData> CinderHeartSkillDataList
        {
            get { return _cinderHeartCatalog.CinderHeartSkillDataList; }
        }

        public IReadOnlyDictionary<string, StatusEffectData> StatusEffectDataList
        {
            get { return _combatCatalog.StatusEffectDataList; }
        }

        public IReadOnlyDictionary<string, BossData> BossDataList
        {
            get { return _combatCatalog.BossDataList; }
        }

        public IReadOnlyDictionary<string, BossPatternData> BossPatternDataList
        {
            get { return _combatCatalog.BossPatternDataList; }
        }

        public IReadOnlyDictionary<string, BuildingUpgradeData> BuildingUpgradeDataList
        {
            get { return _craftingCatalog.BuildingUpgradeDataList; }
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
            _combatCatalog.LoadEnemyData(resourcePath);
        }

        public EnemyData GetEnemy(string id)
        {
            return _combatCatalog.GetEnemy(id);
        }

        public void LoadResourceData(string resourcePath)
        {
            _resourceCatalog.LoadResourceData(resourcePath);
        }

        public ResourceData GetResource(string id)
        {
            return _resourceCatalog.GetResource(id);
        }

        public void LoadHarvestNodeData(string resourcePath)
        {
            _resourceCatalog.LoadHarvestNodeData(resourcePath);
        }

        public HarvestNodeData GetHarvestNode(string id)
        {
            return _resourceCatalog.GetHarvestNode(id);
        }

        public void LoadToolData(string resourcePath)
        {
            _resourceCatalog.LoadToolData(resourcePath);
        }

        public ToolData GetTool(string id)
        {
            return _resourceCatalog.GetTool(id);
        }

        public void LoadWeaponData(string resourcePath)
        {
            _combatCatalog.LoadWeaponData(resourcePath);
        }

        public WeaponData GetWeapon(string id)
        {
            return _combatCatalog.GetWeapon(id);
        }

        public void LoadArmorData(string resourcePath)
        {
            _combatCatalog.LoadArmorData(resourcePath);
        }

        public ArmorData GetArmor(string id)
        {
            return _combatCatalog.GetArmor(id);
        }

        public void LoadBuildingData(string resourcePath)
        {
            _craftingCatalog.LoadBuildingData(resourcePath);
        }

        public BuildingData GetBuilding(string id)
        {
            return _craftingCatalog.GetBuilding(id);
        }

        public void LoadCraftingRecipeData(string resourcePath)
        {
            _craftingCatalog.LoadCraftingRecipeData(resourcePath);
        }

        public CraftingRecipeData GetCraftingRecipe(string id)
        {
            return _craftingCatalog.GetCraftingRecipe(id);
        }

        public void LoadCraftingStationData(string resourcePath)
        {
            _craftingCatalog.LoadCraftingStationData(resourcePath);
        }

        public CraftingStationData GetCraftingStation(string id)
        {
            return _craftingCatalog.GetCraftingStation(id);
        }

        public void LoadSmeltingRecipeData(string resourcePath)
        {
            _craftingCatalog.LoadSmeltingRecipeData(resourcePath);
        }

        public SmeltingRecipeData GetSmeltingRecipe(string id)
        {
            return _craftingCatalog.GetSmeltingRecipe(id);
        }

        public void LoadEnemySpawnRuleData(string resourcePath)
        {
            _flowCatalog.LoadEnemySpawnRuleData(resourcePath);
        }

        public EnemySpawnRuleData GetEnemySpawnRule(string id)
        {
            return _flowCatalog.GetEnemySpawnRule(id);
        }

        public void LoadGameFlowPhaseData(string resourcePath)
        {
            _flowCatalog.LoadGameFlowPhaseData(resourcePath);
        }

        public GameFlowPhaseData GetGameFlowPhase(string id)
        {
            return _flowCatalog.GetGameFlowPhase(id);
        }

        public void LoadLootDropData(string resourcePath)
        {
            _combatCatalog.LoadLootDropData(resourcePath);
        }

        public LootDropData GetLootDrop(string id)
        {
            return _combatCatalog.GetLootDrop(id);
        }

        public void LoadCinderHeartUpgradeData(string resourcePath)
        {
            _cinderHeartCatalog.LoadCinderHeartUpgradeData(resourcePath);
        }

        public CinderHeartUpgradeData GetCinderHeartUpgrade(string id)
        {
            return _cinderHeartCatalog.GetCinderHeartUpgrade(id);
        }

        public void LoadCinderHeartSkillData(string resourcePath)
        {
            _cinderHeartCatalog.LoadCinderHeartSkillData(resourcePath);
        }

        public CinderHeartSkillData GetCinderHeartSkill(string id)
        {
            return _cinderHeartCatalog.GetCinderHeartSkill(id);
        }

        public void LoadStatusEffectData(string resourcePath)
        {
            _combatCatalog.LoadStatusEffectData(resourcePath);
        }

        public StatusEffectData GetStatusEffect(string id)
        {
            return _combatCatalog.GetStatusEffect(id);
        }

        public void LoadBossData(string resourcePath)
        {
            _combatCatalog.LoadBossData(resourcePath);
        }

        public BossData GetBoss(string id)
        {
            return _combatCatalog.GetBoss(id);
        }

        public void LoadBossPatternData(string resourcePath)
        {
            _combatCatalog.LoadBossPatternData(resourcePath);
        }

        public BossPatternData GetBossPattern(string id)
        {
            return _combatCatalog.GetBossPattern(id);
        }

        public void LoadBuildingUpgradeData(string resourcePath)
        {
            _craftingCatalog.LoadBuildingUpgradeData(resourcePath);
        }

        public BuildingUpgradeData GetBuildingUpgrade(string id)
        {
            return _craftingCatalog.GetBuildingUpgrade(id);
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
