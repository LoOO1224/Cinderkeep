using System.Text;
using UnityEngine;

// Run Result 스냅샷을 화면에 표시할 텍스트로 변환하는 전용 포맷터입니다.
// UIManager가 결과창 전환에 집중하도록 기록 표시 규칙을 이곳에 모읍니다.
namespace Cinderkeep.Gameplay
{
    public static class RunResultTextFormatter
    {
        public static RunResultSnapshot CreateFallbackSnapshot(bool isClear, GameRunModel gameRunModel)
        {
            RunResultSnapshot snapshot = new RunResultSnapshot();
            snapshot.IsClear = isClear;
            snapshot.ReachedDay = gameRunModel == null ? 0 : gameRunModel.Day;
            snapshot.FailureReason = isClear ? "Clear" : "CinderHeart destroyed";
            return snapshot;
        }

        public static string BuildText(RunResultSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(snapshot.IsClear ? "CLEAR" : "GAME OVER");
            builder.AppendLine(snapshot.IsClear ? "CinderHeart defended." : "CinderHeart destroyed.");
            builder.AppendLine();
            builder.AppendLine("Result");
            builder.AppendLine("Reached Day: " + snapshot.ReachedDay);
            builder.AppendLine("Survival Time: " + FormatTime(snapshot.SurvivalSeconds));
            builder.AppendLine("Failure Reason: " + snapshot.FailureReason);
            builder.AppendLine();
            builder.AppendLine("Combat");
            builder.AppendLine("Monster Kills: " + snapshot.MonsterKillCount);
            builder.AppendLine("Boss Defeated: " + FormatBool(snapshot.BossDefeated));
            builder.AppendLine("Damage Dealt: " + FormatNumber(snapshot.EnemyDamageDealt));
            builder.AppendLine("Tower Damage Dealt: " + FormatNumber(snapshot.TowerDamageDealt));
            builder.AppendLine("Trap Damage Dealt: " + FormatNumber(snapshot.TrapDamageDealt));
            builder.AppendLine("Player Damage Taken: " + FormatNumber(snapshot.PlayerDamageTaken));
            builder.AppendLine("Player Downs: " + snapshot.PlayerDownCount);
            builder.AppendLine("CinderHeart Damage Taken: " + FormatNumber(snapshot.CinderHeartDamageTaken));
            builder.AppendLine();
            builder.AppendLine("Resources");
            builder.AppendLine("Wood: " + snapshot.WoodGained + " / Stone: " + snapshot.StoneGained);
            builder.AppendLine("Iron: " + snapshot.IronGained + " / Gold: " + snapshot.GoldGained);
            builder.AppendLine("Mithril: " + snapshot.MithrilGained + " / Adamantium: " + snapshot.AdamantiumGained);
            builder.AppendLine();
            builder.AppendLine("Crafting / Building");
            builder.AppendLine("Crafted Items: " + snapshot.CraftedItemCount);
            builder.AppendLine("Placed Buildings: " + snapshot.PlacedBuildingCount);
            builder.AppendLine("Destroyed Buildings: " + snapshot.DestroyedBuildingCount);
            builder.AppendLine("Upgraded Buildings: " + snapshot.UpgradedBuildingCount);
            builder.AppendLine("Trap CC Score: " + FormatNumber(snapshot.TrapCrowdControlScore));
            builder.AppendLine();
            builder.AppendLine("Food");
            builder.AppendLine("Raw Meat Picked Up: " + snapshot.RawMeatPickedUpCount);
            builder.AppendLine("Cooked Meat Created: " + snapshot.CookedMeatCreatedCount);
            builder.AppendLine("Food Eaten: " + snapshot.FoodEatenCount);
            builder.AppendLine("Satiety Restored: " + FormatNumber(snapshot.SatietyRestored));
            builder.AppendLine();
            builder.AppendLine("CinderHeart Upgrades");
            builder.AppendLine(FormatSelectedSkills(snapshot));
            builder.AppendLine();
            builder.AppendLine("R: Restart");
            builder.AppendLine("Esc: Main_Lobby");
            return builder.ToString();
        }

        private static string FormatSelectedSkills(RunResultSnapshot snapshot)
        {
            if (snapshot.SelectedCinderHeartSkillNames == null || snapshot.SelectedCinderHeartSkillNames.Count <= 0)
            {
                return "None";
            }

            return string.Join(", ", snapshot.SelectedCinderHeartSkillNames);
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainSeconds = totalSeconds % 60;
            return minutes.ToString("00") + ":" + remainSeconds.ToString("00");
        }

        private static string FormatNumber(float value)
        {
            return Mathf.RoundToInt(value).ToString();
        }

        private static string FormatBool(bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}
