using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // GameFlowPhase JSON의 한 줄을 담는 데이터 클래스입니다.
    // 낮, 밤, 아침 보상, 보스 접근 같은 페이즈 시간표를 데이터로 관리합니다.
    [Serializable]
    public sealed class GameFlowPhaseData : GameDataBase
    {
        [SerializeField] private int _day;
        [SerializeField] private string _phase;
        [SerializeField] private float _durationSeconds;
        [SerializeField] private string _nextPhaseId;
        [SerializeField] private string _bgmKey;
        [SerializeField] private string _skyboxKey;
        [SerializeField] private string _lightPresetKey;

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

        public float DurationSeconds
        {
            get
            {
                return _durationSeconds;
            }
        }

        public string NextPhaseId
        {
            get
            {
                return _nextPhaseId;
            }
        }

        public string BgmKey
        {
            get
            {
                return _bgmKey;
            }
        }

        public string SkyboxKey
        {
            get
            {
                return _skyboxKey;
            }
        }

        public string LightPresetKey
        {
            get
            {
                return _lightPresetKey;
            }
        }
    }

    // GameFlowPhaseData도 JsonUtility가 읽기 쉬운 Items 감싸기 구조를 사용합니다.
    [Serializable]
    public sealed class GameFlowPhaseDataCatalog
    {
        public List<GameFlowPhaseData> Items = new List<GameFlowPhaseData>();
    }
}
