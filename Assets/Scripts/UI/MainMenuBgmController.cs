using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cinderkeep.UI
{
    // 메인 메뉴에서 시작되는 공용 BGM을 관리합니다.
    // BGM 켜기/끄기와 볼륨 조절은 메인 메뉴 설정창에서만 처리합니다.
    public sealed class MainMenuBgmController : MonoBehaviour
    {
        [FormerlySerializedAs("_bgmAudioSource")]
        [SerializeField] private AudioSource AudioSource_Bgm;
        [FormerlySerializedAs("_volumeMuteToggle")]
        [SerializeField] private Toggle Toggle_VolumeMute;
        [FormerlySerializedAs("_volumeSlider")]
        [SerializeField] private Slider Slider_Volume;
        [FormerlySerializedAs("_volumeMuteText")]
        [SerializeField] private Text Text_VolumeMute;
        [FormerlySerializedAs("_volumeValueText")]
        [SerializeField] private Text Text_VolumeValue;
        [FormerlySerializedAs("_bgmAudioClips")]
        [SerializeField] private AudioClip[] AudioClips_Bgm;
        [SerializeField] private float _volume = 0.3f;

        private static MainMenuBgmController _activeController;

        private int _currentClipIndex;
        private bool _isBgmOn = true;
        private bool _isInitialized;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateCursorState();
            UpdateBgmLoop();
        }

        private void OnDestroy()
        {
            ReleaseControls();

            if (_activeController == this)
            {
                _activeController = null;
            }
        }

        public void SetReferences(
            AudioSource audioSourceBgm,
            Toggle toggleVolumeMute,
            Slider sliderVolume,
            Text textVolumeMute,
            Text textVolumeValue,
            AudioClip[] audioClipsBgm,
            float volume)
        {
            AudioSource_Bgm = audioSourceBgm;
            Toggle_VolumeMute = toggleVolumeMute;
            Slider_Volume = sliderVolume;
            Text_VolumeMute = textVolumeMute;
            Text_VolumeValue = textVolumeValue;
            AudioClips_Bgm = audioClipsBgm;
            _volume = volume;
        }

        public void SetVolumeMute(bool isMute)
        {
            SetBgmOn(isMute == false);
        }

        public void SetVolumeBySlider(float volumeLevel)
        {
            float safeVolumeLevel = Mathf.Clamp(volumeLevel, 0f, 10f);
            _volume = safeVolumeLevel / 10f;
            ApplyVolume();
            UpdateVolumeText();
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_activeController != null && _activeController != this)
            {
                Destroy(gameObject);
                return;
            }

            _activeController = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSource();
            InitializeControls();
            PlayFirstBgm();
            UpdateBgmView();

            _isInitialized = true;
        }

        private void InitializeAudioSource()
        {
            if (AudioSource_Bgm == null)
            {
                return;
            }

            AudioSource_Bgm.playOnAwake = false;
            AudioSource_Bgm.loop = false;
            AudioSource_Bgm.spatialBlend = 0f;
            ApplyVolume();
        }

        private void InitializeControls()
        {
            if (Toggle_VolumeMute != null)
            {
                Toggle_VolumeMute.onValueChanged.AddListener(SetVolumeMute);
            }

            if (Slider_Volume != null)
            {
                Slider_Volume.minValue = 0f;
                Slider_Volume.maxValue = 10f;
                Slider_Volume.wholeNumbers = true;
                Slider_Volume.value = Mathf.RoundToInt(Mathf.Clamp01(_volume) * 10f);
                Slider_Volume.onValueChanged.AddListener(SetVolumeBySlider);
            }
        }

        private void ReleaseControls()
        {
            if (Toggle_VolumeMute != null)
            {
                Toggle_VolumeMute.onValueChanged.RemoveListener(SetVolumeMute);
            }

            if (Slider_Volume != null)
            {
                Slider_Volume.onValueChanged.RemoveListener(SetVolumeBySlider);
            }
        }

        private void SetBgmOn(bool isBgmOn)
        {
            _isBgmOn = isBgmOn;

            if (_isBgmOn)
            {
                PlayCurrentBgm();
            }
            else
            {
                StopBgm();
            }

            UpdateBgmView();
        }

        private void PlayFirstBgm()
        {
            if (CheckCanPlayBgm() == false)
            {
                return;
            }

            _currentClipIndex = Random.Range(0, AudioClips_Bgm.Length);
            PlayCurrentBgm();
        }

        private void PlayCurrentBgm()
        {
            if (CheckCanPlayBgm() == false)
            {
                return;
            }

            AudioSource_Bgm.clip = AudioClips_Bgm[_currentClipIndex];
            ApplyVolume();
            AudioSource_Bgm.Play();
        }

        private void PlayNextBgm()
        {
            if (CheckCanPlayBgm() == false)
            {
                return;
            }

            _currentClipIndex = SelectNextClipIndex();
            PlayCurrentBgm();
        }

        private void StopBgm()
        {
            if (AudioSource_Bgm == null)
            {
                return;
            }

            AudioSource_Bgm.Stop();
        }

        private void UpdateBgmLoop()
        {
            if (_isInitialized == false || _isBgmOn == false || AudioSource_Bgm == null)
            {
                return;
            }

            if (AudioSource_Bgm.isPlaying)
            {
                return;
            }

            PlayNextBgm();
        }

        private void UpdateCursorState()
        {
            if (SceneManager.GetActiveScene().name == "Main_Lobby")
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void UpdateBgmView()
        {
            UpdateMuteToggle();
            UpdateVolumeText();
        }

        private void UpdateMuteToggle()
        {
            if (Toggle_VolumeMute != null)
            {
                Toggle_VolumeMute.SetIsOnWithoutNotify(_isBgmOn == false);
            }

            if (Text_VolumeMute != null)
            {
                Text_VolumeMute.text = "볼륨 끄기";
            }
        }

        private void UpdateVolumeText()
        {
            if (Text_VolumeValue == null)
            {
                return;
            }

            int volumeLevel = Mathf.RoundToInt(Mathf.Clamp01(_volume) * 10f);
            Text_VolumeValue.text = volumeLevel.ToString() + " / 10";
        }

        private void ApplyVolume()
        {
            if (AudioSource_Bgm == null)
            {
                return;
            }

            AudioSource_Bgm.volume = Mathf.Clamp01(_volume);
        }

        private int SelectNextClipIndex()
        {
            if (AudioClips_Bgm == null || AudioClips_Bgm.Length <= 1)
            {
                return 0;
            }

            int nextClipIndex = Random.Range(0, AudioClips_Bgm.Length);

            if (nextClipIndex == _currentClipIndex)
            {
                nextClipIndex++;
            }

            if (nextClipIndex >= AudioClips_Bgm.Length)
            {
                nextClipIndex = 0;
            }

            return nextClipIndex;
        }

        private bool CheckCanPlayBgm()
        {
            if (AudioSource_Bgm == null || AudioClips_Bgm == null || AudioClips_Bgm.Length == 0)
            {
                return false;
            }

            if (AudioClips_Bgm[_currentClipIndex] == null)
            {
                return false;
            }

            return true;
        }
    }
}
