using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 변하지 않는 기획 데이터의 부모 클래스입니다.
    // 예: 몬스터 기본 체력, 이동 속도, 공격력, 제작 레시피, 불꽃 강화 등.
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

    // JaeUk-MonsterAI 브랜치에서 만든 Enemy 데이터 필드를 현재 규칙에 맞춰 반영했습니다.
    // 3일 MVP 루프에서는 밤 웨이브와 보스 접근의 기초 스탯으로 사용합니다.
    // JSON 필드명은 _id, _displayName, _health 같은 이름을 그대로 사용합니다.
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
    }

    // JsonUtility는 배열만 있는 JSON을 바로 읽지 못해서 감싸는 클래스가 필요합니다.
    // JSON 루트가 "Items"라서 이 필드명은 대문자로 유지합니다.
    [Serializable]
    public sealed class EnemyDataCatalog
    {
        public List<EnemyData> Items = new List<EnemyData>();
    }
}
