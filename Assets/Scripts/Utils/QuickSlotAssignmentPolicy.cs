// 퀵슬롯 자동 배치 우선순위를 정의합니다.
// UI와 픽업 로직이 같은 슬롯 정책을 쓰도록 선호 슬롯과 대체 슬롯 기준을 한 곳에 둡니다.
namespace Cinderkeep.Gameplay
{
    public static class QuickSlotAssignmentPolicy
    {
        public const int WeaponPreferredSlotIndex = 0;
        public const int AxePreferredSlotIndex = 1;
        public const int PickaxePreferredSlotIndex = 2;
        public const int FoodPreferredSlotIndex = 3;
        public const int FreePreferredSlotIndex = 4;

        public static int ReplacementSlotIndex
        {
            get
            {
                return PlayerInventoryModel.QuickSlotCount - 1;
            }
        }

        public static int GetPreferredSlotIndex(InventoryItemModel itemModel)
        {
            if (itemModel == null || itemModel.IsEmpty)
            {
                return FreePreferredSlotIndex;
            }

            return GetPreferredSlotIndex(itemModel.ItemId, itemModel.ItemType);
        }

        public static int GetPreferredSlotIndex(string itemId, InventoryItemType itemType)
        {
            if (itemType == InventoryItemType.Weapon)
            {
                return WeaponPreferredSlotIndex;
            }

            if (itemType == InventoryItemType.Food)
            {
                return FoodPreferredSlotIndex;
            }

            if (itemType == InventoryItemType.Tool)
            {
                return GetToolPreferredSlotIndex(itemId);
            }

            return FreePreferredSlotIndex;
        }

        private static int GetToolPreferredSlotIndex(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return AxePreferredSlotIndex;
            }

            if (itemId == global::PlayerToolController.HandStoneToolDataId)
            {
                return WeaponPreferredSlotIndex;
            }

            if (itemId.IndexOf("pickaxe", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PickaxePreferredSlotIndex;
            }

            if (itemId.IndexOf("axe", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return AxePreferredSlotIndex;
            }

            return FreePreferredSlotIndex;
        }
    }
}
