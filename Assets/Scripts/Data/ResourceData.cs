using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // Resource JSON의 한 줄을 담는 데이터 클래스입니다.
    // Wood, Stone, Iron처럼 게임 전체에서 공유하는 자원 ID와 표시 정보를 관리합니다.
    [Serializable]
    public sealed class ResourceData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _resourceType;
        [SerializeField] private int _tier;
        [SerializeField] private string _description;
        [SerializeField] private string _iconKey;
        [SerializeField] private string _prefabKey;
        [SerializeField] private int _maxStackCount;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string ResourceType
        {
            get
            {
                return _resourceType;
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

        public string IconKey
        {
            get
            {
                return _iconKey;
            }
        }

        public string PrefabKey
        {
            get
            {
                return _prefabKey;
            }
        }

        public int MaxStackCount
        {
            get
            {
                return _maxStackCount;
            }
        }
    }

    // ResourceData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class ResourceDataCatalog
    {
        public List<ResourceData> Items = new List<ResourceData>();
    }
}
