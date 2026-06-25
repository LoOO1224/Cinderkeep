using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// CinderHeart 보상 선택창의 단일 선택지를 표시하는 View입니다.
// JSON의 표시명/설명을 그대로 보여주고, 클릭 이벤트는 부모 UI에만 전달합니다.
public sealed class CinderHeartSkillOptionView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;

    private CinderHeartSkillSelectionUI _owner;
    private CinderHeartSkillData _skillData;
    private bool _isInitialized;

    public void Initialize()
    {
        if (_isInitialized == true)
        {
            return;
        }

        if (_button != null)
        {
            _button.onClick.AddListener(HandleButtonClicked);
        }

        _isInitialized = true;
    }

    public void SetSkill(CinderHeartSkillData skillData, CinderHeartSkillSelectionUI owner)
    {
        Initialize();
        _skillData = skillData;
        _owner = owner;

        if (_nameText != null)
        {
            _nameText.text = skillData != null ? skillData.DisplayName : "";
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = skillData != null ? skillData.Description : "";
        }

        if (_button != null)
        {
            _button.interactable = skillData != null;
        }
    }

    public void Clear()
    {
        _skillData = null;
        _owner = null;

        if (_nameText != null)
        {
            _nameText.text = "";
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = "";
        }

        if (_button != null)
        {
            _button.interactable = false;
        }
    }

    private void HandleButtonClicked()
    {
        if (_owner == null)
        {
            return;
        }

        _owner.SelectSkill(_skillData);
    }
}
