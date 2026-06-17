using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainHub.Audio
{
    // MainHub ??? ?? ??????? BGM? ???? ???????.
    // ?? ?? ?(Cinderkeep_Game)? ???? BGM? ?? ????? ??? ?????.
    // ? BGM? ???? ?? ??? 0.4? ????.
    public sealed class MainHub_BgmController : MonoBehaviour
    {
        private const string BgmEnabledKey = "MainHub.Cinderkeep.BgmEnabled";
        private const string GameplaySceneName = "Cinderkeep_Game";
        private const float DefaultVolume = 0.4f;

        [SerializeField] private AudioSource AudioSource_BgmSource;
        [SerializeField] private AudioClip AudioClip_BgmClip;
        [SerializeField] private AudioClip[] AudioClip_BgmPlaylist = new AudioClip[0];
        [SerializeField] private float _volume = DefaultVolume;
        [SerializeField] private bool _playOnAwake = true;

        private int _currentClipIndex = -1;

        public static MainHub_BgmController Instance { get; private set; }

        public bool IsBgmEnabled
        {
            get
            {
                return PlayerPrefs.GetInt(BgmEnabledKey, 1) == 1;
            }
        }

        public float Volume
        {
            get
            {
                return _volume;
            }
        }

        public AudioClip BgmClip
        {
            get
            {
                return AudioClip_BgmClip;
            }
        }

        public AudioClip[] BgmClips
        {
            get
            {
                return AudioClip_BgmPlaylist;
            }
        }

        public bool IsScenePlaybackAllowed
        {
            get
            {
                return IsBgmAllowedInScene(SceneManager.GetActiveScene().name);
            }
        }

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
            // ?? ?? BGM? ? ?? ?? 0.4? ?????.
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

        private void AbsorbReferencesFrom(MainHub_BgmController other)
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
