using UnityEngine;

namespace OODong.Cinderkeep
{
    // Static Data: 자원 기획 데이터.
    // TODO(팀원 작업 요청): Excel 컬럼명과 이 필드명을 맞춘 뒤 JSON Export 규칙을 고정해 주세요.
    [System.Serializable]
    public sealed class CinderkeepResourceData : GameDataBase
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private int _tier = 1;

        public string DisplayName { get => _displayName; set => _displayName = value ?? string.Empty; }
        public int Tier { get => _tier; set => _tier = Mathf.Max(1, value); }
    }

    [System.Serializable]
    // Static Data: 제작식. MVP에서는 화살 묶음 정도만 쓰지만, 이후 장비/건축 제작으로 확장한다.
    public sealed class CinderkeepRecipeData : GameDataBase
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private string _costItemId = "Stone";
        [SerializeField] private int _costAmount = 1;
        [SerializeField] private string _resultItemId = "Arrow";
        [SerializeField] private int _resultAmount = 3;

        public string DisplayName { get => _displayName; set => _displayName = value ?? string.Empty; }
        public string CostItemId { get => _costItemId; set => _costItemId = value ?? string.Empty; }
        public int CostAmount { get => _costAmount; set => _costAmount = Mathf.Max(1, value); }
        public string ResultItemId { get => _resultItemId; set => _resultItemId = value ?? string.Empty; }
        public int ResultAmount { get => _resultAmount; set => _resultAmount = Mathf.Max(1, value); }
    }

    [System.Serializable]
    public sealed class CinderkeepCraftingCostData
    {
        [SerializeField] private string _itemId = "Stone";
        [SerializeField] private int _amount = 1;

        public string ItemId { get => _itemId; set => _itemId = value ?? string.Empty; }
        public int Amount { get => _amount; set => _amount = Mathf.Max(1, value); }
    }

    [System.Serializable]
    // Static Data: 건축/무기/장비/채집도구 제작 규칙.
    // TODO(팀 작업): 실제 제작 UI가 붙으면 category와 stationId를 기준으로 탭과 제작대를 분리해 주세요.
    public sealed class CinderkeepCraftingRecipeData : GameDataBase
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private string _category = "building";
        [SerializeField] private string _stationId = "field";
        [SerializeField] private string _resultItemId = string.Empty;
        [SerializeField] private int _resultAmount = 1;
        [SerializeField] private CinderkeepCraftingCostData[] _costs = new CinderkeepCraftingCostData[0];
        [SerializeField] private string _requiredToolId = string.Empty;
        [SerializeField] private int _unlockDay = 1;
        [SerializeField] private string _description = string.Empty;

        public string DisplayName { get => _displayName; set => _displayName = value ?? string.Empty; }
        public string Category { get => _category; set => _category = value ?? string.Empty; }
        public string StationId { get => _stationId; set => _stationId = value ?? string.Empty; }
        public string ResultItemId { get => _resultItemId; set => _resultItemId = value ?? string.Empty; }
        public int ResultAmount { get => _resultAmount; set => _resultAmount = Mathf.Max(1, value); }
        public CinderkeepCraftingCostData[] Costs { get => _costs; set => _costs = value ?? new CinderkeepCraftingCostData[0]; }
        public string RequiredToolId { get => _requiredToolId; set => _requiredToolId = value ?? string.Empty; }
        public int UnlockDay { get => _unlockDay; set => _unlockDay = Mathf.Max(1, value); }
        public string Description { get => _description; set => _description = value ?? string.Empty; }
    }

    [System.Serializable]
    // Static Data: Day/Night 진행 규칙.
    // GameFlowController는 순서만 지휘하고, 시간/스폰/보스 여부는 JSON 데이터가 결정한다.
    public sealed class CinderkeepDayRuleData : GameDataBase
    {
        [SerializeField] private int _dayIndex = 1;
        [SerializeField] private float _dayDuration = 180f;
        [SerializeField] private float _nightDuration = 120f;
        [SerializeField] private float _morningRewardDuration = 15f;
        [SerializeField] private float _bossNightDuration = 180f;
        [SerializeField] private int _nightSpawnMax = 8;
        [SerializeField] private int _bossSpawnMax = 12;
        [SerializeField] private float _spawnInterval = 4f;
        [SerializeField] private float _spawnRadius = 32f;
        [SerializeField] private string _enemyDataId = "plant";
        [SerializeField] private string _bossDataId = "boss";
        [SerializeField] private bool _isBossDay;
        [SerializeField] private string _dayObjective = "Gather resources and build fixed defense sites.";
        [SerializeField] private string _nightObjective = "Defend the FlameHeart.";

        public int DayIndex { get => _dayIndex; set => _dayIndex = Mathf.Max(1, value); }
        public float DayDuration { get => _dayDuration; set => _dayDuration = Mathf.Max(1f, value); }
        public float NightDuration { get => _nightDuration; set => _nightDuration = Mathf.Max(1f, value); }
        public float MorningRewardDuration { get => _morningRewardDuration; set => _morningRewardDuration = Mathf.Max(1f, value); }
        public float BossNightDuration { get => _bossNightDuration; set => _bossNightDuration = Mathf.Max(1f, value); }
        public int NightSpawnMax { get => _nightSpawnMax; set => _nightSpawnMax = Mathf.Max(0, value); }
        public int BossSpawnMax { get => _bossSpawnMax; set => _bossSpawnMax = Mathf.Max(0, value); }
        public float SpawnInterval { get => _spawnInterval; set => _spawnInterval = Mathf.Max(0.2f, value); }
        public float SpawnRadius { get => _spawnRadius; set => _spawnRadius = Mathf.Max(8f, value); }
        public string EnemyDataId { get => _enemyDataId; set => _enemyDataId = value ?? string.Empty; }
        public string BossDataId { get => _bossDataId; set => _bossDataId = value ?? string.Empty; }
        public bool IsBossDay { get => _isBossDay; set => _isBossDay = value; }
        public string DayObjective { get => _dayObjective; set => _dayObjective = value ?? string.Empty; }
        public string NightObjective { get => _nightObjective; set => _nightObjective = value ?? string.Empty; }
    }

    [System.Serializable]
    // Static Data: 적 스펙. Spawner가 id로 찾고 Enemy가 Configure에서 반영한다.
    public sealed class CinderkeepEnemyData : GameDataBase
    {
        [SerializeField] private string _displayName = "Enemy";
        [SerializeField] private int _health = 1;
        [SerializeField] private float _moveSpeed = 2.2f;
        [SerializeField] private float _stopDistance = 1.4f;
        [SerializeField] private int _attackDamage = 4;
        [SerializeField] private float _attackInterval = 1.2f;
        [SerializeField] private float _visualScale = 1f;

        public string DisplayName { get => _displayName; set => _displayName = value ?? string.Empty; }
        public int Health { get => _health; set => _health = Mathf.Max(1, value); }
        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = Mathf.Max(0.1f, value); }
        public float StopDistance { get => _stopDistance; set => _stopDistance = Mathf.Max(0.2f, value); }
        public int AttackDamage { get => _attackDamage; set => _attackDamage = Mathf.Max(1, value); }
        public float AttackInterval { get => _attackInterval; set => _attackInterval = Mathf.Max(0.1f, value); }
        public float VisualScale { get => _visualScale; set => _visualScale = Mathf.Max(0.1f, value); }
    }

    [System.Serializable]
    // Static Data: 성장 보상. 현재는 뼈대만 있고, 실제 선택 UI는 별도 구현 대상이다.
    public sealed class CinderkeepRelicData : GameDataBase
    {
        [SerializeField] private string _displayName = "Relic";
        [SerializeField] private string _description = string.Empty;
        [SerializeField] private float _value = 1f;

        public string DisplayName { get => _displayName; set => _displayName = value ?? string.Empty; }
        public string Description { get => _description; set => _description = value ?? string.Empty; }
        public float Value { get => _value; set => _value = value; }
    }

    [System.Serializable]
    // Static Data: FlameHeart 업그레이드. Beam/Aura 등 방어 능력 확장용이다.
    public sealed class CinderkeepFlameUpgradeData : GameDataBase
    {
        [SerializeField] private string _displayName = "Upgrade";
        [SerializeField] private string _description = string.Empty;
        [SerializeField] private float _value = 1f;

        public string DisplayName { get => _displayName; set => _displayName = value ?? string.Empty; }
        public string Description { get => _description; set => _description = value ?? string.Empty; }
        public float Value { get => _value; set => _value = value; }
    }

    [System.Serializable]
    // Static Data: 고정 건설 지점에서 만들 수 있는 구조물 정보.
    public sealed class CinderkeepBuildingData : GameDataBase
    {
        [SerializeField] private string _displayName = "Building";
        [SerializeField] private string _costItemId = "Stone";
        [SerializeField] private int _costAmount = 2;
        [SerializeField] private int _maxHealth = 20;

        public string DisplayName { get => _displayName; set => _displayName = value ?? string.Empty; }
        public string CostItemId { get => _costItemId; set => _costItemId = value ?? string.Empty; }
        public int CostAmount { get => _costAmount; set => _costAmount = Mathf.Max(1, value); }
        public int MaxHealth { get => _maxHealth; set => _maxHealth = Mathf.Max(1, value); }
    }
}
