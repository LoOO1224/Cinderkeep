using System;

namespace Cinderkeep.Gameplay
{
    // Instance data for one gameplay run.
    // The 3일 게임 루프 starts with day 1-3 and can expand to a longer loop later.
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
