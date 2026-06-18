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
    [FormerlySerializedAs("_targetPlayerStatus")]
    [SerializeField] private PlayerStatus PlayerStatus_Target;

    [Header("Health UI")]
    [FormerlySerializedAs("_healthSlider")]
    [SerializeField] private Slider Slider_Health;
    [FormerlySerializedAs("_healthCurrentText")]
    [SerializeField] private TMP_Text Text_HealthCurrent;
    [FormerlySerializedAs("_healthMaxText")]
    [SerializeField] private TMP_Text Text_HealthMax;

    [Header("Stamina UI")]
    [FormerlySerializedAs("_staminaSlider")]
    [SerializeField] private Slider Slider_Stamina;
    [FormerlySerializedAs("_staminaCurrentText")]
    [SerializeField] private TMP_Text Text_StaminaCurrent;
    [FormerlySerializedAs("_staminaMaxText")]
    [SerializeField] private TMP_Text Text_StaminaMax;

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
        PlayerStatus_Target = playerStatus;
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        if (HasPlayerStatus() == false)
        {
            return;
        }

        InitializeSlider(Slider_Health, PlayerStatus_Target.GetMaxHealth(), PlayerStatus_Target.GetCurrentHealth());
        InitializeSlider(Slider_Stamina, PlayerStatus_Target.GetMaxStamina(), PlayerStatus_Target.GetCurrentStamina());
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (HasPlayerStatus() == false)
        {
            return;
        }

        RefreshSlider(Slider_Health, PlayerStatus_Target.GetCurrentHealth());
        RefreshSlider(Slider_Stamina, PlayerStatus_Target.GetCurrentStamina());

        RefreshText(Text_HealthCurrent, PlayerStatus_Target.GetCurrentHealth());
        RefreshText(Text_HealthMax, PlayerStatus_Target.GetMaxHealth());
        RefreshText(Text_StaminaCurrent, PlayerStatus_Target.GetCurrentStamina());
        RefreshText(Text_StaminaMax, PlayerStatus_Target.GetMaxStamina());
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
        return PlayerStatus_Target != null;
    }
}
