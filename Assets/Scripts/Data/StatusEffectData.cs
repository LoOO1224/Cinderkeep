using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // StatusEffect JSON의 한 줄을 담는 데이터 클래스입니다.
    // 추위, 오라, 이동속도, 회복 같은 버프와 디버프를 같은 형식으로 관리합니다.
    [Serializable]
    public sealed class StatusEffectData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _effectType;
        [SerializeField] private float _value;
        [SerializeField] private float _durationSeconds;
        [SerializeField] private bool _stackable;
        [SerializeField] private string _iconKey;
        [SerializeField] private string _description;

        public string DisplayName
        {
            get
            {
                return _displayName;
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

        public float DurationSeconds
        {
            get
            {
                return _durationSeconds;
            }
        }

        public bool Stackable
        {
            get
            {
                return _stackable;
            }
        }

        public string IconKey
        {
            get
            {
                return _iconKey;
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

    // StatusEffectData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class StatusEffectDataCatalog
    {
        public List<StatusEffectData> Items = new List<StatusEffectData>();
    }
}
