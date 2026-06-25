namespace Cinderkeep.Gameplay
{
    // InventoryUI가 들고 있는 슬롯 배열을 현재 모델 상태로 갱신합니다.
    // 드래그, 더블클릭, 자동 장착 규칙은 InventoryUI에 남기고 화면 반영 반복문만 담당합니다.
    public static class InventorySlotRefreshPresenter
    {
        public static void RefreshEquipmentSlots(
            EquipmentSlotView[] slotViews,
            PlayerEquipmentModel playerEquipmentModel,
            InventoryUI owner)
        {
            if (slotViews == null || playerEquipmentModel == null)
            {
                return;
            }

            for (int i = 0; i < slotViews.Length; i++)
            {
                EquipmentSlotView slotView = slotViews[i];
                if (slotView == null)
                {
                    continue;
                }

                EquipmentSlotType slotType = slotView.SlotType;
                string equippedItemId = playerEquipmentModel.GetEquippedItemId(slotType);
                slotView.SetSlot(slotType, equippedItemId, owner);
            }
        }

        public static void RefreshInventorySlots(
            InventorySlotView[] slotViews,
            PlayerInventoryModel playerInventoryModel,
            InventoryUI owner)
        {
            if (slotViews == null || playerInventoryModel == null)
            {
                return;
            }

            for (int i = 0; i < slotViews.Length; i++)
            {
                InventorySlotView slotView = slotViews[i];
                if (slotView == null)
                {
                    continue;
                }

                InventoryItemModel itemModel = playerInventoryModel.GetInventoryItem(i);
                slotView.SetSlot(i, itemModel, owner);
            }
        }

        public static void RefreshQuickSlots(
            QuickSlotView[] slotViews,
            PlayerInventoryModel playerInventoryModel,
            InventoryUI owner)
        {
            if (slotViews == null || playerInventoryModel == null)
            {
                return;
            }

            for (int i = 0; i < slotViews.Length; i++)
            {
                QuickSlotView slotView = slotViews[i];
                if (slotView == null)
                {
                    continue;
                }

                InventoryItemModel itemModel = playerInventoryModel.GetQuickSlotItem(i);
                slotView.SetSlot(i, itemModel, owner);
            }
        }
    }
}
