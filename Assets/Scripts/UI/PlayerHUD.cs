using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// 플레이어의 체력과 스태미나를 화면에 표시하는 HUD 컴포넌트입니다.
// 실제 수치 계산은 PlayerStatus가 담당하고, 이 클래스는 UI 갱신만 담당합니다.
public sealed class PlayerHUD : MonoBehaviour
{
    [Header("Connected Components")]
    [FormerlySerializedAs("PlayerStatus_Target")]
    [SerializeField] private PlayerStatus _targetPlayerStatus;

    [Header("Health UI")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _healthCurrentText;
    [SerializeField] private TMP_Text _healthMaxText;

    [Header("Stamina UI")]
    [SerializeField] private Slider _staminaSlider;
    [SerializeField] private TMP_Text _staminaCurrentText;
    [SerializeField] private TMP_Text _staminaMaxText;

    [Header("Satiety UI")]
    [SerializeField] private Slider _satietySlider;
    [SerializeField] private TMP_Text _satietyCurrentText;
    [SerializeField] private TMP_Text _satietyMaxText;

    private void Start()
    {
        InitializeHUD();
    }

    private void Update()
    {
        RefreshHUD();
    }

    public void SetPlayerStatus(PlayerStatus playerStatus)
    {
        _targetPlayerStatus = playerStatus;
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        if (HasPlayerStatus() == false)
        {
            return;
        }

        InitializeSlider(_healthSlider, _targetPlayerStatus.GetMaxHealth(), _targetPlayerStatus.GetCurrentHealth());
        InitializeSlider(_staminaSlider, _targetPlayerStatus.GetMaxStamina(), _targetPlayerStatus.GetCurrentStamina());
        InitializeSlider(_satietySlider, _targetPlayerStatus.GetMaxSatiety(), _targetPlayerStatus.GetCurrentSatiety());

        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (HasPlayerStatus() == false)
        {
            return;
        }

        RefreshSlider(_healthSlider, _targetPlayerStatus.GetCurrentHealth());
        RefreshSlider(_staminaSlider, _targetPlayerStatus.GetCurrentStamina());
        RefreshSlider(_satietySlider, _targetPlayerStatus.GetCurrentSatiety());


        RefreshText(_healthCurrentText, _targetPlayerStatus.GetCurrentHealth());
        RefreshText(_healthMaxText, _targetPlayerStatus.GetMaxHealth());

        RefreshText(_staminaCurrentText, _targetPlayerStatus.GetCurrentStamina());
        RefreshText(_staminaMaxText, _targetPlayerStatus.GetMaxStamina());

        RefreshText(_satietyCurrentText, _targetPlayerStatus.GetCurrentSatiety());
        RefreshText(_satietyMaxText, _targetPlayerStatus.GetMaxSatiety());



    }

    private void InitializeSlider(Slider slider, float maxValue, float currentValue)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = maxValue;
        slider.value = currentValue;
    }

    private void RefreshSlider(Slider slider, float currentValue)
    {
        if (slider == null)
        {
            return;
        }

        slider.value = currentValue;
    }

    private void RefreshText(TMP_Text text, float value)
    {
        if (text == null)
        {
            return;
        }

        text.text = value.ToString("F0");
    }

    private bool HasPlayerStatus()
    {
        return _targetPlayerStatus != null;
    }
}
