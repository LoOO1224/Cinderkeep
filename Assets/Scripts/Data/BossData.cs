using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // Boss JSON의 한 줄을 담는 데이터 클래스입니다.
    // 보스의 기본 체력, 이동, 공격 수치와 프리팹 키를 관리합니다.
    [Serializable]
    public sealed class BossData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private float _health;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _attackDamage;
        [SerializeField] private float _attackInterval;
        [SerializeField] private float _stopDistance;
        [SerializeField] private int _phaseCount;
        [SerializeField] private string _prefabKey;
        [SerializeField] private float _detectorDistance;
        [SerializeField] private float _detectorInterval;

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public float Health
        {
            get
            {
                return _health;
            }
        }

        public float MoveSpeed
        {
            get
            {
                return _moveSpeed;
            }
        }

        public float AttackDamage
        {
            get
            {
                return _attackDamage;
            }
        }

        public float AttackInterval
        {
            get
            {
                return _attackInterval;
            }
        }

        public float StopDistance
        {
            get
            {
                return _stopDistance;
            }
        }

        public int PhaseCount
        {
            get
            {
                return _phaseCount;
            }
        }

        public string PrefabKey
        {
            get
            {
                return _prefabKey;
            }
        }

        public float DetectorDistance
        {
            get
            {
                return _detectorDistance;
            }
        }

        public float DetectorInterval
        {
            get
            {
                return _detectorInterval;
            }
        }
    }

    // BossData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class BossDataCatalog
    {
        public List<BossData> Items = new List<BossData>();
    }
}
