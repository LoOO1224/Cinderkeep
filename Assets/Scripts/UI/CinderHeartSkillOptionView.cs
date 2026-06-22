using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// CinderHeart 스킬 선택창의 선택지 한 칸을 표시합니다.
// 클릭 처리만 부모 UI로 넘기고, 효과 적용은 직접 하지 않습니다.
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
        // cinderheart_skills.json에서 온 이름/설명을 그대로 표시합니다.
        // 텍스트를 바꾸고 싶으면 스크립트가 아니라 JSON의 displayName, description을 수정합니다.
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
