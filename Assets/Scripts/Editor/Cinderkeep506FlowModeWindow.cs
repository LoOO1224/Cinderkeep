using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Cinderkeep.EditorTools
{
    // 일반/테스트초고속 모드의 낮밤 시간을 빠르게 조정하는 에디터 창입니다.
    // 값은 Resources JSON에 저장되어 플레이 모드와 빌드에서 같은 기준으로 읽힙니다.
    public sealed class Cinderkeep506FlowModeWindow : EditorWindow
    {
        private const string SettingsAssetPath = "Assets/Resources/Cinderkeep/data/game_mode_settings.json";

        private float _normalDayDuration;
        private float _normalNightDuration;
        private float _testFastDayDuration;
        private float _testFastNightDuration;

        [MenuItem("Cinderkeep/5.23 Flow Mode/Open Time Settings")]
        [MenuItem("Cinderkeep/5.06 Flow Mode/Open Time Settings")]
        public static void OpenWindow()
        {
            Cinderkeep506FlowModeWindow window = GetWindow<Cinderkeep506FlowModeWindow>("Flow Mode");
            window.minSize = new Vector2(360f, 230f);
            window.LoadSettings();
            window.Show();
        }

        [MenuItem("Cinderkeep/5.23 Flow Mode/Reset Default Time Settings")]
        [MenuItem("Cinderkeep/5.06 Flow Mode/Reset Default Time Settings")]
        public static void ResetDefaultSettings()
        {
            GameModeSettingsData settings = GameModeSettingsData.CreateDefault();
            SaveSettingsAsset(settings);
            Debug.Log("Cinderkeep Flow Mode: time settings reset to defaults. " + GameLaunchSettings.GetSettingsSummary());
        }

        [MenuItem("Cinderkeep/5.23 Flow Mode/Print Current Time Settings")]
        public static void PrintCurrentSettings()
        {
            GameLaunchSettings.ReloadSettings();
            Debug.Log("Cinderkeep Flow Mode: " + GameLaunchSettings.GetSettingsSummary());
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("일반 모드", EditorStyles.boldLabel);
            _normalDayDuration = EditorGUILayout.FloatField("낮 시간", _normalDayDuration);
            _normalNightDuration = EditorGUILayout.FloatField("밤 시간", _normalNightDuration);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("테스트초고속 모드", EditorStyles.boldLabel);
            _testFastDayDuration = EditorGUILayout.FloatField("낮 시간", _testFastDayDuration);
            _testFastNightDuration = EditorGUILayout.FloatField("밤 시간", _testFastNightDuration);

            EditorGUILayout.Space(14f);
            EditorGUILayout.HelpBox("기본값: 일반 낮 180초 / 밤 120초, 테스트초고속 낮 90초 / 밤 60초", MessageType.Info);
            EditorGUILayout.HelpBox("현재 파일 기준: " + GameLaunchSettings.GetSettingsSummary(), MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("현재값 다시 읽기"))
            {
                LoadSettings();
            }

            if (GUILayout.Button("기본값"))
            {
                ApplyDefaultValues();
            }

            if (GUILayout.Button("저장"))
            {
                SaveCurrentValues();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void LoadSettings()
        {
            GameModeSettingsData settings = GameLaunchSettings.GetSettings();
            _normalDayDuration = settings.NormalDayDuration;
            _normalNightDuration = settings.NormalNightDuration;
            _testFastDayDuration = settings.TestFastDayDuration;
            _testFastNightDuration = settings.TestFastNightDuration;
        }

        private void ApplyDefaultValues()
        {
            GameModeSettingsData settings = GameModeSettingsData.CreateDefault();
            _normalDayDuration = settings.NormalDayDuration;
            _normalNightDuration = settings.NormalNightDuration;
            _testFastDayDuration = settings.TestFastDayDuration;
            _testFastNightDuration = settings.TestFastNightDuration;
        }

        private void SaveCurrentValues()
        {
            GameModeSettingsData settings = GameModeSettingsData.CreateDefault();
            settings.SetDurations(_normalDayDuration, _normalNightDuration, _testFastDayDuration, _testFastNightDuration);
            SaveSettingsAsset(settings);
        }

        private static void SaveSettingsAsset(GameModeSettingsData settings)
        {
            settings.ClampValues();
            string json = JsonUtility.ToJson(settings, true);
            System.IO.File.WriteAllText(SettingsAssetPath, json, System.Text.Encoding.UTF8);
            AssetDatabase.ImportAsset(SettingsAssetPath);
            AssetDatabase.SaveAssets();
            GameLaunchSettings.ReloadSettings();
        }
    }
}
