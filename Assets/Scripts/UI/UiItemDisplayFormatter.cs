using System.Globalization;
using System.Text;

namespace Cinderkeep.Gameplay
{
    // UI 슬롯이 같은 기준으로 아이템 표시명을 가져오게 하는 표시 전용 유틸입니다.
    // 데이터가 아직 로드되지 않았으면 item_id를 읽기 쉬운 fallback 문구로 바꿉니다.
    public static class UiItemDisplayFormatter
    {
        public static string GetItemName(InventoryItemModel itemModel)
        {
            if (itemModel == null || itemModel.IsEmpty)
            {
                return "Empty";
            }

            return GetItemName(itemModel.ItemId, itemModel.ItemType);
        }

        public static string GetItemName(string itemId, InventoryItemType itemType)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return "Empty";
            }

            string dataDisplayName = TryGetDataDisplayName(itemId, itemType);
            if (string.IsNullOrEmpty(dataDisplayName) == false)
            {
                return dataDisplayName;
            }

            if (itemType == InventoryItemType.Food)
            {
                return GetFoodFallbackName(itemId);
            }

            return HumanizeId(itemId);
        }

        public static string BuildItemStackText(InventoryItemModel itemModel, bool showAmount)
        {
            if (itemModel == null || itemModel.IsEmpty)
            {
                return string.Empty;
            }

            string itemName = GetItemName(itemModel);
            if (showAmount == false || itemModel.Amount <= 1)
            {
                return itemName;
            }

            return itemName + "\nx" + itemModel.Amount.ToString(CultureInfo.InvariantCulture);
        }

        public static string GetEquipmentSlotLabel(EquipmentSlotType slotType)
        {
            if (slotType == EquipmentSlotType.Helmet)
            {
                return "Helmet";
            }

            if (slotType == EquipmentSlotType.Armor)
            {
                return "Armor";
            }

            if (slotType == EquipmentSlotType.Weapon)
            {
                return "Weapon";
            }

            if (slotType == EquipmentSlotType.Boots)
            {
                return "Boots";
            }

            return "Slot";
        }

        public static string HumanizeId(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string[] parts = value.Replace('-', '_').Split('_');
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                {
                    builder.Append(part.Substring(1));
                }
            }

            return builder.Length > 0 ? builder.ToString() : value;
        }

        private static string TryGetDataDisplayName(string itemId, InventoryItemType itemType)
        {
            GameDataManager gameDataManager = GetGameDataManager();
            if (gameDataManager == null)
            {
                return string.Empty;
            }

            if (itemType == InventoryItemType.Tool)
            {
                ToolData data = gameDataManager.GetTool(itemId);
                return data == null ? string.Empty : data.DisplayName;
            }

            if (itemType == InventoryItemType.Weapon)
            {
                WeaponData data = gameDataManager.GetWeapon(itemId);
                return data == null ? string.Empty : data.DisplayName;
            }

            if (itemType == InventoryItemType.Helmet
                || itemType == InventoryItemType.Armor
                || itemType == InventoryItemType.Boots)
            {
                ArmorData data = gameDataManager.GetArmor(itemId);
                return data == null ? string.Empty : data.DisplayName;
            }

            if (itemType == InventoryItemType.Resource)
            {
                ResourceData data = gameDataManager.GetResource(itemId);
                return data == null ? string.Empty : data.DisplayName;
            }

            if (itemType == InventoryItemType.Building)
            {
                BuildingData data = gameDataManager.GetBuilding(itemId);
                return data == null ? string.Empty : data.DisplayName;
            }

            if (itemType == InventoryItemType.CinderHeartUpgrade)
            {
                CinderHeartUpgradeData data = gameDataManager.GetCinderHeartUpgrade(itemId);
                return data == null ? string.Empty : data.DisplayName;
            }

            return string.Empty;
        }

        private static string GetFoodFallbackName(string itemId)
        {
            if (itemId == FoodItemIds.RawMeat)
            {
                return "Raw Meat";
            }

            if (itemId == FoodItemIds.CookedMeat)
            {
                return "Cooked Meat";
            }

            return HumanizeId(itemId);
        }

        private static GameDataManager GetGameDataManager()
        {
            if (GameManager.Inst == null)
            {
                return null;
            }

            return GameManager.Inst.GetGameDataManager();
        }
    }
}
