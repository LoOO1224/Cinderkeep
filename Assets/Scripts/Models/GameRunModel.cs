using System;

// 한 판 플레이 중 변하는 런타임 상태를 저장합니다.
// 상태 변경은 명시적인 메서드로 처리하고, UI와 시스템은 이 모델을 읽거나 요청만 보냅니다.
namespace Cinderkeep.Gameplay
{
    // 한 번의 플레이 진행 상황을 저장하는 Instance Data입니다.
    // GameFlowController는 이 모델에 현재 일차, 페이즈, 남은 시간을 기록합니다.
    [Serializable]
    public sealed class GameRunModel
    {
        public const int FirstDay = 1;
        public const int FinalDay = 3;

        private int _day;
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

        public void SetDay(int day)
        {
            if (day < FirstDay)
            {
                _day = FirstDay;
                return;
            }

            if (day > FinalDay)
            {
                _day = FinalDay;
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
            return _day >= FinalDay;
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
