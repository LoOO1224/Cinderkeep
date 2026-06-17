using System;

namespace Cinderkeep.Gameplay
{
    // 플레이 중 저장되어야 하는 플레이어 Instance Data입니다.
    // 변하지 않는 기획 데이터는 GameData, 변하는 저장 데이터는 Model 이름을 붙입니다.
    [Serializable]
    public sealed class PlayerModel
    {
        private int _health;
        private int _maxHealth;
        private int _level;

        public int Health
        {
            get
            {
                return _health;
            }
        }

        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
        }

        public int Level
        {
            get
            {
                return _level;
            }
        }

        public void InitializeDefault()
        {
            _maxHealth = 100;
            _health = _maxHealth;
            _level = 1;
        }
    }

    // 한 판의 진행 상태를 담는 Instance Data입니다.
    // 현재는 3일/15분 MVP 루프의 기본 진행 상태만 보관합니다.
    // Day는 1~3을 먼저 사용하고, 확장 단계에서 1~7로 늘립니다.
    // 이후 7일 루프, 보스전, 추위 같은 확장 상태가 여기에 추가됩니다.
    [Serializable]
    public sealed class GameRunModel
    {
        private int _day;
        private bool _isPlaying;

        public int Day
        {
            get
            {
                return _day;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
        }

        public void InitializeDefault()
        {
            _day = 1;
            _isPlaying = false;
        }

        public void StartRun()
        {
            _day = 1;
            _isPlaying = true;
        }

        public void EndRun()
        {
            _isPlaying = false;
        }
    }
}
