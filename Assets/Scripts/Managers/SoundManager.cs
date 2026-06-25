using UnityEngine;

using System.Collections.Generic;
using UnityEngine.Serialization;

// 씬 참조와 런타임 모델을 연결하는 얇은 매니저 허브입니다.
// 계산과 세부 규칙은 작은 시스템/헬퍼로 분리하고, 이 클래스는 연결 책임에 집중합니다.
namespace Cinderkeep.Gameplay
{
    // 게임 씬 전용 사운드 매니저입니다.
    // 현재는 게임 루프의 BGM과 전투/채집 효과음 재생 기반만 준비합니다.
    // BGM과 효과음 AudioSource는 Inspector에서 연결하고, 다른 스크립트는 이 매니저 함수만 호출합니다.
    public sealed class SoundManager : MonoBehaviour, IGameInitializable
    {
        private const string BgmRootPath = "Cinderkeep/audio/bgm";
        private const string SfxRootPath = "Cinderkeep/audio/sfx/";

        private const string UiClickClipPath = SfxRootPath + "sfx_ui_click";
        private const string UiBackClipPath = SfxRootPath + "sfx_ui_back";
        private const string UiNotificationClipPath = SfxRootPath + "sfx_ui_notification";
        private const string UiSuccessClipPath = SfxRootPath + "sfx_ui_success";
        private const string UiFailClipPath = SfxRootPath + "sfx_ui_fail";
        private const string RewardSelectClipPath = SfxRootPath + "sfx_reward_select";
        private const string HealClipPath = SfxRootPath + "sfx_heal";
        private const string ResourcePickupClipPath = SfxRootPath + "sfx_resource_pickup";
        private const string ResourceOreHitClipPath = SfxRootPath + "sfx_resource_ore_hit";

        [SerializeField] private AudioSource _bgmAudioSource;
        [SerializeField] private AudioSource _effectAudioSource;
        [SerializeField] private float _defaultVolume = 0.7f;

        private AudioClip _uiClickClip;
        private AudioClip _uiBackClip;
        private AudioClip _uiNotificationClip;
        private AudioClip _uiSuccessClip;
        private AudioClip _uiFailClip;
        private AudioClip _rewardSelectClip;
        private AudioClip _healClip;
        private AudioClip _resourcePickupClip;
        private AudioClip _resourceOreHitClip;
        private AudioClip[] _allBgmClips = new AudioClip[0];
        private AudioClip[] _dayBgmClips = new AudioClip[0];
        private AudioClip[] _nightBgmClips = new AudioClip[0];
        private AudioClip[] _rewardBgmClips = new AudioClip[0];
        private AudioClip[] _bossBgmClips = new AudioClip[0];
        private AudioClip _currentBgmClip;

        private bool _isInitialized;

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            SetVolume(_defaultVolume);
            InitializeAudioSources();
            LoadDefaultBgmClips();
            LoadDefaultSfxClips();
            _isInitialized = true;
        }

        public void PlayBgm(AudioClip bgmClip)
        {
            if (_bgmAudioSource == null || bgmClip == null)
            {
                return;
            }

            _bgmAudioSource.clip = bgmClip;
            _bgmAudioSource.loop = true;
            _bgmAudioSource.volume = Mathf.Clamp01(_defaultVolume);
            _bgmAudioSource.spatialBlend = 0f;
            _currentBgmClip = bgmClip;
            _bgmAudioSource.Play();
        }

        public void PlayBgmForPhase(GameRunPhase phase)
        {
            AudioClip[] phaseClips = GetBgmClipsForPhase(phase);
            AudioClip selectedClip = SelectRandomBgmClip(phaseClips);
            if (selectedClip == null)
            {
                return;
            }

            PlayBgm(selectedClip);
        }

        public void PlayBgmForPhase(GameRunPhase phase, string bgmKey)
        {
            AudioClip[] namedClips = GetBgmClipsForKey(bgmKey);
            if (namedClips == null || namedClips.Length == 0)
            {
                PlayBgmForPhase(phase);
                return;
            }

            AudioClip selectedClip = SelectRandomBgmClip(namedClips);
            if (selectedClip == null)
            {
                return;
            }

            PlayBgm(selectedClip);
        }

        public void StopBgm()
        {
            if (_bgmAudioSource == null)
            {
                return;
            }

            _bgmAudioSource.Stop();
        }

        public void PlayEffect(AudioClip effectClip)
        {
            if (_effectAudioSource == null || effectClip == null)
            {
                return;
            }

            _effectAudioSource.PlayOneShot(effectClip);
        }

        public void PlayUiClick()
        {
            PlayEffect(_uiClickClip);
        }

        public void PlayUiBack()
        {
            PlayEffect(_uiBackClip);
        }

        public void PlayUiNotification()
        {
            PlayEffect(_uiNotificationClip);
        }

        public void PlayUiSuccess()
        {
            PlayEffect(_uiSuccessClip);
        }

        public void PlayUiFail()
        {
            PlayEffect(_uiFailClip);
        }

        public void PlayCraftSuccess()
        {
            PlayUiSuccess();
        }

        public void PlayCraftFail()
        {
            PlayUiFail();
        }

        public void PlayRewardSelect()
        {
            PlayEffect(_rewardSelectClip);
        }

        public void PlayHeal()
        {
            PlayEffect(_healClip);
        }

        public void PlayResourcePickup()
        {
            PlayEffect(_resourcePickupClip);
        }

        public void PlayResourceGather(string resourceId)
        {
            if (IsOreLikeResource(resourceId))
            {
                PlayEffect(_resourceOreHitClip);
                return;
            }

            PlayEffect(_resourcePickupClip);
        }

        public void SetVolume(float volume)
        {
            float safeVolume = Mathf.Clamp01(volume);

            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.volume = safeVolume;
            }

            if (_effectAudioSource != null)
            {
                _effectAudioSource.volume = safeVolume;
            }
        }

        private void LoadDefaultSfxClips()
        {
            _uiClickClip = LoadSfxClip(UiClickClipPath);
            _uiBackClip = LoadSfxClip(UiBackClipPath);
            _uiNotificationClip = LoadSfxClip(UiNotificationClipPath);
            _uiSuccessClip = LoadSfxClip(UiSuccessClipPath);
            _uiFailClip = LoadSfxClip(UiFailClipPath);
            _rewardSelectClip = LoadSfxClip(RewardSelectClipPath);
            _healClip = LoadSfxClip(HealClipPath);
            _resourcePickupClip = LoadSfxClip(ResourcePickupClipPath);
            _resourceOreHitClip = LoadSfxClip(ResourceOreHitClipPath);
        }

        private void InitializeAudioSources()
        {
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.playOnAwake = false;
                _bgmAudioSource.loop = true;
                _bgmAudioSource.spatialBlend = 0f;
                _bgmAudioSource.mute = false;
            }

            if (_effectAudioSource != null)
            {
                _effectAudioSource.playOnAwake = false;
                _effectAudioSource.loop = false;
                _effectAudioSource.spatialBlend = 0f;
                _effectAudioSource.mute = false;
            }
        }

        private void LoadDefaultBgmClips()
        {
            _allBgmClips = Resources.LoadAll<AudioClip>(BgmRootPath);
            if (_allBgmClips == null || _allBgmClips.Length == 0)
            {
                _allBgmClips = new AudioClip[0];
                Debug.LogWarning("SoundManager: BGM clips were not found at Resources/" + BgmRootPath + ".");
                return;
            }

            List<AudioClip> dayClips = new List<AudioClip>();
            List<AudioClip> nightClips = new List<AudioClip>();
            List<AudioClip> rewardClips = new List<AudioClip>();
            List<AudioClip> bossClips = new List<AudioClip>();

            for (int i = 0; i < _allBgmClips.Length; i++)
            {
                AudioClip clip = _allBgmClips[i];
                if (clip == null)
                {
                    continue;
                }

                string clipName = clip.name.ToLowerInvariant();
                if (IsDayBgmName(clipName))
                {
                    dayClips.Add(clip);
                }

                if (IsNightBgmName(clipName))
                {
                    nightClips.Add(clip);
                }

                if (IsRewardBgmName(clipName))
                {
                    rewardClips.Add(clip);
                }

                if (IsBossBgmName(clipName))
                {
                    bossClips.Add(clip);
                }
            }

            _dayBgmClips = GetSafeBgmBucket(dayClips);
            _nightBgmClips = GetSafeBgmBucket(nightClips);
            _rewardBgmClips = GetSafeBgmBucket(rewardClips);
            _bossBgmClips = GetSafeBgmBucket(bossClips);
        }

        private AudioClip[] GetSafeBgmBucket(List<AudioClip> clips)
        {
            if (clips == null || clips.Count == 0)
            {
                return _allBgmClips;
            }

            return clips.ToArray();
        }

        private AudioClip[] GetBgmClipsForPhase(GameRunPhase phase)
        {
            switch (phase)
            {
                case GameRunPhase.Day:
                    return _dayBgmClips;
                case GameRunPhase.Night:
                    return _nightBgmClips;
                case GameRunPhase.MorningReward:
                    return _rewardBgmClips;
                case GameRunPhase.BossApproach:
                case GameRunPhase.BossFight:
                    return _bossBgmClips;
                default:
                    return _allBgmClips;
            }
        }

        private AudioClip[] GetBgmClipsForKey(string bgmKey)
        {
            if (string.IsNullOrEmpty(bgmKey))
            {
                return null;
            }

            string lowerKey = bgmKey.ToLowerInvariant();
            if (lowerKey.Contains("day") || lowerKey.Contains("dawn"))
            {
                return _dayBgmClips;
            }

            if (lowerKey.Contains("night"))
            {
                return _nightBgmClips;
            }

            if (lowerKey.Contains("reward") || lowerKey.Contains("morning"))
            {
                return _rewardBgmClips;
            }

            if (lowerKey.Contains("boss"))
            {
                return _bossBgmClips;
            }

            return null;
        }

        private AudioClip SelectRandomBgmClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            if (clips.Length == 1)
            {
                return clips[0];
            }

            AudioClip selectedClip = clips[Random.Range(0, clips.Length)];
            if (selectedClip != _currentBgmClip)
            {
                return selectedClip;
            }

            int currentIndex = System.Array.IndexOf(clips, selectedClip);
            int nextIndex = (currentIndex + 1) % clips.Length;
            return clips[nextIndex];
        }

        private bool IsDayBgmName(string clipName)
        {
            return clipName.Contains("day")
                || clipName.Contains("dawn")
                || clipName.Contains("hearth")
                || clipName.Contains("vetranatto")
                || clipName.Contains("cinderkeep_bgm");
        }

        private bool IsNightBgmName(string clipName)
        {
            return clipName.Contains("night")
                || clipName.Contains("siege")
                || clipName.Contains("ashes")
                || clipName.Contains("frostvegr")
                || clipName.Contains("last ember");
        }

        private bool IsRewardBgmName(string clipName)
        {
            return clipName.Contains("dawn")
                || clipName.Contains("hearth")
                || clipName.Contains("last ember");
        }

        private bool IsBossBgmName(string clipName)
        {
            return clipName.Contains("siege")
                || clipName.Contains("frostvegr")
                || clipName.Contains("ashes");
        }

        private AudioClip LoadSfxClip(string resourcePath)
        {
            AudioClip audioClip = Resources.Load<AudioClip>(resourcePath);
            if (audioClip == null)
            {
                Debug.LogWarning("SoundManager: SFX clip was not found. path=" + resourcePath);
            }

            return audioClip;
        }

        private bool IsOreLikeResource(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return false;
            }

            return resourceId == PlayerModel.ResourceStone
                || resourceId == PlayerModel.ResourceIron
                || resourceId == PlayerModel.ResourceIronOre
                || resourceId == PlayerModel.ResourceGold
                || resourceId == PlayerModel.ResourceGoldOre
                || resourceId == PlayerModel.ResourceAdamantium
                || resourceId == PlayerModel.ResourceAdamantiumOre;
        }
    }
}
