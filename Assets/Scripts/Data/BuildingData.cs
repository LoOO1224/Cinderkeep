using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // Building JSON의 한 줄을 담는 데이터 클래스입니다.
    // 벽과 타워처럼 설치되는 건축물의 체력, 공격, 표시 키를 데이터로 관리합니다.
    [Serializable]
    public sealed class BuildingData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _buildingType;
        [SerializeField] private int _tier;
        [SerializeField] private string _description;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _defense;
        [SerializeField] private float _attackDamage;
        [SerializeField] private float _attackRange;
        [SerializeField] private float _attackInterval;
        [SerializeField] private string _prefabKey;
        [SerializeField] private string _materialKey;
        [SerializeField] private string _craftingRecipeId;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string BuildingType
        {
            get
            {
                return _buildingType;
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

        public float MaxHealth
        {
            get
            {
                return _maxHealth;
            }
        }

        public float Defense
        {
            get
            {
                return _defense;
            }
        }

        public float AttackDamage
        {
            get
            {
                return _attackDamage;
            }
        }

        public float AttackRange
        {
            get
            {
                return _attackRange;
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

        public string MaterialKey
        {
            get
            {
                return _materialKey;
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

    // BuildingData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class BuildingDataCatalog
    {
        public List<BuildingData> Items = new List<BuildingData>();
    }
}
