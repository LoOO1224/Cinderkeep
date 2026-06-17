using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MainHub.CharacterSelect
{
    public sealed class MainHub_WorkProgressProfile : MonoBehaviour
    {
        private const string SavePrefix = "MainHub.DeveloperWorkProgress.";

        [SerializeField] private string _characterName;
        [SerializeField] private string _roleName;
        [SerializeField] private string _ownerEnglishName;
        [SerializeField] private float _experiencePerSecond = 6f;
        [SerializeField] private int _baseExperienceToLevelUp = 100;
        [FormerlySerializedAs("_profileTitleText")]
        [SerializeField] private Text Text_ProfileTitle;
        [FormerlySerializedAs("_levelText")]
        [SerializeField] private Text Text_Level;
        [FormerlySerializedAs("_statusText")]
        [SerializeField] private Text Text_Status;
        [FormerlySerializedAs("_experienceText")]
        [SerializeField] private Text Text_Experience;
        [FormerlySerializedAs("_experienceFillImage")]
        [SerializeField] private Image Image_ExperienceFill;

        private int _level = 1;
        private float _experience;
        private float _workTime;

        public int Level
        {
            get
            {
                return _level;
            }
        }

        public float Experience
        {
            get
            {
                return _experience;
            }
        }

        private void Awake()
        {
            LoadProgress();
            RefreshView();
        }

        private void Update()
        {
            AddExperience(_experiencePerSecond * Time.deltaTime);
            _workTime += Time.deltaTime;
            RefreshView();
        }

        private void OnDisable()
        {
            SaveProgress();
        }

        public void SetProfile(string characterName, string roleName, string ownerEnglishName)
        {
            _characterName = characterName;
            _roleName = roleName;
            _ownerEnglishName = ownerEnglishName;
        }

        public void SetViewReferences(
            Text profileTitleText,
            Text levelText,
            Text statusText,
            Text experienceText,
            Image experienceFillImage)
        {
            Text_ProfileTitle = profileTitleText;
            Text_Level = levelText;
            Text_Status = statusText;
            Text_Experience = experienceText;
            Image_ExperienceFill = experienceFillImage;
        }

        private void AddExperience(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _experience += amount;
            while (_experience >= GetExperienceToLevelUp())
            {
                _experience -= GetExperienceToLevelUp();
                _level++;
            }
        }

        private void RefreshView()
        {
            int requiredExperience = GetExperienceToLevelUp();
            float progress = Mathf.Clamp01(_experience / requiredExperience);

            if (Text_ProfileTitle != null)
            {
                Text_ProfileTitle.text = $"{_roleName} {_ownerEnglishName}";
            }

            if (Text_Level != null)
            {
                Text_Level.text = $"DEV LEVEL {_level:00}";
            }

            if (Text_Status != null)
            {
                Text_Status.text = $"?? ?? ? - {_ownerEnglishName} {GetWorkingDots()}";
            }

            if (Text_Experience != null)
            {
                Text_Experience.text = $"EXP {Mathf.FloorToInt(_experience):000} / {requiredExperience:000}";
            }

            if (Image_ExperienceFill != null)
            {
                Image_ExperienceFill.fillAmount = progress;
            }
        }

        private string GetWorkingDots()
        {
            int dotCount = 1 + (Mathf.FloorToInt(_workTime * 2f) % 3);
            return new string('.', dotCount);
        }

        private int GetExperienceToLevelUp()
        {
            return _baseExperienceToLevelUp + ((_level - 1) * 30);
        }

        private void LoadProgress()
        {
            string key = GetSaveKey();
            _level = Mathf.Max(1, PlayerPrefs.GetInt($"{key}.level", 1));
            _experience = Mathf.Max(0f, PlayerPrefs.GetFloat($"{key}.experience", 0f));
        }

        private void SaveProgress()
        {
            string key = GetSaveKey();
            PlayerPrefs.SetInt($"{key}.level", _level);
            PlayerPrefs.SetFloat($"{key}.experience", _experience);
            PlayerPrefs.Save();
        }

        private string GetSaveKey()
        {
            return $"{SavePrefix}{_characterName}";
        }
    }
}
