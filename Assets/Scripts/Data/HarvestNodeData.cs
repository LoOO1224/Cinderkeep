using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // HarvestNode JSON의 한 줄을 담는 데이터 클래스입니다.
    // 나무, 바위, 광석처럼 채집 대상이 어떤 자원을 주는지 관리합니다.
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

    // HarvestNodeData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class HarvestNodeDataCatalog
    {
        public List<HarvestNodeData> Items = new List<HarvestNodeData>();
    }
}
