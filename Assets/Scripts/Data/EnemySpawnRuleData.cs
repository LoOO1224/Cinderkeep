using System;
using System.Collections.Generic;
using UnityEngine;

// JSON에서 읽어오는 정적 데이터 구조입니다.
// 이 파일에는 규칙 로직을 넣지 않고, 실제 처리는 Systems/Managers/Catalog 쪽에서 담당합니다.
namespace Cinderkeep.Gameplay
{
    // EnemySpawnRule JSON 한 줄을 읽는 데이터 클래스입니다.
    // 낮/밤/일차별 몬스터 스폰 주기와 최대 생존 수를 코드 밖에서 조절하기 위한 데이터입니다.
    [Serializable]
    public sealed class EnemySpawnRuleData : GameDataBase
    {
        [SerializeField] private int _day;
        [SerializeField] private string _phase;
        [SerializeField] private string _enemyId;
        [SerializeField] private float _spawnInterval;
        [SerializeField] private int _minSpawnCount;
        [SerializeField] private int _maxSpawnCount;
        [SerializeField] private int _maxAliveCount;
        [SerializeField] private int _spawnWeight;
        [SerializeField] private float _spawnDistanceMin;
        [SerializeField] private float _spawnDistanceMax;

        public int Day
        {
            get
            {
                return _day;
            }
        }

        public string Phase
        {
            get
            {
                return _phase;
            }
        }

        public string EnemyId
        {
            get
            {
                return _enemyId;
            }
        }

        public float SpawnInterval
        {
            get
            {
                return _spawnInterval;
            }
        }

        public int MinSpawnCount
        {
            get
            {
                return _minSpawnCount;
            }
        }

        public int MaxSpawnCount
        {
            get
            {
                return _maxSpawnCount;
            }
        }

        public int MaxAliveCount
        {
            get
            {
                return _maxAliveCount;
            }
        }

        public int SpawnWeight
        {
            get
            {
                return _spawnWeight;
            }
        }

        public float SpawnDistanceMin
        {
            get
            {
                return _spawnDistanceMin;
            }
        }

        public float SpawnDistanceMax
        {
            get
            {
                return _spawnDistanceMax;
            }
        }
    }

    // JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class EnemySpawnRuleDataCatalog
    {
        public List<EnemySpawnRuleData> Items = new List<EnemySpawnRuleData>();
    }
}
