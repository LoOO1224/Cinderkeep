using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // CinderHeartUpgrade JSON의 한 줄을 담는 데이터 클래스입니다.
    // CinderHeart의 체력, 방어력, 오라, 회복 같은 성장 요소를 관리합니다.
    [Serializable]
    public sealed class CinderHeartUpgradeData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private int _tier;
        [SerializeField] private string _description;
        [SerializeField] private int _requiredDay;
        [SerializeField] private float _maxHealthBonus;
        [SerializeField] private float _defenseBonus;
        [SerializeField] private float _auraRange;
        [SerializeField] private string _statusEffectId;
        [SerializeField] private string _craftingRecipeId;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public int Tier
        {
            get
            {
                return _tier;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public int RequiredDay
        {
            get
            {
                return _requiredDay;
            }
        }

        public float MaxHealthBonus
        {
            get
            {
                return _maxHealthBonus;
            }
        }

        public float DefenseBonus
        {
            get
            {
                return _defenseBonus;
            }
        }

        public float AuraRange
        {
            get
            {
                return _auraRange;
            }
        }

        public string StatusEffectId
        {
            get
            {
                return _statusEffectId;
            }
        }

        public string CraftingRecipeId
        {
            get
            {
                return _craftingRecipeId;
            }
        }
    }

    // CinderHeartUpgradeData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class CinderHeartUpgradeDataCatalog
    {
        public List<CinderHeartUpgradeData> Items = new List<CinderHeartUpgradeData>();
    }
}
