using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 변하지 않는 기획 데이터의 부모 클래스입니다.
    // 몬스터, 자원, 제작법처럼 JSON에서 읽는 Static Data는 이 클래스를 기준으로 확장합니다.
    [Serializable]
    public class GameDataBase
    {
        [SerializeField] private string _id;

        public string Id
        {
            get
            {
                return _id;
            }
        }
    }

    // Enemy JSON의 한 줄을 담는 데이터 클래스입니다.
    // 3일 현재 단계에서는 일반 몬스터의 체력, 이동, 공격 값을 먼저 사용합니다.
    // JSON 필드명은 _id, _displayName, _health처럼 변수명과 맞춥니다.
    [Serializable]
    public sealed class EnemyData : GameDataBase
    {
        [SerializeField] private string _displayName;
        [SerializeField] private float _health;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _stopDistance;
        [SerializeField] private float _attackDamage;
        [SerializeField] private float _attackInterval;
        [SerializeField] private float _visualScale;
        [SerializeField] private float _detectorDistance;

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

        public float StopDistance
        {
            get
            {
                return _stopDistance;
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

        public float VisualScale
        {
            get
            {
                return _visualScale;
            }
        }

        public float DetectorDistance
        {
            get
            {
                return _detectorDistance;
            }
        }
    }

    // JsonUtility는 배열만 있는 JSON을 바로 읽기 어렵습니다.
    // 그래서 JSON 루트는 대문자 Items 필드를 가진 감싸는 클래스로 둡니다.
    [Serializable]
    public sealed class EnemyDataCatalog
    {
        public List<EnemyData> Items = new List<EnemyData>();
    }
}
