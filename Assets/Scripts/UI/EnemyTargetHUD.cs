using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 최근 피격된 적의 체력을 화면 상단에 표시하는 HUD입니다.
// EnemyStatus가 피격 사실을 알려주면 이 컴포넌트가 짧은 시간 동안 체력바를 보여줍니다.
public sealed class EnemyTargetHUD : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("최근 피격된 적 HP HUD의 표시와 숨김을 제어합니다.")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Health UI")]
    [Tooltip("최근 피격된 적 체력 비율을 표시하는 Slider입니다.")]
    [SerializeField] private Slider _healthSlider;
    [Tooltip("최근 피격된 적 체력을 숫자로 표시하는 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text _healthText;

    [Header("Visibility")]
    [Tooltip("적을 공격한 뒤 HP HUD를 유지할 시간입니다.")]
    [SerializeField] private float _visibleSeconds = 3f;

    private EnemyStatus _currentEnemyStatus;
    private float _hideTimer;

    private void OnEnable()
    {
        EnemyStatus.EnemyDamaged += ShowEnemyStatus;
        HideHUD();
    }

    private void OnDisable()
    {
        EnemyStatus.EnemyDamaged -= ShowEnemyStatus;
    }

    private void Update()
    {
        RefreshCurrentEnemy();
        UpdateHideTimer();
    }

    public void ShowEnemyStatus(EnemyStatus enemyStatus)
    {
        if (enemyStatus == null)
        {
            return;
        }

        _currentEnemyStatus = enemyStatus;
        _hideTimer = _visibleSeconds;
        RefreshHUD(enemyStatus.GetCurrentHealth(), enemyStatus.GetMaxHealth());
        ShowHUD();
    }

    private void RefreshCurrentEnemy()
    {
        if (_currentEnemyStatus == null)
        {
            return;
        }

        RefreshHUD(_currentEnemyStatus.GetCurrentHealth(), _currentEnemyStatus.GetMaxHealth());
    }

    private void RefreshHUD(float currentHealth, float maxHealth)
    {
        RefreshSlider(currentHealth, maxHealth);
        RefreshText(currentHealth, maxHealth);
    }

    private void RefreshSlider(float currentHealth, float maxHealth)
    {
        if (_healthSlider == null)
        {
            return;
        }

        _healthSlider.minValue = 0f;
        _healthSlider.maxValue = Mathf.Max(1f, maxHealth);
        _healthSlider.value = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void RefreshText(float currentHealth, float maxHealth)
    {
        if (_healthText == null)
        {
            return;
        }

        _healthText.text = "Enemy HP " + currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
    }

    private void UpdateHideTimer()
    {
        if (_hideTimer <= 0f)
        {
            return;
        }

        _hideTimer -= Time.deltaTime;
        if (_hideTimer <= 0f)
        {
            HideHUD();
        }
    }

    private void ShowHUD()
    {
        SetVisible(true);
    }

    private void HideHUD()
    {
        SetVisible(false);
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
}
