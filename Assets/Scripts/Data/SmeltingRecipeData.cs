using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // smelting_recipes.json의 한 줄을 담는 Static Data입니다.
    // 용광로에서 어떤 광석을 어떤 주괴로 바꿀지 관리합니다.
    [Serializable]
    public sealed class SmeltingRecipeData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _inputResourceId;
        [SerializeField] private int _inputAmount;
        [SerializeField] private string _outputResourceId;
        [SerializeField] private int _outputAmount;
        [SerializeField] private float _smeltSeconds;
        [SerializeField] private string _stationType;
        [SerializeField] private int _requiredStationTier;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string InputResourceId
        {
            get
            {
                return _inputResourceId;
            }
        }

        public int InputAmount
        {
            get
            {
                return _inputAmount;
            }
        }

        public string OutputResourceId
        {
            get
            {
                return _outputResourceId;
            }
        }

        public int OutputAmount
        {
            get
            {
                return _outputAmount;
            }
        }

        public float SmeltSeconds
        {
            get
            {
                return _smeltSeconds;
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
    }

    // JsonUtility가 읽기 쉬운 Items 감싸기 구조입니다.
    [Serializable]
    public sealed class SmeltingRecipeDataCatalog
    {
        public List<SmeltingRecipeData> Items = new List<SmeltingRecipeData>();
    }
}
