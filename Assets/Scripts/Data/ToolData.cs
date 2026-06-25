using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // tools.json의 한 줄을 담는 Static Data 클래스입니다.
    // 도끼와 곡괭이처럼 채집에 쓰이는 도구의 수치와 제작 연결 정보를 관리합니다.
    [Serializable]
    public sealed class ToolData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _toolType;
        [SerializeField] private int _tier;
        [SerializeField] private string _description;
        [SerializeField] private float _damage;
        [SerializeField] private float _attackDistance;
        [SerializeField] private float _attackRadius;
        [SerializeField] private float _attackInterval;
        [SerializeField] private int _gatherPower;
        [SerializeField] private float _woodGatherMultiplier;
        [SerializeField] private float _stoneGatherMultiplier;
        [SerializeField] private float _ironGatherMultiplier;
        [SerializeField] private float _goldGatherMultiplier;
        [SerializeField] private float _adamantiumGatherMultiplier;
        [SerializeField] private string _targetResourceType;
        [SerializeField] private string _prefabKey;
        [SerializeField] private string _craftingRecipeId;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string ToolType
        {
            get
            {
                return _toolType;
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

        public int GatherPower
        {
            get
            {
                return _gatherPower;
            }
        }

        public float WoodGatherMultiplier
        {
            get
            {
                return _woodGatherMultiplier;
            }
        }

        public float StoneGatherMultiplier
        {
            get
            {
                return _stoneGatherMultiplier;
            }
        }

        public float IronGatherMultiplier
        {
            get
            {
                return _ironGatherMultiplier;
            }
        }

        public float GoldGatherMultiplier
        {
            get
            {
                return _goldGatherMultiplier;
            }
        }

        public float AdamantiumGatherMultiplier
        {
            get
            {
                return _adamantiumGatherMultiplier;
            }
        }

        public string TargetResourceType
        {
            get
            {
                return _targetResourceType;
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

    // JsonUtility가 읽기 쉬운 Items 감싸기 구조입니다.
    [Serializable]
    public sealed class ToolDataCatalog
    {
        public List<ToolData> Items = new List<ToolData>();
    }
}
