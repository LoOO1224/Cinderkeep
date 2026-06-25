using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이 상태를 화면에 표시하거나 사용자의 UI 요청을 전달합니다.
// UI는 규칙을 소유하지 않고 모델을 읽고 시스템에 요청을 보내는 계층으로 유지합니다.
// 베이스 귀환 시전 시간을 화면에 표시하는 UI 컴포넌트입니다.
// UI 오브젝트는 씬이나 프리팹에서 미리 만들고, 이 컴포넌트는 표시 값만 갱신합니다.
public sealed class BaseTeleportProgressUI : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("귀환 UI 전체 표시와 숨김을 담당할 루트 오브젝트입니다. 비워두면 이 GameObject를 사용합니다.")]
    [SerializeField] private GameObject _rootObject;
    [Tooltip("알파와 클릭 차단을 제어할 CanvasGroup입니다.")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Progress UI")]
    [Tooltip("귀환 진행률을 0~1 비율로 표시하는 Slider입니다.")]
    [SerializeField] private Slider _progressSlider;
    [Tooltip("귀환 제목 텍스트입니다.")]
    [SerializeField] private TMP_Text _titleText;
    [Tooltip("남은 시간과 진행률을 표시하는 텍스트입니다.")]
    [SerializeField] private TMP_Text _progressText;

    [Header("Text")]
    [Tooltip("귀환 중 표시할 제목입니다.")]
    [SerializeField] private string _title = "베이스 귀환 중";
    [Tooltip("시전이 끊길 수 있음을 알려주는 보조 문구입니다.")]
    [SerializeField] private string _cancelGuide = "피격 시 취소";

    private float _durationSeconds = 3f;

    private void Awake()
    {
        InitializeSlider();
        Close();
    }

    public void Open(float durationSeconds)
    {
        _durationSeconds = Mathf.Max(durationSeconds, 0.1f);
        SetVisible(true);
        SetProgress(0f);
    }

    public void Close()
    {
        SetProgress(0f);
        SetVisible(false);
    }

    public void SetProgress(float progress01)
    {
        float progress = Mathf.Clamp01(progress01);
        RefreshSlider(progress);
        RefreshText(progress);
    }

    private void InitializeSlider()
    {
        if (_progressSlider == null)
        {
            return;
        }

        _progressSlider.minValue = 0f;
        _progressSlider.maxValue = 1f;
        _progressSlider.value = 0f;
    }

    private void RefreshSlider(float progress)
    {
        if (_progressSlider == null)
        {
            return;
        }

        _progressSlider.value = progress;
    }

    private void RefreshText(float progress)
    {
        if (_titleText != null)
        {
            _titleText.text = _title + "  |  " + _cancelGuide;
        }

        if (_progressText == null)
        {
            return;
        }

        float remainingSeconds = Mathf.Max(0f, _durationSeconds * (1f - progress));
        int progressPercent = Mathf.RoundToInt(progress * 100f);
        _progressText.text = progressPercent.ToString() + "%  /  " + remainingSeconds.ToString("F1") + "초";
    }

    private void SetVisible(bool isVisible)
    {
        GameObject targetRoot = GetRootObject();
        if (targetRoot != null && targetRoot.activeSelf != isVisible)
        {
            targetRoot.SetActive(isVisible);
        }

        if (_canvasGroup == null)
        {
            return;
        }

        if (isVisible == true)
        {
            _canvasGroup.alpha = 1f;
        }
        else
        {
            _canvasGroup.alpha = 0f;
        }

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private GameObject GetRootObject()
    {
        if (_rootObject != null)
        {
            return _rootObject;
        }

        return gameObject;
    }
}
