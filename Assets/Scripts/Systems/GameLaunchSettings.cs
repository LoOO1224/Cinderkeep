using System;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    public enum GameLaunchMode
    {
        Normal,
        TestFast
    }

    // 메인 메뉴에서 선택한 실행 모드와 낮/밤 시간값을 보관합니다.
    // 일반 모드와 테스트초고속 모드는 같은 게임 씬을 쓰고, 시간 설정만 다르게 적용합니다.
    public static class GameLaunchSettings
    {
        private const string SettingsResourcePath = "Cinderkeep/data/game_mode_settings";

        private static GameLaunchMode _mode = GameLaunchMode.Normal;
        private static GameModeSettingsData _cachedSettings;

        public static GameLaunchMode Mode
        {
            get
            {
                return _mode;
            }
        }

        public static bool IsTestFastMode
        {
            get
            {
                return _mode == GameLaunchMode.TestFast;
            }
        }

        public static string ModeDisplayName
        {
            get
            {
                return IsTestFastMode ? "테스트초고속 모드" : "일반 모드";
            }
        }

        public static void SetNormalMode()
        {
            _mode = GameLaunchMode.Normal;
        }

        public static void SetTestFastMode()
        {
            _mode = GameLaunchMode.TestFast;
        }

        public static void ReloadSettings()
        {
            _cachedSettings = null;
        }

        public static GameModeSettingsData GetSettings()
        {
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            TextAsset settingsAsset = Resources.Load<TextAsset>(SettingsResourcePath);
            if (settingsAsset != null && string.IsNullOrWhiteSpace(settingsAsset.text) == false)
            {
                _cachedSettings = JsonUtility.FromJson<GameModeSettingsData>(settingsAsset.text);
            }

            if (_cachedSettings == null)
            {
                _cachedSettings = GameModeSettingsData.CreateDefault();
            }

            _cachedSettings.ClampValues();
            return _cachedSettings;
        }

        public static bool TryGetDurationOverride(GameRunPhase phase, out float duration)
        {
            GameModeSettingsData settings = GetSettings();
            duration = 0f;

            switch (phase)
            {
                case GameRunPhase.Day:
                    duration = IsTestFastMode ? settings.TestFastDayDuration : settings.NormalDayDuration;
                    return true;
                case GameRunPhase.Night:
                    duration = IsTestFastMode ? settings.TestFastNightDuration : settings.NormalNightDuration;
                    return true;
            }

            return false;
        }
    }

    [Serializable]
    public sealed class GameModeSettingsData
    {
        private const float DefaultNormalDayDuration = 180f;
        private const float DefaultNormalNightDuration = 120f;
        private const float DefaultTestFastDayDuration = 90f;
        private const float DefaultTestFastNightDuration = 60f;

        [SerializeField] private float _normalDayDuration = DefaultNormalDayDuration;
        [SerializeField] private float _normalNightDuration = DefaultNormalNightDuration;
        [SerializeField] private float _testFastDayDuration = DefaultTestFastDayDuration;
        [SerializeField] private float _testFastNightDuration = DefaultTestFastNightDuration;

        public float NormalDayDuration
        {
            get
            {
                return _normalDayDuration;
            }
        }

        public float NormalNightDuration
        {
            get
            {
                return _normalNightDuration;
            }
        }

        public float TestFastDayDuration
        {
            get
            {
                return _testFastDayDuration;
            }
        }

        public float TestFastNightDuration
        {
            get
            {
                return _testFastNightDuration;
            }
        }

        public static GameModeSettingsData CreateDefault()
        {
            GameModeSettingsData settings = new GameModeSettingsData();
            settings.SetDurations(
                DefaultNormalDayDuration,
                DefaultNormalNightDuration,
                DefaultTestFastDayDuration,
                DefaultTestFastNightDuration);
            return settings;
        }

        public void SetDurations(float normalDayDuration, float normalNightDuration, float testFastDayDuration, float testFastNightDuration)
        {
            _normalDayDuration = normalDayDuration;
            _normalNightDuration = normalNightDuration;
            _testFastDayDuration = testFastDayDuration;
            _testFastNightDuration = testFastNightDuration;
            ClampValues();
        }

        public void ClampValues()
        {
            _normalDayDuration = Mathf.Max(1f, _normalDayDuration);
            _normalNightDuration = Mathf.Max(1f, _normalNightDuration);
            _testFastDayDuration = Mathf.Max(1f, _testFastDayDuration);
            _testFastNightDuration = Mathf.Max(1f, _testFastNightDuration);
        }
    }
}
