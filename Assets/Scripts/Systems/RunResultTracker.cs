using System;
using System.Collections.Generic;
using UnityEngine;

// 한 판 동안 발생한 전투, 채집, 제작, 건축, 보상 선택 기록을 모아 Run Result에 전달합니다.
// 각 시스템의 이벤트를 구독해 집계만 담당하고, 실제 게임 규칙은 변경하지 않습니다.
namespace Cinderkeep.Gameplay
{
    public sealed class RunResultTracker : MonoBehaviour
    {
        private static RunResultTracker _instance;

        private readonly List<string> _selectedCinderHeartSkillNames = new List<string>();

        private float _runStartTime;
        private int _reachedDay;
        private int _monsterKillCount;
        private int _playerDownCount;
        private int _craftedItemCount;
        private int _placedBuildingCount;
        private int _woodGained;
        private int _stoneGained;
        private int _ironGained;
        private int _goldGained;
        private int _mithrilGained;
        private int _adamantiumGained;
        private float _cinderHeartDamageTaken;
        private float _playerDamageTaken;
        private float _enemyDamageDealt;
        private float _towerDamageDealt;
        private float _trapDamageDealt;
        private float _trapCrowdControlScore;
        private bool _bossDefeated;
        private bool _isTracking;

        public static RunResultTracker Instance
        {
            get { return _instance; }
        }

        public static RunResultTracker EnsureSceneTracker()
        {
            if (_instance != null)
            {
                return _instance;
            }

            RunResultTracker existingTracker = FindFirstObjectByType<RunResultTracker>();
            if (existingTracker != null)
            {
                return existingTracker;
            }

            GameObject trackerObject = new GameObject("RunResultTracker_5_00");
            return trackerObject.AddComponent<RunResultTracker>();
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                return;
            }

            if (_instance == this)
            {
                return;
            }

            Destroy(gameObject);
        }

        private void OnEnable()
        {
            PlayerModel.ResourceAddedGlobal += HandleResourceAdded;
            EnemyStatus.EnemyDiedGlobal += HandleEnemyDied;
            EnemyStatus.EnemyDamagedByAmountGlobal += HandleEnemyDamaged;
            PlayerStatus.PlayerDamagedGlobal += HandlePlayerDamaged;
            PlayerStatus.PlayerDiedGlobal += HandlePlayerDied;
            CinderHeart.CinderHeartDamagedGlobal += HandleCinderHeartDamaged;
            CraftingRecipeExecutor.RecipeCraftedGlobal += HandleRecipeCrafted;
            BuildingManager.BuildingPlacedGlobal += HandleBuildingPlaced;
            CinderHeartSkillSelectionUI.SkillSelectedGlobal += HandleCinderHeartSkillSelected;
            global::DamageDealer.DamageAppliedGlobal += HandleDamageApplied;
            global::TrapCrowdControlReporter.TrapCrowdControlScoredGlobal += HandleTrapCrowdControlScored;
        }

        private void OnDisable()
        {
            PlayerModel.ResourceAddedGlobal -= HandleResourceAdded;
            EnemyStatus.EnemyDiedGlobal -= HandleEnemyDied;
            EnemyStatus.EnemyDamagedByAmountGlobal -= HandleEnemyDamaged;
            PlayerStatus.PlayerDamagedGlobal -= HandlePlayerDamaged;
            PlayerStatus.PlayerDiedGlobal -= HandlePlayerDied;
            CinderHeart.CinderHeartDamagedGlobal -= HandleCinderHeartDamaged;
            CraftingRecipeExecutor.RecipeCraftedGlobal -= HandleRecipeCrafted;
            BuildingManager.BuildingPlacedGlobal -= HandleBuildingPlaced;
            CinderHeartSkillSelectionUI.SkillSelectedGlobal -= HandleCinderHeartSkillSelected;
            global::DamageDealer.DamageAppliedGlobal -= HandleDamageApplied;
            global::TrapCrowdControlReporter.TrapCrowdControlScoredGlobal -= HandleTrapCrowdControlScored;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void BeginRun(GameRunModel gameRunModel)
        {
            _runStartTime = Time.unscaledTime;
            _reachedDay = gameRunModel == null ? GameRunModel.FirstDay : gameRunModel.Day;
            _monsterKillCount = 0;
            _playerDownCount = 0;
            _craftedItemCount = 0;
            _placedBuildingCount = 0;
            _woodGained = 0;
            _stoneGained = 0;
            _ironGained = 0;
            _goldGained = 0;
            _mithrilGained = 0;
            _adamantiumGained = 0;
            _cinderHeartDamageTaken = 0f;
            _playerDamageTaken = 0f;
            _enemyDamageDealt = 0f;
            _towerDamageDealt = 0f;
            _trapDamageDealt = 0f;
            _trapCrowdControlScore = 0f;
            _bossDefeated = false;
            _selectedCinderHeartSkillNames.Clear();
            _isTracking = true;
        }

        public void RecordBossDefeated()
        {
            if (_isTracking == false)
            {
                return;
            }

            _bossDefeated = true;
        }

        public RunResultSnapshot CreateSnapshot(bool isClear, GameRunModel gameRunModel)
        {
            RefreshReachedDay(gameRunModel);

            RunResultSnapshot snapshot = new RunResultSnapshot();
            snapshot.IsClear = isClear;
            snapshot.ReachedDay = _reachedDay;
            snapshot.SurvivalSeconds = Mathf.Max(0f, Time.unscaledTime - _runStartTime);
            snapshot.FailureReason = GetFailureReason(isClear);
            snapshot.MonsterKillCount = _monsterKillCount;
            snapshot.BossDefeated = _bossDefeated;
            snapshot.WoodGained = _woodGained;
            snapshot.StoneGained = _stoneGained;
            snapshot.IronGained = _ironGained;
            snapshot.GoldGained = _goldGained;
            snapshot.MithrilGained = _mithrilGained;
            snapshot.AdamantiumGained = _adamantiumGained;
            snapshot.CraftedItemCount = _craftedItemCount;
            snapshot.PlacedBuildingCount = _placedBuildingCount;
            snapshot.CinderHeartDamageTaken = _cinderHeartDamageTaken;
            snapshot.PlayerDamageTaken = _playerDamageTaken;
            snapshot.EnemyDamageDealt = _enemyDamageDealt;
            snapshot.TowerDamageDealt = _towerDamageDealt;
            snapshot.TrapDamageDealt = _trapDamageDealt;
            snapshot.PlayerDownCount = _playerDownCount;
            snapshot.TrapCrowdControlScore = _trapCrowdControlScore;
            snapshot.SelectedCinderHeartSkillNames.AddRange(_selectedCinderHeartSkillNames);
            return snapshot;
        }

        private void HandleResourceAdded(string resourceId, int amount)
        {
            if (_isTracking == false || amount <= 0)
            {
                return;
            }

            if (IsResource(resourceId, PlayerModel.ResourceWood))
            {
                _woodGained += amount;
                return;
            }

            if (IsResource(resourceId, PlayerModel.ResourceStone))
            {
                _stoneGained += amount;
                return;
            }

            if (IsResource(resourceId, PlayerModel.ResourceIron) || IsResource(resourceId, PlayerModel.ResourceIronOre) || IsResource(resourceId, PlayerModel.ResourceIronIngot))
            {
                _ironGained += amount;
                return;
            }

            if (IsResource(resourceId, PlayerModel.ResourceGold) || IsResource(resourceId, PlayerModel.ResourceGoldOre) || IsResource(resourceId, PlayerModel.ResourceGoldIngot))
            {
                _goldGained += amount;
                return;
            }

            if (IsResource(resourceId, PlayerModel.ResourceMithril))
            {
                _mithrilGained += amount;
                return;
            }

            if (IsResource(resourceId, PlayerModel.ResourceAdamantium) || IsResource(resourceId, PlayerModel.ResourceAdamantiumOre) || IsResource(resourceId, PlayerModel.ResourceAdamantiumIngot))
            {
                _adamantiumGained += amount;
            }
        }

        private void HandleEnemyDied(EnemyStatus enemyStatus)
        {
            if (_isTracking == false)
            {
                return;
            }

            _monsterKillCount++;
        }

        private void HandleEnemyDamaged(float damage)
        {
            if (_isTracking == false || damage <= 0f)
            {
                return;
            }

            _enemyDamageDealt += damage;
        }

        private void HandleDamageApplied(global::DamageDealer damageDealer, global::Damageable damageable, float damage, global::DamageSourceType sourceType)
        {
            if (_isTracking == false || damage <= 0f)
            {
                return;
            }

            if (sourceType == global::DamageSourceType.Tower)
            {
                _towerDamageDealt += damage;
                return;
            }

            if (sourceType == global::DamageSourceType.Trap)
            {
                _trapDamageDealt += damage;
            }
        }

        private void HandleTrapCrowdControlScored(float score)
        {
            if (_isTracking == false || score <= 0f)
            {
                return;
            }

            _trapCrowdControlScore += score;
        }

        private void HandlePlayerDamaged(float damage)
        {
            if (_isTracking == false || damage <= 0f)
            {
                return;
            }

            _playerDamageTaken += damage;
        }

        private void HandlePlayerDied()
        {
            if (_isTracking == false)
            {
                return;
            }

            _playerDownCount++;
        }

        private void HandleCinderHeartDamaged(float damage)
        {
            if (_isTracking == false || damage <= 0f)
            {
                return;
            }

            _cinderHeartDamageTaken += damage;
        }

        private void HandleRecipeCrafted(CraftingRecipeData recipeData)
        {
            if (_isTracking == false || recipeData == null)
            {
                return;
            }

            _craftedItemCount++;
        }

        private void HandleBuildingPlaced(BuildingData buildingData)
        {
            if (_isTracking == false || buildingData == null)
            {
                return;
            }

            _placedBuildingCount++;
        }

        private void HandleCinderHeartSkillSelected(CinderHeartSkillData skillData)
        {
            if (_isTracking == false || skillData == null)
            {
                return;
            }

            string displayName = string.IsNullOrEmpty(skillData.DisplayName) ? skillData.Id : skillData.DisplayName;
            _selectedCinderHeartSkillNames.Add(displayName);
        }

        private void RefreshReachedDay(GameRunModel gameRunModel)
        {
            if (gameRunModel == null)
            {
                return;
            }

            _reachedDay = Mathf.Max(_reachedDay, gameRunModel.Day);
        }

        private string GetFailureReason(bool isClear)
        {
            if (isClear)
            {
                return "Clear";
            }

            return "CinderHeart destroyed";
        }

        private bool IsResource(string resourceId, string expectedResourceId)
        {
            return string.Equals(resourceId, expectedResourceId, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class RunResultSnapshot
    {
        public bool IsClear;
        public int ReachedDay;
        public float SurvivalSeconds;
        public string FailureReason;
        public int MonsterKillCount;
        public bool BossDefeated;
        public int WoodGained;
        public int StoneGained;
        public int IronGained;
        public int GoldGained;
        public int MithrilGained;
        public int AdamantiumGained;
        public int CraftedItemCount;
        public int PlacedBuildingCount;
        public float CinderHeartDamageTaken;
        public float PlayerDamageTaken;
        public float EnemyDamageDealt;
        public float TowerDamageDealt;
        public float TrapDamageDealt;
        public int PlayerDownCount;
        public float TrapCrowdControlScore;
        public readonly List<string> SelectedCinderHeartSkillNames = new List<string>();
    }
}
