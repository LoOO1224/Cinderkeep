using System;

// 한 판 플레이 중 변하는 런타임 상태를 저장합니다.
// 상태 변경은 명시적인 메서드로 처리하고, UI와 시스템은 이 모델을 읽거나 요청만 보냅니다.
namespace Cinderkeep.Gameplay
{
    public enum InventoryItemType
    {
        None,
        Tool,
        Weapon,
        Helmet,
        Armor,
        Boots,
        Resource,
        Food,
        Building,
        CinderHeartUpgrade
    }

    public enum EquipmentSlotType
    {
        Helmet,
        Armor,
        Weapon,
        Boots
    }

    // 인벤토리 한 칸에 들어가는 저장용 데이터입니다.
    // 실제 아이템 수치와 이름은 이후 JSON 데이터와 연결하고, 이 모델은 선택된 아이템의 상태만 저장합니다.
    [Serializable]
    public sealed class InventoryItemModel
    {
        private string _itemId;
        private InventoryItemType _itemType;
        private int _amount;

        public string ItemId
        {
            get
            {
                return _itemId;
            }
        }

        public InventoryItemType ItemType
        {
            get
            {
                return _itemType;
            }
        }

        public int Amount
        {
            get
            {
                return _amount;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(_itemId) || _amount <= 0;
            }
        }

        public void SetItem(string itemId, InventoryItemType itemType, int amount)
        {
            _itemId = itemId;
            _itemType = itemType;
            _amount = amount;
        }

        public void Clear()
        {
            _itemId = string.Empty;
            _itemType = InventoryItemType.None;
            _amount = 0;
        }

        public InventoryItemModel Clone()
        {
            InventoryItemModel itemModel = new InventoryItemModel();
            itemModel.SetItem(_itemId, _itemType, _amount);
            return itemModel;
        }
    }
}
