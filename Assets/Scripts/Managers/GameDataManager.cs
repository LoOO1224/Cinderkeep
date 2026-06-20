using System.Collections.Generic;
using UnityEngine;

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
            _enemyDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: enemy JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            EnemyDataCatalog catalog = JsonUtility.FromJson<EnemyDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: enemy JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_enemyDataList, catalog.Items[i]);
            }
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
            _resourceDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: resource JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            ResourceDataCatalog catalog = JsonUtility.FromJson<ResourceDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: resource JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_resourceDataList, catalog.Items[i]);
            }
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
            _harvestNodeDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: harvest node JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            HarvestNodeDataCatalog catalog = JsonUtility.FromJson<HarvestNodeDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: harvest node JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_harvestNodeDataList, catalog.Items[i]);
            }
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
            _toolDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: tool JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            ToolDataCatalog catalog = JsonUtility.FromJson<ToolDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: tool JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_toolDataList, catalog.Items[i]);
            }
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
            _weaponDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: weapon JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            WeaponDataCatalog catalog = JsonUtility.FromJson<WeaponDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: weapon JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_weaponDataList, catalog.Items[i]);
            }
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
            _armorDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: armor JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            ArmorDataCatalog catalog = JsonUtility.FromJson<ArmorDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: armor JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_armorDataList, catalog.Items[i]);
            }
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
            _buildingDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: building JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            BuildingDataCatalog catalog = JsonUtility.FromJson<BuildingDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: building JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_buildingDataList, catalog.Items[i]);
            }
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
            _craftingRecipeDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: crafting recipe JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            CraftingRecipeDataCatalog catalog = JsonUtility.FromJson<CraftingRecipeDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: crafting recipe JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_craftingRecipeDataList, catalog.Items[i]);
            }
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
            _craftingStationDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: crafting station JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            CraftingStationDataCatalog catalog = JsonUtility.FromJson<CraftingStationDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: crafting station JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_craftingStationDataList, catalog.Items[i]);
            }
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
            _smeltingRecipeDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: smelting recipe JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            SmeltingRecipeDataCatalog catalog = JsonUtility.FromJson<SmeltingRecipeDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: smelting recipe JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_smeltingRecipeDataList, catalog.Items[i]);
            }
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
            _enemySpawnRuleDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: enemy spawn rule JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            EnemySpawnRuleDataCatalog catalog = JsonUtility.FromJson<EnemySpawnRuleDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: enemy spawn rule JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_enemySpawnRuleDataList, catalog.Items[i]);
            }
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
            _gameFlowPhaseDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: game flow phase JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            GameFlowPhaseDataCatalog catalog = JsonUtility.FromJson<GameFlowPhaseDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: game flow phase JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_gameFlowPhaseDataList, catalog.Items[i]);
            }
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
            _lootDropDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: loot drop JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            LootDropDataCatalog catalog = JsonUtility.FromJson<LootDropDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: loot drop JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_lootDropDataList, catalog.Items[i]);
            }
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
            _cinderHeartUpgradeDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: CinderHeart upgrade JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            CinderHeartUpgradeDataCatalog catalog = JsonUtility.FromJson<CinderHeartUpgradeDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: CinderHeart upgrade JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_cinderHeartUpgradeDataList, catalog.Items[i]);
            }
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

        public void LoadStatusEffectData(string resourcePath)
        {
            _statusEffectDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: status effect JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            StatusEffectDataCatalog catalog = JsonUtility.FromJson<StatusEffectDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: status effect JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_statusEffectDataList, catalog.Items[i]);
            }
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
            _bossDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: boss JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            BossDataCatalog catalog = JsonUtility.FromJson<BossDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: boss JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_bossDataList, catalog.Items[i]);
            }
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
            _bossPatternDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: boss pattern JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            BossPatternDataCatalog catalog = JsonUtility.FromJson<BossPatternDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: boss pattern JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_bossPatternDataList, catalog.Items[i]);
            }
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
            _buildingUpgradeDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: building upgrade JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            BuildingUpgradeDataCatalog catalog = JsonUtility.FromJson<BuildingUpgradeDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: building upgrade JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_buildingUpgradeDataList, catalog.Items[i]);
            }
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

        private void AddData<TData>(Dictionary<string, TData> target, TData data)
            where TData : GameDataBase
        {
            if (target == null || data == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(data.Id))
            {
                return;
            }

            if (target.ContainsKey(data.Id))
            {
                target[data.Id] = data;
                return;
            }

            target.Add(data.Id, data);
        }
    }
}
