using System.Collections.Generic;
using System.Text;
using Cinderkeep.Gameplay;
using UnityEngine;
using UnityEngine.UI;

// Clear/GameOver 이후 한 판의 기록을 보여주는 결과창입니다.
// 표시 순서는 run_result_stats.json을 따르고, 실제 집계값은 RunResultTracker에서 가져옵니다.
namespace Cinderkeep.Gameplay
{
    public sealed class RunResultUI : MonoBehaviour
    {
        private const string DefaultStatDefinitionResourcePath = "Cinderkeep/data/run_result_stats";

        [Header("View")]
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _resultText;

        [Header("Data")]
        [SerializeField] private string _statDefinitionResourcePath = DefaultStatDefinitionResourcePath;

        private readonly List<RunResultStatData> _statDefinitions = new List<RunResultStatData>();
        private bool _isStatDefinitionLoaded;

        public static bool IsSupportedStatKey(string statKey)
        {
            switch (statKey)
            {
                case "result_status":
                case "reached_day":
                case "survival_time":
                case "failure_reason":
                case "monster_kills":
                case "boss_defeated":
                case "wood_gained":
                case "stone_gained":
                case "iron_gained":
                case "gold_gained":
                case "mithril_gained":
                case "adamantium_gained":
                case "crafted_item_count":
                case "placed_building_count":
                case "destroyed_building_count":
                case "upgraded_building_count":
                case "cinderheart_damage_taken":
                case "player_damage_taken":
                case "enemy_damage_dealt":
                case "tower_damage_dealt":
                case "trap_damage_dealt":
                case "player_down_count":
                case "trap_crowd_control_score":
                case "raw_meat_picked_up":
                case "cooked_meat_created":
                case "food_eaten_count":
                case "satiety_restored":
                case "selected_cinderheart_skills":
                    return true;
            }

            return false;
        }

        public void Open(bool isClear)
        {
            ConnectView();
            LoadStatDefinitionsIfNeeded();
            SetActive(_root, true);
            RefreshText(isClear);
        }

        public void Close()
        {
            ConnectView();
            SetActive(_root, false);
        }

        private void ConnectView()
        {
            if (_root == null)
            {
                _root = gameObject;
            }

            if (_resultText == null)
            {
                _resultText = GetComponentInChildren<Text>(true);
            }
        }

        private void LoadStatDefinitionsIfNeeded()
        {
            if (_isStatDefinitionLoaded)
            {
                return;
            }

            _statDefinitions.Clear();
            TextAsset textAsset = Resources.Load<TextAsset>(_statDefinitionResourcePath);
            if (textAsset != null)
            {
                RunResultStatDataCatalog catalog = JsonUtility.FromJson<RunResultStatDataCatalog>(textAsset.text);
                if (catalog != null && catalog.Items != null)
                {
                    _statDefinitions.AddRange(catalog.Items);
                }
            }

            if (_statDefinitions.Count <= 0)
            {
                AddFallbackStatDefinitions();
            }

            _statDefinitions.Sort(CompareStatDefinition);
            _isStatDefinitionLoaded = true;
        }

        private int CompareStatDefinition(RunResultStatData left, RunResultStatData right)
        {
            int leftOrder = left == null ? 0 : left.SortOrder;
            int rightOrder = right == null ? 0 : right.SortOrder;
            return leftOrder.CompareTo(rightOrder);
        }

        private void AddFallbackStatDefinitions()
        {
            AddFallbackStat("result_status", "Result", "Status", 10);
            AddFallbackStat("reached_day", "Result", "Reached Day", 20);
            AddFallbackStat("survival_time", "Result", "Survival Time", 30);
            AddFallbackStat("failure_reason", "Result", "Failure Reason", 40);
            AddFallbackStat("monster_kills", "Combat", "Monster Kills", 100);
            AddFallbackStat("boss_defeated", "Combat", "Boss Defeated", 110);
            AddFallbackStat("enemy_damage_dealt", "Combat", "Player Damage Dealt", 120);
            AddFallbackStat("tower_damage_dealt", "Combat", "Tower Damage Dealt", 130);
            AddFallbackStat("trap_damage_dealt", "Combat", "Trap Damage Dealt", 140);
            AddFallbackStat("player_damage_taken", "Defense", "Player Damage Taken", 200);
            AddFallbackStat("player_down_count", "Defense", "Player Downs", 210);
            AddFallbackStat("cinderheart_damage_taken", "Defense", "CinderHeart Damage Taken", 220);
            AddFallbackStat("wood_gained", "Resources", "Wood", 300);
            AddFallbackStat("stone_gained", "Resources", "Stone", 310);
            AddFallbackStat("iron_gained", "Resources", "Iron", 320);
            AddFallbackStat("gold_gained", "Resources", "Gold", 330);
            AddFallbackStat("mithril_gained", "Resources", "Mithril", 340);
            AddFallbackStat("adamantium_gained", "Resources", "Adamantium", 350);
            AddFallbackStat("crafted_item_count", "Crafting / Building", "Crafted Items", 400);
            AddFallbackStat("placed_building_count", "Crafting / Building", "Placed Buildings", 410);
            AddFallbackStat("destroyed_building_count", "Crafting / Building", "Destroyed Buildings", 420);
            AddFallbackStat("upgraded_building_count", "Crafting / Building", "Upgraded Buildings", 430);
            AddFallbackStat("trap_crowd_control_score", "Crafting / Building", "Trap CC Score", 440);
            AddFallbackStat("raw_meat_picked_up", "Food", "Raw Meat Picked Up", 500);
            AddFallbackStat("cooked_meat_created", "Food", "Cooked Meat Created", 510);
            AddFallbackStat("food_eaten_count", "Food", "Food Eaten", 520);
            AddFallbackStat("satiety_restored", "Food", "Satiety Restored", 530);
            AddFallbackStat("selected_cinderheart_skills", "CinderHeart", "Selected Upgrades", 600);
        }

        private void AddFallbackStat(string statKey, string group, string label, int sortOrder)
        {
            RunResultStatData statData = new RunResultStatData();
            typeof(GameDataBase).GetField("_id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(statData, statKey);
            typeof(RunResultStatData).GetField("_group", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(statData, group);
            typeof(RunResultStatData).GetField("_label", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(statData, label);
            typeof(RunResultStatData).GetField("_statKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(statData, statKey);
            typeof(RunResultStatData).GetField("_sortOrder", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(statData, sortOrder);
            _statDefinitions.Add(statData);
        }

        private void RefreshText(bool isClear)
        {
            if (_resultText == null)
            {
                return;
            }

            GameRunModel gameRunModel = GameManager.Inst == null ? null : GameManager.Inst.GameRunModel;
            RunResultTracker tracker = RunResultTracker.Instance;
            RunResultSnapshot snapshot = tracker == null
                ? RunResultTextFormatter.CreateFallbackSnapshot(isClear, gameRunModel)
                : tracker.CreateSnapshot(isClear, gameRunModel);

            _resultText.text = BuildRunResultText(snapshot);
        }

        private string BuildRunResultText(RunResultSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            string previousGroup = null;
            for (int i = 0; i < _statDefinitions.Count; i++)
            {
                RunResultStatData statData = _statDefinitions[i];
                if (statData == null)
                {
                    continue;
                }

                if (string.Equals(previousGroup, statData.Group, System.StringComparison.OrdinalIgnoreCase) == false)
                {
                    AppendGroupHeader(builder, statData.Group);
                    previousGroup = statData.Group;
                }

                builder.AppendLine(statData.Label + ": " + GetSafeStatValue(snapshot, statData.StatKey));
            }

            builder.AppendLine();
            builder.AppendLine("R: Restart");
            builder.AppendLine("Esc: Main_Lobby");
            return builder.ToString();
        }

        private string GetSafeStatValue(RunResultSnapshot snapshot, string statKey)
        {
            string statValue = GetStatValue(snapshot, statKey);
            if (ContainsBrokenDisplayText(statValue))
            {
                return "미집계";
            }

            return statValue;
        }

        private bool ContainsBrokenDisplayText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return text.Contains("誘") || text.Contains("�");
        }

        private void AppendGroupHeader(StringBuilder builder, string group)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine(string.IsNullOrEmpty(group) ? "Result" : group);
        }

        private string GetStatValue(RunResultSnapshot snapshot, string statKey)
        {
            switch (statKey)
            {
                case "result_status":
                    return snapshot.IsClear ? "CLEAR" : "GAME OVER";
                case "reached_day":
                    return snapshot.ReachedDay.ToString();
                case "survival_time":
                    return FormatTime(snapshot.SurvivalSeconds);
                case "failure_reason":
                    return snapshot.FailureReason;
                case "monster_kills":
                    return snapshot.MonsterKillCount.ToString();
                case "boss_defeated":
                    return FormatBool(snapshot.BossDefeated);
                case "wood_gained":
                    return snapshot.WoodGained.ToString();
                case "stone_gained":
                    return snapshot.StoneGained.ToString();
                case "iron_gained":
                    return snapshot.IronGained.ToString();
                case "gold_gained":
                    return snapshot.GoldGained.ToString();
                case "mithril_gained":
                    return snapshot.MithrilGained.ToString();
                case "adamantium_gained":
                    return snapshot.AdamantiumGained.ToString();
                case "crafted_item_count":
                    return snapshot.CraftedItemCount.ToString();
                case "placed_building_count":
                    return snapshot.PlacedBuildingCount.ToString();
                case "destroyed_building_count":
                    return snapshot.DestroyedBuildingCount.ToString();
                case "upgraded_building_count":
                    return snapshot.UpgradedBuildingCount.ToString();
                case "cinderheart_damage_taken":
                    return FormatNumber(snapshot.CinderHeartDamageTaken);
                case "player_damage_taken":
                    return FormatNumber(snapshot.PlayerDamageTaken);
                case "enemy_damage_dealt":
                    return FormatNumber(snapshot.EnemyDamageDealt);
                case "tower_damage_dealt":
                    return FormatNumber(snapshot.TowerDamageDealt);
                case "trap_damage_dealt":
                    return FormatNumber(snapshot.TrapDamageDealt);
                case "player_down_count":
                    return snapshot.PlayerDownCount.ToString();
                case "trap_crowd_control_score":
                    return FormatNumber(snapshot.TrapCrowdControlScore);
                case "raw_meat_picked_up":
                    return snapshot.RawMeatPickedUpCount.ToString();
                case "cooked_meat_created":
                    return snapshot.CookedMeatCreatedCount.ToString();
                case "food_eaten_count":
                    return snapshot.FoodEatenCount.ToString();
                case "satiety_restored":
                    return FormatNumber(snapshot.SatietyRestored);
                case "selected_cinderheart_skills":
                    return FormatSelectedSkills(snapshot);
            }

            return "미집계";
        }

        private string FormatSelectedSkills(RunResultSnapshot snapshot)
        {
            if (snapshot.SelectedCinderHeartSkillNames == null || snapshot.SelectedCinderHeartSkillNames.Count <= 0)
            {
                return "None";
            }

            return string.Join(", ", snapshot.SelectedCinderHeartSkillNames);
        }

        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainSeconds = totalSeconds % 60;
            return minutes.ToString("00") + ":" + remainSeconds.ToString("00");
        }

        private string FormatNumber(float value)
        {
            return Mathf.RoundToInt(value).ToString();
        }

        private string FormatBool(bool value)
        {
            return value ? "Yes" : "No";
        }

        private void SetActive(GameObject targetObject, bool isActive)
        {
            if (targetObject == null)
            {
                return;
            }

            targetObject.SetActive(isActive);
        }
    }
}
