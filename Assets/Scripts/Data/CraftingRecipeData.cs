using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // 제작 재료 한 줄을 담는 데이터입니다.
    // 예: Wood 3개, Stone 2개처럼 제작법 내부에서 반복 사용합니다.
    [Serializable]
    public sealed class CraftingCostData
    {
        [SerializeField] private string _resourceId;
        [SerializeField] private int _amount;

        public string ResourceId
        {
            get
            {
                return _resourceId;
            }
        }

        public int Amount
        {
            get
            {
                return _amount;
            }
        }
    }

    // CraftingRecipe JSON의 한 줄을 담는 데이터 클래스입니다.
    // 실제 제작 실행은 이후 CraftingSystem에서 맡고, 이 클래스는 기획 값만 보관합니다.
    [Serializable]
    public sealed class CraftingRecipeData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _resultDataType;
        [SerializeField] private string _resultItemId;
        [SerializeField] private int _resultCount;
        [SerializeField] private string _stationType;
        [SerializeField] private int _requiredStationTier;
        [SerializeField] private int _unlockDay;
        [SerializeField] private List<CraftingCostData> _costs = new List<CraftingCostData>();

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string ResultDataType
        {
            get
            {
                return _resultDataType;
            }
        }

        public string ResultItemId
        {
            get
            {
                return _resultItemId;
            }
        }

        public int ResultCount
        {
            get
            {
                return _resultCount;
            }
        }

        public string StationType
        {
            get
            {
                return _stationType;
            }
        }

        public int RequiredStationTier
        {
            get
            {
                return _requiredStationTier;
            }
        }

        public int UnlockDay
        {
            get
            {
                return _unlockDay;
            }
        }

        public IReadOnlyList<CraftingCostData> Costs
        {
            get
            {
                return _costs;
            }
        }
    }

    // CraftingRecipeData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class CraftingRecipeDataCatalog
    {
        public List<CraftingRecipeData> Items = new List<CraftingRecipeData>();
    }
}
