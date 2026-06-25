using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이 상태를 화면에 표시하거나 사용자의 UI 요청을 전달합니다.
// UI는 규칙을 소유하지 않고 모델을 읽고 시스템에 요청을 보내는 계층으로 유지합니다.
// CinderHeart 체력을 화면 상단에 표시하는 HUD 컴포넌트입니다.
// CinderHeart는 게임의 핵심 방어 대상이므로 게임 시작부터 계속 표시합니다.
public sealed class CinderHeartHUD : MonoBehaviour
{
    [Header("Connected Component")]
    [Tooltip("체력을 표시할 CinderHeart 컴포넌트입니다.")]
    [SerializeField] private CinderHeart _targetCinderHeart;

    [Header("Root")]
    [Tooltip("HUD 표시와 숨김을 제어하는 CanvasGroup입니다.")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Health UI")]
    [Tooltip("CinderHeart 체력 비율을 표시하는 Slider입니다.")]
    [SerializeField] private Slider _healthSlider;
    [Tooltip("CinderHeart 체력을 숫자로 표시하는 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text _healthText;

    [Header("Visibility")]
    [Tooltip("체력이 줄지 않아도 항상 표시할지 결정합니다.")]
    [SerializeField] private bool _isAlwaysVisible = true;

    private void Start()
    {
        InitializeHUD();
    }

    private void Update()
    {
        RefreshHUD();
    }

    public void SetCinderHeart(CinderHeart cinderHeart)
    {
        _targetCinderHeart = cinderHeart;
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        if (HasCinderHeart() == false)
        {
            SetVisible(false);
            return;
        }

        InitializeSlider();
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (HasCinderHeart() == false)
        {
            SetVisible(false);
            return;
        }

        float currentHealth = _targetCinderHeart.GetCurrentHealth();
        float maxHealth = _targetCinderHeart.GetMaxHealth();

        RefreshSlider(currentHealth, maxHealth);
        RefreshText(currentHealth, maxHealth);
        RefreshVisibility(currentHealth, maxHealth);
    }

    private void InitializeSlider()
    {
        if (_healthSlider == null)
        {
            return;
        }

        _healthSlider.minValue = 0f;
        _healthSlider.maxValue = _targetCinderHeart.GetMaxHealth();
        _healthSlider.value = _targetCinderHeart.GetCurrentHealth();
    }

    private void RefreshSlider(float currentHealth, float maxHealth)
    {
        if (_healthSlider == null)
        {
            return;
        }

        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = currentHealth;
    }

    private void RefreshText(float currentHealth, float maxHealth)
    {
        if (_healthText == null)
        {
            return;
        }

        _healthText.text = "CinderHeart HP " + currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
    }

    private void RefreshVisibility(float currentHealth, float maxHealth)
    {
        if (_isAlwaysVisible == true)
        {
            SetVisible(true);
            return;
        }

        SetVisible(currentHealth < maxHealth);
    }

    private void SetVisible(bool isVisible)
    {
        if (_canvasGroup == null)
        {
            return;
        }

        if (isVisible)
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

    private bool HasCinderHeart()
    {
        return _targetCinderHeart != null;
    }
}
