using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // harvest_nodes.json의 한 줄을 담는 Static Data 클래스입니다.
    // 나무, 돌, 광석이 어떤 자원을 얼마나 주는지 관리합니다.
    [Serializable]
    public sealed class HarvestNodeData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _resourceId;
        [SerializeField] private string _requiredToolType;
        [SerializeField] private int _requiredToolTier;
        [SerializeField] private int _gatherAmount;
        [SerializeField] private int _maxGatherCount;
        [SerializeField] private float _respawnSeconds;
        [SerializeField] private string _prefabKey;
        [SerializeField] private string _materialKey;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string ResourceId
        {
            get
            {
                return _resourceId;
            }
        }

        public string RequiredToolType
        {
            get
            {
                return _requiredToolType;
            }
        }

        public int RequiredToolTier
        {
            get
            {
                return _requiredToolTier;
            }
        }

        public int GatherAmount
        {
            get
            {
                return _gatherAmount;
            }
        }

        public int MaxGatherCount
        {
            get
            {
                return _maxGatherCount;
            }
        }

        public float RespawnSeconds
        {
            get
            {
                return _respawnSeconds;
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
    }

    // JsonUtility가 읽기 쉬운 Items 감싸기 구조입니다.
    [Serializable]
    public sealed class HarvestNodeDataCatalog
    {
        public List<HarvestNodeData> Items = new List<HarvestNodeData>();
    }
}
