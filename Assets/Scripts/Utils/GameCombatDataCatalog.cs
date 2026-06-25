using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 전투, 적, 보스, 장비 데이터를 묶어 관리하는 GameDataManager 내부 카탈로그입니다.
    // 5.00 이후 보스/전투 확장이 늘어나도 GameDataManager 본문이 다시 커지지 않게 합니다.
    public sealed class GameCombatDataCatalog
    {
        private readonly Dictionary<string, EnemyData> _enemyDataList = new Dictionary<string, EnemyData>();
        private readonly Dictionary<string, WeaponData> _weaponDataList = new Dictionary<string, WeaponData>();
        private readonly Dictionary<string, ArmorData> _armorDataList = new Dictionary<string, ArmorData>();
        private readonly Dictionary<string, LootDropData> _lootDropDataList = new Dictionary<string, LootDropData>();
        private readonly Dictionary<string, StatusEffectData> _statusEffectDataList = new Dictionary<string, StatusEffectData>();
        private readonly Dictionary<string, BossData> _bossDataList = new Dictionary<string, BossData>();
        private readonly Dictionary<string, BossPatternData> _bossPatternDataList = new Dictionary<string, BossPatternData>();

        public IReadOnlyDictionary<string, EnemyData> EnemyDataList
        {
            get { return _enemyDataList; }
        }

        public IReadOnlyDictionary<string, WeaponData> WeaponDataList
        {
            get { return _weaponDataList; }
        }

        public IReadOnlyDictionary<string, ArmorData> ArmorDataList
        {
            get { return _armorDataList; }
        }

        public IReadOnlyDictionary<string, LootDropData> LootDropDataList
        {
            get { return _lootDropDataList; }
        }

        public IReadOnlyDictionary<string, StatusEffectData> StatusEffectDataList
        {
            get { return _statusEffectDataList; }
        }

        public IReadOnlyDictionary<string, BossData> BossDataList
        {
            get { return _bossDataList; }
        }

        public IReadOnlyDictionary<string, BossPatternData> BossPatternDataList
        {
            get { return _bossPatternDataList; }
        }

        public void LoadEnemyData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<EnemyData, EnemyDataCatalog>(_enemyDataList, resourcePath, "enemy", catalog => catalog.Items);
        }

        public EnemyData GetEnemy(string id)
        {
            return GameDataCatalogLookup.GetById(_enemyDataList, id);
        }

        public void LoadWeaponData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<WeaponData, WeaponDataCatalog>(_weaponDataList, resourcePath, "weapon", catalog => catalog.Items);
        }

        public WeaponData GetWeapon(string id)
        {
            return GameDataCatalogLookup.GetById(_weaponDataList, id);
        }

        public void LoadArmorData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<ArmorData, ArmorDataCatalog>(_armorDataList, resourcePath, "armor", catalog => catalog.Items);
        }

        public ArmorData GetArmor(string id)
        {
            return GameDataCatalogLookup.GetById(_armorDataList, id);
        }

        public void LoadLootDropData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<LootDropData, LootDropDataCatalog>(_lootDropDataList, resourcePath, "loot drop", catalog => catalog.Items);
        }

        public LootDropData GetLootDrop(string id)
        {
            return GameDataCatalogLookup.GetById(_lootDropDataList, id);
        }

        public void LoadStatusEffectData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<StatusEffectData, StatusEffectDataCatalog>(_statusEffectDataList, resourcePath, "status effect", catalog => catalog.Items);
        }

        public StatusEffectData GetStatusEffect(string id)
        {
            return GameDataCatalogLookup.GetById(_statusEffectDataList, id);
        }

        public void LoadBossData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BossData, BossDataCatalog>(_bossDataList, resourcePath, "boss", catalog => catalog.Items);
        }

        public BossData GetBoss(string id)
        {
            return GameDataCatalogLookup.GetById(_bossDataList, id);
        }

        public void LoadBossPatternData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BossPatternData, BossPatternDataCatalog>(_bossPatternDataList, resourcePath, "boss pattern", catalog => catalog.Items);
        }

        public BossPatternData GetBossPattern(string id)
        {
            return GameDataCatalogLookup.GetById(_bossPatternDataList, id);
        }
    }
}
