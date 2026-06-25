using System;
using System.Collections.Generic;
using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// CinderHeart 아침 보상 3택을 보여주고 선택 이벤트만 전달하는 UI입니다.
// 실제 효과 적용은 CinderHeartSkillApplier가 담당하므로 이 클래스는 표시와 버튼 흐름만 관리합니다.
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
    private bool _canSkipCurrentSelection;

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
            _titleText.text = "불씨 보상 하나를 선택하세요";
        }

        bool hasSelectableOptions = HasSelectableOptions(skillOptions);
        _canSkipCurrentSelection = hasSelectableOptions == false;
        SetSkipButtonVisible(_canSkipCurrentSelection);
        ApplySkillOptions(skillOptions);
        SetRootActive(true);
    }

    public void Close()
    {
        _isOpen = false;
        _canSkipCurrentSelection = false;
        SetRootActive(false);
    }

    public void SelectSkill(CinderHeartSkillData skillData)
    {
        if (_isOpen == false || skillData == null)
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

    private bool HasSelectableOptions(IReadOnlyList<CinderHeartSkillData> skillOptions)
    {
        if (skillOptions == null)
        {
            return false;
        }

        for (int i = 0; i < skillOptions.Count; i++)
        {
            if (skillOptions[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplySkillOptions(IReadOnlyList<CinderHeartSkillData> skillOptions)
    {
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
        if (_isOpen == false || _canSkipCurrentSelection == false)
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

    private void SetSkipButtonVisible(bool isVisible)
    {
        if (_skipButton == null)
        {
            return;
        }

        _skipButton.gameObject.SetActive(isVisible);
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
