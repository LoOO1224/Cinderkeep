using System;

// 한 번의 플레이 진행 상태를 저장하는 런타임 모델입니다.
// 현재 일차, 페이즈, 남은 시간, 승패 상태만 보관하고 실제 규칙 실행은 컨트롤러가 담당합니다.
namespace Cinderkeep.Gameplay
{
    [Serializable]
    public sealed class GameRunModel
    {
        public const int FirstDay = 1;
        public const int FinalDay = 3;

        private int _day;
        private int _finalDay = FinalDay;
        private GameRunPhase _phase;
        private float _remainingTime;
        private float _phaseDuration;
        private bool _isPlaying;
        private bool _isGameOver;
        private bool _isClear;

        public int Day
        {
            get
            {
                return _day;
            }
        }

        public GameRunPhase Phase
        {
            get
            {
                return _phase;
            }
        }

        public float RemainingTime
        {
            get
            {
                return _remainingTime;
            }
        }

        public float PhaseDuration
        {
            get
            {
                return _phaseDuration;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
        }

        public bool IsGameOver
        {
            get
            {
                return _isGameOver;
            }
        }

        public bool IsClear
        {
            get
            {
                return _isClear;
            }
        }

        public void InitializeDefault()
        {
            _day = FirstDay;
            _finalDay = FinalDay;
            _phase = GameRunPhase.None;
            _remainingTime = 0f;
            _phaseDuration = 0f;
            _isPlaying = false;
            _isGameOver = false;
            _isClear = false;
        }

        public void StartRun()
        {
            _day = FirstDay;
            _phase = GameRunPhase.Day;
            _remainingTime = 0f;
            _phaseDuration = 0f;
            _isPlaying = true;
            _isGameOver = false;
            _isClear = false;
        }

        public void EndRun()
        {
            _isPlaying = false;
            _isGameOver = true;
            _isClear = false;
            _phase = GameRunPhase.GameOver;
            _remainingTime = 0f;
            _phaseDuration = 0f;
        }

        public void ClearRun()
        {
            _isPlaying = false;
            _isGameOver = false;
            _isClear = true;
            _phase = GameRunPhase.Clear;
            _remainingTime = 0f;
            _phaseDuration = 0f;
        }

        public void SetFinalDay(int finalDay)
        {
            _finalDay = Math.Max(FirstDay, finalDay);
            if (_day > _finalDay)
            {
                _day = _finalDay;
            }
        }

        public int GetFinalDay()
        {
            return _finalDay;
        }

        public void SetDay(int day)
        {
            if (day < FirstDay)
            {
                _day = FirstDay;
                return;
            }

            if (day > _finalDay)
            {
                _day = _finalDay;
                return;
            }

            _day = day;
        }

        public void SetPhase(GameRunPhase phase)
        {
            _phase = phase;
        }

        public void SetRemainingTime(float remainingTime)
        {
            if (remainingTime < 0f)
            {
                _remainingTime = 0f;
                return;
            }

            _remainingTime = remainingTime;
        }

        public void SetPhaseDuration(float phaseDuration)
        {
            if (phaseDuration < 0f)
            {
                _phaseDuration = 0f;
                return;
            }

            _phaseDuration = phaseDuration;
        }

        public void SetPhaseTime(float phaseDuration)
        {
            SetPhaseDuration(phaseDuration);
            SetRemainingTime(phaseDuration);
        }

        public void AdvanceDay()
        {
            SetDay(_day + 1);
        }

        public bool IsFinalDay()
        {
            return _day >= _finalDay;
        }
    }

    public enum GameRunPhase
    {
        None,
        Day,
        Night,
        MorningReward,
        BossApproach,
        BossFight,
        GameOver,
        Clear
    }
}
