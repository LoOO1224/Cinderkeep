using System;
using Cinderkeep.Gameplay;
using UnityEngine;

// CinderHeart 보상 선택 결과를 실제 플레이어/CinderHeart 수치에 적용합니다.
// 적용 가능한 EffectType 기준은 GameDataCheckRules와 공유해 데이터 Check과 런타임 처리가 갈라지지 않게 합니다.
public sealed class CinderHeartSkillApplier : MonoBehaviour
{
    [SerializeField] private CinderHeart _cinderHeart;
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private PlayerAttack _playerAttack;

    public void SetTargets(CinderHeart cinderHeart, PlayerStatus playerStatus)
    {
        _cinderHeart = cinderHeart;
        _playerStatus = playerStatus;
    }

    public void ApplySkill(CinderHeartSkillData skillData)
    {
        if (skillData == null)
        {
            return;
        }

        ConnectTargetsIfNeeded();

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectCinderHeartAttackDamageAdd))
        {
            ApplyCinderHeartAttackDamageAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectCinderHeartMaxHealthAdd))
        {
            ApplyCinderHeartMaxHealthAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectCinderHeartHealFlat))
        {
            ApplyCinderHeartHealFlat(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectCinderHeartHealRate))
        {
            ApplyCinderHeartHealRate(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectPlayerHealRate))
        {
            ApplyPlayerHealRate(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectPlayerReviveRate))
        {
            ApplyPlayerReviveRate(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectPlayerMaxHealthAdd))
        {
            ApplyPlayerMaxHealthAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectPlayerMaxStaminaAdd))
        {
            ApplyPlayerMaxStaminaAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectPlayerStaminaRecoveryAdd))
        {
            ApplyPlayerStaminaRecoveryAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectPlayerMaxSatietyAdd))
        {
            ApplyPlayerMaxSatietyAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, GameDataCheckRules.RewardEffectPlayerAttackDamageAdd))
        {
            ApplyPlayerAttackDamageAdd(skillData.Value);
            return;
        }

        Debug.LogWarning("[CinderHeartSkillApplier] Unsupported reward effect: " + skillData.EffectType);
    }

    private bool IsEffectType(CinderHeartSkillData skillData, string effectType)
    {
        return string.Equals(skillData.EffectType, effectType, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyCinderHeartAttackDamageAdd(float amount)
    {
        if (_cinderHeart == null)
        {
            return;
        }

        _cinderHeart.AddAttackDamage(amount);
    }

    private void ApplyCinderHeartMaxHealthAdd(float amount)
    {
        if (_cinderHeart == null)
        {
            return;
        }

        _cinderHeart.AddMaxHealth(amount);
    }

    private void ApplyCinderHeartHealFlat(float amount)
    {
        if (_cinderHeart == null)
        {
            return;
        }

        _cinderHeart.Heal(amount);
        PlayHealSfx();
    }

    private void ApplyCinderHeartHealRate(float rate)
    {
        if (_cinderHeart == null || rate <= 0f)
        {
            return;
        }

        _cinderHeart.HealByRate(rate);
        PlayHealSfx();
    }

    private void ApplyPlayerHealRate(float rate)
    {
        if (_playerStatus == null || rate <= 0f)
        {
            return;
        }

        float healAmount = _playerStatus.GetMaxHealth() * rate;
        _playerStatus.Heal(healAmount);
        PlayHealSfx();
    }

    private void ApplyPlayerReviveRate(float rate)
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.Revive(rate);
        PlayHealSfx();
    }

    private void ApplyPlayerMaxHealthAdd(float amount)
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.AddMaxHealth(amount);
        PlayHealSfx();
    }

    private void ApplyPlayerMaxStaminaAdd(float amount)
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.AddMaxStamina(amount);
    }

    private void ApplyPlayerStaminaRecoveryAdd(float amount)
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.AddStaminaRecoveryRate(amount);
    }

    private void ApplyPlayerMaxSatietyAdd(float amount)
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.AddMaxSatiety(amount);
    }

    private void ApplyPlayerAttackDamageAdd(float amount)
    {
        if (_playerAttack == null)
        {
            return;
        }

        _playerAttack.AddBonusAttackDamage(amount);
    }

    private void ConnectTargetsIfNeeded()
    {
        if (_cinderHeart == null)
        {
            _cinderHeart = UnityEngine.Object.FindFirstObjectByType<CinderHeart>();
        }

        if (_playerStatus == null)
        {
            _playerStatus = UnityEngine.Object.FindFirstObjectByType<PlayerStatus>();
        }

        if (_playerAttack == null)
        {
            _playerAttack = UnityEngine.Object.FindFirstObjectByType<PlayerAttack>();
        }
    }

    private void PlayHealSfx()
    {
        if (GameManager.Inst == null || GameManager.Inst.GetSoundManager() == null)
        {
            return;
        }

        GameManager.Inst.GetSoundManager().PlayHeal();
    }
}
