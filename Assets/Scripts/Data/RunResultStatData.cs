using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // Run Result 화면에 표시할 항목 순서와 라벨을 정의하는 데이터입니다.
    // 실제 값 집계는 RunResultTracker가 담당하고, 이 데이터는 표시 방식만 관리합니다.
    [Serializable]
    public sealed class RunResultStatData : GameDataBase
    {
        [SerializeField] private string _group;
        [SerializeField] private string _label;
        [SerializeField] private string _statKey;
        [SerializeField] private int _sortOrder;

        public string Group
        {
            get
            {
                return _group;
            }
        }

        public string Label
        {
            get
            {
                return _label;
            }
        }

        public string StatKey
        {
            get
            {
                return _statKey;
            }
        }

        public int SortOrder
        {
            get
            {
                return _sortOrder;
            }
        }
    }

    [Serializable]
    public sealed class RunResultStatDataCatalog
    {
        public List<RunResultStatData> Items = new List<RunResultStatData>();
    }
}
