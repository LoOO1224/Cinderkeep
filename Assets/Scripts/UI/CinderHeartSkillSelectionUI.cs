using System;
using System.Collections.Generic;
using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 밤을 넘긴 뒤 CinderHeart 보상 스킬 3개를 보여주는 UI입니다.
// UI 표시와 버튼 선택만 담당하고, 실제 효과 적용은 CinderHeartSkillApplier에 위임합니다.
public sealed class CinderHeartSkillSelectionUI : MonoBehaviour
{
    public static event Action<CinderHeartSkillData> SkillSelectedGlobal;

    [SerializeField] private GameObject _rootObject;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private CinderHeartSkillOptionView[] _optionViews;
    [SerializeField] private Button _skipButton;
    [SerializeField] private CinderHeartSkillApplier _skillApplier;

    private Action _onClosed;
    private bool _isInitialized;
    private bool _isOpen;

    public void Initialize()
    {
        if (_isInitialized == true)
        {
            return;
        }

        if (_skipButton != null)
        {
            _skipButton.onClick.AddListener(HandleSkipButtonClicked);
        }

        if (_optionViews != null)
        {
            for (int i = 0; i < _optionViews.Length; i++)
            {
                if (_optionViews[i] != null)
                {
                    _optionViews[i].Initialize();
                }
            }
        }

        Close();
        _isInitialized = true;
    }

    public void Open(IReadOnlyList<CinderHeartSkillData> skillOptions, Action onClosed)
    {
        Initialize();
        _onClosed = onClosed;
        _isOpen = true;

        if (_titleText != null)
        {
            _titleText.text = "셋 중에 하나를 고르시오";
        }

        ApplySkillOptions(skillOptions);
        SetRootActive(true);
    }

    public void Close()
    {
        _isOpen = false;
        SetRootActive(false);
    }

    public void SelectSkill(CinderHeartSkillData skillData)
    {
        if (_isOpen == false)
        {
            return;
        }

        if (_skillApplier != null)
        {
            _skillApplier.ApplySkill(skillData);
        }

        NotifySkillSelected(skillData);
        PlayRewardSelectSfx();
        CloseAndNotify();
    }

    private void ApplySkillOptions(IReadOnlyList<CinderHeartSkillData> skillOptions)
    {
        // GameFlowController가 넘긴 CinderHeartSkillData를 3개의 선택지 UI에만 표시합니다.
        // 실제 수치 적용은 CinderHeartSkillApplier가 맡으므로 이 UI는 표시와 선택 전달만 담당합니다.
        if (_optionViews == null)
        {
            return;
        }

        for (int i = 0; i < _optionViews.Length; i++)
        {
            if (_optionViews[i] == null)
            {
                continue;
            }

            if (skillOptions != null && i < skillOptions.Count)
            {
                _optionViews[i].SetSkill(skillOptions[i], this);
            }
            else
            {
                _optionViews[i].Clear();
            }
        }
    }

    private void HandleSkipButtonClicked()
    {
        if (_isOpen == false)
        {
            return;
        }

        PlayUiBackSfx();
        CloseAndNotify();
    }

    private void CloseAndNotify()
    {
        Action onClosed = _onClosed;
        _onClosed = null;
        Close();

        if (onClosed != null)
        {
            onClosed.Invoke();
        }
    }

    private void SetRootActive(bool isActive)
    {
        if (_rootObject != null)
        {
            _rootObject.SetActive(isActive);
            return;
        }

        gameObject.SetActive(isActive);
    }

    private void PlayRewardSelectSfx()
    {
        SoundManager soundManager = GetSoundManager();
        if (soundManager == null)
        {
            return;
        }

        soundManager.PlayRewardSelect();
    }

    private void NotifySkillSelected(CinderHeartSkillData skillData)
    {
        if (SkillSelectedGlobal == null)
        {
            return;
        }

        SkillSelectedGlobal(skillData);
    }

    private void PlayUiBackSfx()
    {
        SoundManager soundManager = GetSoundManager();
        if (soundManager == null)
        {
            return;
        }

        soundManager.PlayUiBack();
    }

    private SoundManager GetSoundManager()
    {
        if (GameManager.Inst == null)
        {
            return null;
        }

        return GameManager.Inst.GetSoundManager();
    }
}
