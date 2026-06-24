using System;
using Cinderkeep.Gameplay;
using UnityEngine;

public sealed class CinderHeartSkillApplier : MonoBehaviour
{
    private const string EffectTypeAttackDamageAdd = "CinderHeartAttackDamageAdd";
    private const string EffectTypeMaxHealthAdd = "CinderHeartMaxHealthAdd";
    private const string EffectTypePlayerHealRate = "PlayerHealRate";
    private const string EffectTypePlayerReviveRate = "PlayerReviveRate";

    [SerializeField] private CinderHeart _cinderHeart;
    [SerializeField] private PlayerStatus _playerStatus;

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

        if (IsEffectType(skillData, EffectTypeAttackDamageAdd))
        {
            ApplyCinderHeartAttackDamageAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, EffectTypeMaxHealthAdd))
        {
            ApplyCinderHeartMaxHealthAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, EffectTypePlayerHealRate))
        {
            ApplyPlayerHealRate(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, EffectTypePlayerReviveRate))
        {
            ApplyPlayerReviveRate(skillData.Value);
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
