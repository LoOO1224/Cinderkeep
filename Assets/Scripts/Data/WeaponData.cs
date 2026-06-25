using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // Weapon JSON의 한 줄을 담는 데이터 클래스입니다.
    // 현재 4.20 단계에서는 칼 계열부터 데이터로 분리합니다.
    [Serializable]
    public sealed class WeaponData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _weaponType;
        [SerializeField] private int _tier;
        [SerializeField] private string _description;
        [SerializeField] private float _damage;
        [SerializeField] private float _attackDistance;
        [SerializeField] private float _attackRadius;
        [SerializeField] private float _attackInterval;
        [SerializeField] private string _prefabKey;
        [SerializeField] private string _craftingRecipeId;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string WeaponType
        {
            get
            {
                return _weaponType;
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

        public float Damage
        {
            get
            {
                return _damage;
            }
        }

        public float AttackDistance
        {
            get
            {
                return _attackDistance;
            }
        }

        public float AttackRadius
        {
            get
            {
                return _attackRadius;
            }
        }

        public float AttackInterval
        {
            get
            {
                return _attackInterval;
            }
        }

        public string PrefabKey
        {
            get
            {
                return _prefabKey;
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

    // WeaponData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class WeaponDataCatalog
    {
        public List<WeaponData> Items = new List<WeaponData>();
    }
}
