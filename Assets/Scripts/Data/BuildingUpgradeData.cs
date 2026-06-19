using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // BuildingUpgrade JSON의 한 줄을 담는 데이터 클래스입니다.
    // 기존 건축물을 다음 티어 건축물로 교체하거나 강화할 때 사용하는 데이터입니다.
    [Serializable]
    public sealed class BuildingUpgradeData : GameDataBase
    {
        [SerializeField] private string _fromBuildingId;
        [SerializeField] private string _toBuildingId;
        [SerializeField] private string _craftingRecipeId;
        [SerializeField] private int _requiredDay;
        [SerializeField] private string _description;

        public string FromBuildingId
        {
            get
            {
                return _fromBuildingId;
            }
        }

        public string ToBuildingId
        {
            get
            {
                return _toBuildingId;
            }
        }

        public string CraftingRecipeId
        {
            get
            {
                return _craftingRecipeId;
            }
        }

        public int RequiredDay
        {
            get
            {
                return _requiredDay;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }
    }

    // BuildingUpgradeData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class BuildingUpgradeDataCatalog
    {
        public List<BuildingUpgradeData> Items = new List<BuildingUpgradeData>();
    }
}
