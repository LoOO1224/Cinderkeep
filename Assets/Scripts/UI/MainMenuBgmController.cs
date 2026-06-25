using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// 플레이 상태를 화면에 표시하거나 사용자의 UI 요청을 전달합니다.
// UI는 규칙을 소유하지 않고 모델을 읽고 시스템에 요청을 보내는 계층으로 유지합니다.
namespace Cinderkeep.UI
{
    // 메인 메뉴에서 시작되는 공용 BGM을 관리합니다.
    // BGM 켜기/끄기와 볼륨 조절은 메인 메뉴 설정창에서만 처리합니다.
    public sealed class MainMenuBgmController : MonoBehaviour
    {
        [SerializeField] private AudioSource _bgmAudioSource;
        [SerializeField] private Toggle _volumeMuteToggle;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Text _volumeMuteText;
        [SerializeField] private Text _volumeValueText;
        [SerializeField] private AudioClip[] _bgmAudioClips;
        [SerializeField] private float _volume = 0.7f;

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
            _bgmAudioSource = audioSourceBgm;
            _volumeMuteToggle = toggleVolumeMute;
            _volumeSlider = sliderVolume;
            _volumeMuteText = textVolumeMute;
            _volumeValueText = textVolumeValue;
            _bgmAudioClips = audioClipsBgm;
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
            if (_bgmAudioSource == null)
            {
                return;
            }

            _bgmAudioSource.playOnAwake = false;
            _bgmAudioSource.loop = false;
            _bgmAudioSource.spatialBlend = 0f;
            ApplyVolume();
        }

        private void InitializeControls()
        {
            if (_volumeMuteToggle != null)
            {
                _volumeMuteToggle.onValueChanged.AddListener(SetVolumeMute);
            }

            if (_volumeSlider != null)
            {
                _volumeSlider.minValue = 0f;
                _volumeSlider.maxValue = 10f;
                _volumeSlider.wholeNumbers = true;
                _volumeSlider.value = Mathf.RoundToInt(Mathf.Clamp01(_volume) * 10f);
                _volumeSlider.onValueChanged.AddListener(SetVolumeBySlider);
            }
        }

        private void ReleaseControls()
        {
            if (_volumeMuteToggle != null)
            {
                _volumeMuteToggle.onValueChanged.RemoveListener(SetVolumeMute);
            }

            if (_volumeSlider != null)
            {
                _volumeSlider.onValueChanged.RemoveListener(SetVolumeBySlider);
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

            _currentClipIndex = Random.Range(0, _bgmAudioClips.Length);
            PlayCurrentBgm();
        }

        private void PlayCurrentBgm()
        {
            if (CheckCanPlayBgm() == false)
            {
                return;
            }

            _bgmAudioSource.clip = _bgmAudioClips[_currentClipIndex];
            ApplyVolume();
            _bgmAudioSource.Play();
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
            if (_bgmAudioSource == null)
            {
                return;
            }

            _bgmAudioSource.Stop();
        }

        private void UpdateBgmLoop()
        {
            if (_isInitialized == false || _isBgmOn == false || _bgmAudioSource == null)
            {
                return;
            }

            if (_bgmAudioSource.isPlaying)
            {
                return;
            }

            PlayNextBgm();
        }
        private void UpdateBgmView()
        {
            UpdateMuteToggle();
            UpdateVolumeText();
        }

        private void UpdateMuteToggle()
        {
            if (_volumeMuteToggle != null)
            {
                _volumeMuteToggle.SetIsOnWithoutNotify(_isBgmOn == false);
            }

            if (_volumeMuteText != null)
            {
                _volumeMuteText.text = "볼륨 끄기";
            }
        }

        private void UpdateVolumeText()
        {
            if (_volumeValueText == null)
            {
                return;
            }

            int volumeLevel = Mathf.RoundToInt(Mathf.Clamp01(_volume) * 10f);
            _volumeValueText.text = volumeLevel.ToString() + " / 10";
        }

        private void ApplyVolume()
        {
            if (_bgmAudioSource == null)
            {
                return;
            }

            _bgmAudioSource.volume = Mathf.Clamp01(_volume);
        }

        private int SelectNextClipIndex()
        {
            if (_bgmAudioClips == null || _bgmAudioClips.Length <= 1)
            {
                return 0;
            }

            int nextClipIndex = Random.Range(0, _bgmAudioClips.Length);

            if (nextClipIndex == _currentClipIndex)
            {
                nextClipIndex++;
            }

            if (nextClipIndex >= _bgmAudioClips.Length)
            {
                nextClipIndex = 0;
            }

            return nextClipIndex;
        }

        private bool CheckCanPlayBgm()
        {
            if (_bgmAudioSource == null || _bgmAudioClips == null || _bgmAudioClips.Length == 0)
            {
                return false;
            }

            if (_bgmAudioClips[_currentClipIndex] == null)
            {
                return false;
            }

            return true;
        }
    }
}
