using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // armors.json의 방어구 한 줄을 읽는 정적 데이터입니다.
    // 실제 장착 효과는 PlayerEquipmentStatApplier가 PlayerStatus와 PlayerMovement에 적용합니다.
    [Serializable]
    public sealed class ArmorData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _armorSlot;
        [SerializeField] private int _tier;
        [SerializeField] private string _description;
        [SerializeField] private float _defense;
        [SerializeField] private float _maxHealthBonus;
        [SerializeField] private float _staminaBonus;
        [SerializeField] private float _moveSpeedBonus;
        [SerializeField] private string _prefabKey;
        [SerializeField] private string _craftingRecipeId;

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string ArmorSlot
        {
            get { return _armorSlot; }
        }

        public int Tier
        {
            get { return _tier; }
        }

        public string Description
        {
            get { return _description; }
        }

        public float Defense
        {
            get { return _defense; }
        }

        public float MaxHealthBonus
        {
            get { return _maxHealthBonus; }
        }

        public float StaminaBonus
        {
            get { return _staminaBonus; }
        }

        public float MoveSpeedBonus
        {
            get { return _moveSpeedBonus; }
        }

        public string PrefabKey
        {
            get { return _prefabKey; }
        }

        public string CraftingRecipeId
        {
            get { return _craftingRecipeId; }
        }
    }

    // JsonUtility가 읽을 수 있도록 Items 배열로 감싼 방어구 카탈로그입니다.
    [Serializable]
    public sealed class ArmorDataCatalog
    {
        public List<ArmorData> Items = new List<ArmorData>();
    }
}
