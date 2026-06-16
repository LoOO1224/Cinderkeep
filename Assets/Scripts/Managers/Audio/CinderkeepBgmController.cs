using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OODong.Cinderkeep
{
    // Main hub BGM 전용 컨트롤러입니다.
    // 팀원 작업 영역과 실제 게임 영역을 분리하기 위해 Cinderkeep_Game에서는 자동으로 멈춥니다.
    // Main_Lobby에서 시작된 BGM은 캐릭터 선택/개인 작업실/공동 작업실까지 유지됩니다.
    public sealed class CinderkeepBgmController : MonoBehaviour
    {
        private const string BgmEnabledKey = "OODong.Cinderkeep.BgmEnabled";
        private const string GameplaySceneName = "Cinderkeep_Game";
        private const float DefaultVolume = 0.4f;

        [SerializeField] private AudioSource AudioSource_BgmSource;
        [SerializeField] private AudioClip AudioClip_BgmClip;
        [SerializeField] private AudioClip[] AudioClip_BgmPlaylist = new AudioClip[0];
        [SerializeField] private float _volume = DefaultVolume;
        [SerializeField] private bool _playOnAwake = true;

        private int _currentClipIndex = -1;

        public static CinderkeepBgmController Instance { get; private set; }

        public bool IsBgmEnabled => PlayerPrefs.GetInt(BgmEnabledKey, 1) == 1;
        public float Volume => _volume;
        public AudioClip BgmClip => AudioClip_BgmClip;
        public AudioClip[] BgmClips => AudioClip_BgmPlaylist;
        public bool IsScenePlaybackAllowed => IsBgmAllowedInScene(SceneManager.GetActiveScene().name);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Instance.AbsorbReferencesFrom(this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
            ResolveReferences();
            ResolvePlaylist();
            ApplySourceSettings();
            SceneManager.sceneLoaded += HandleSceneLoaded;

            if (_playOnAwake)
            {
                RefreshPlayback();
            }
        }

        private void Update()
        {
            if (AudioSource_BgmSource == null || !IsBgmEnabled || !IsScenePlaybackAllowed)
            {
                return;
            }

            if (AudioSource_BgmSource.clip != null && !AudioSource_BgmSource.isPlaying)
            {
                PlayNextClip(true);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        public void SetReferences(AudioSource bgmSource, AudioClip bgmClip, float volume)
        {
            SetReferences(bgmSource, bgmClip != null ? new[] { bgmClip } : null, volume);
        }

        public void SetReferences(AudioSource bgmSource, AudioClip[] bgmPlaylist, float volume)
        {
            AudioSource_BgmSource = bgmSource;
            SetPlaylist(bgmPlaylist);
            // 현재 Cinderkeep 허브 BGM 정책: 어떤 BGM을 추가해도 기본 볼륨은 0.4입니다.
            _volume = DefaultVolume;
            ResolveReferences();
            ApplySourceSettings();
        }

        public void ToggleBgm()
        {
            SetBgmEnabled(!IsBgmEnabled);
        }

        public void SetBgmEnabled(bool isEnabled)
        {
            PlayerPrefs.SetInt(BgmEnabledKey, isEnabled ? 1 : 0);
            PlayerPrefs.Save();
            RefreshPlayback();
        }

        public void RefreshPlayback()
        {
            ResolveReferences();
            ResolvePlaylist();
            ApplySourceSettings();

            if (AudioSource_BgmSource == null)
            {
                return;
            }

            if (!IsBgmEnabled || !IsScenePlaybackAllowed || AudioClip_BgmPlaylist.Length <= 0)
            {
                AudioSource_BgmSource.Stop();
                return;
            }

            if (!AudioSource_BgmSource.isPlaying)
            {
                PlayNextClip(false);
            }
        }

        private void AbsorbReferencesFrom(CinderkeepBgmController other)
        {
            if (other == null)
            {
                return;
            }

            if (other.AudioClip_BgmPlaylist != null && other.AudioClip_BgmPlaylist.Length > 0)
            {
                SetPlaylist(other.AudioClip_BgmPlaylist);
            }
            else if (AudioClip_BgmPlaylist.Length <= 0 && other.AudioClip_BgmClip != null)
            {
                SetPlaylist(new[] { other.AudioClip_BgmClip });
            }

            _volume = DefaultVolume;
            ApplySourceSettings();
            RefreshPlayback();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshPlayback();
        }

        private void ResolveReferences()
        {
            if (AudioSource_BgmSource == null)
            {
                AudioSource_BgmSource = GetComponent<AudioSource>();
            }

            if (AudioSource_BgmSource == null)
            {
                AudioSource_BgmSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void ResolvePlaylist()
        {
            if ((AudioClip_BgmPlaylist == null || AudioClip_BgmPlaylist.Length <= 0) && AudioClip_BgmClip != null)
            {
                SetPlaylist(new[] { AudioClip_BgmClip });
            }
        }

        private void SetPlaylist(AudioClip[] bgmPlaylist)
        {
            List<AudioClip> validClips = new List<AudioClip>();
            if (bgmPlaylist != null)
            {
                for (int i = 0; i < bgmPlaylist.Length; i++)
                {
                    if (bgmPlaylist[i] != null && !validClips.Contains(bgmPlaylist[i]))
                    {
                        validClips.Add(bgmPlaylist[i]);
                    }
                }
            }

            AudioClip_BgmPlaylist = validClips.ToArray();
            if (AudioClip_BgmPlaylist.Length <= 0)
            {
                AudioClip_BgmClip = null;
                _currentClipIndex = -1;
                return;
            }

            _currentClipIndex = -1;
            AudioClip_BgmClip = AudioClip_BgmPlaylist[0];
        }

        private void PlayNextClip(bool shouldAdvance)
        {
            if (AudioSource_BgmSource == null || AudioClip_BgmPlaylist == null || AudioClip_BgmPlaylist.Length <= 0)
            {
                return;
            }

            if (shouldAdvance || _currentClipIndex < 0 || _currentClipIndex >= AudioClip_BgmPlaylist.Length)
            {
                _currentClipIndex = GetNextShuffleIndex();
            }
            else if (AudioSource_BgmSource.clip == null)
            {
                _currentClipIndex = GetNextShuffleIndex();
            }

            AudioClip_BgmClip = AudioClip_BgmPlaylist[_currentClipIndex];
            AudioSource_BgmSource.clip = AudioClip_BgmClip;
            AudioSource_BgmSource.Play();
        }

        private int GetNextShuffleIndex()
        {
            if (AudioClip_BgmPlaylist == null || AudioClip_BgmPlaylist.Length <= 1)
            {
                return 0;
            }

            int nextIndex = Random.Range(0, AudioClip_BgmPlaylist.Length - 1);
            if (nextIndex >= _currentClipIndex)
            {
                nextIndex++;
            }

            return Mathf.Clamp(nextIndex, 0, AudioClip_BgmPlaylist.Length - 1);
        }

        private void ApplySourceSettings()
        {
            if (AudioSource_BgmSource == null)
            {
                return;
            }

            AudioSource_BgmSource.clip = AudioClip_BgmClip;
            AudioSource_BgmSource.loop = false;
            AudioSource_BgmSource.playOnAwake = false;
            AudioSource_BgmSource.spatialBlend = 0f;
            AudioSource_BgmSource.volume = _volume;
        }

        private static bool IsBgmAllowedInScene(string sceneName)
        {
            return !string.Equals(sceneName, GameplaySceneName, System.StringComparison.Ordinal);
        }
    }
}
