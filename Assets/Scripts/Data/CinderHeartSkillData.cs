using System;
using System.Collections.Generic;
using UnityEngine;

// 5.00 direction: Defines static JSON-backed data for the 5.00 playable loop.
// 5.01+ note: Keep this file behavior-free; add gameplay logic in Systems or Managers and keep new fields data-driven.
namespace Cinderkeep.Gameplay
{
    // CinderHeart 아침 보상 선택지 한 줄을 담는 정적 데이터입니다.
    // 실제 효과 적용은 CinderHeartSkillApplier가 담당하고, 이 클래스는 기획 수치만 보관합니다.
    [Serializable]
    public sealed class CinderHeartSkillData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        [SerializeField] private string _effectType;
        [SerializeField] private float _value;
        [SerializeField] private int _requiredDay;
        [SerializeField] private int _weight;
        [SerializeField] private bool _canRepeat;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public string EffectType
        {
            get
            {
                return _effectType;
            }
        }

        public float Value
        {
            get
            {
                return _value;
            }
        }

        public int RequiredDay
        {
            get
            {
                return _requiredDay;
            }
        }

        public int Weight
        {
            get
            {
                return _weight;
            }
        }

        public bool CanRepeat
        {
            get
            {
                return _canRepeat;
            }
        }
    }

    // JsonUtility가 읽을 수 있도록 Items 배열을 감싸는 카탈로그입니다.
    [Serializable]
    public sealed class CinderHeartSkillDataCatalog
    {
        public List<CinderHeartSkillData> Items = new List<CinderHeartSkillData>();
    }
}
