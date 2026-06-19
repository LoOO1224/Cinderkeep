using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // BossPattern JSON의 한 줄을 담는 데이터 클래스입니다.
    // 보스 체력 구간별 패턴, 쿨타임, 피해량, 범위를 관리합니다.
    [Serializable]
    public sealed class BossPatternData : GameDataBase
    {
        [SerializeField] private string _bossId;
        [SerializeField] private string _patternType;
        [SerializeField] private float _triggerHealthRate;
        [SerializeField] private float _cooldown;
        [SerializeField] private float _damage;
        [SerializeField] private float _range;
        [SerializeField] private string _effectKey;

        public string BossId
        {
            get
            {
                return _bossId;
            }
        }

        public string PatternType
        {
            get
            {
                return _patternType;
            }
        }

        public float TriggerHealthRate
        {
            get
            {
                return _triggerHealthRate;
            }
        }

        public float Cooldown
        {
            get
            {
                return _cooldown;
            }
        }

        public float Damage
        {
            get
            {
                return _damage;
            }
        }

        public float Range
        {
            get
            {
                return _range;
            }
        }

        public string EffectKey
        {
            get
            {
                return _effectKey;
            }
        }
    }

    // BossPatternData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class BossPatternDataCatalog
    {
        public List<BossPatternData> Items = new List<BossPatternData>();
    }
}
