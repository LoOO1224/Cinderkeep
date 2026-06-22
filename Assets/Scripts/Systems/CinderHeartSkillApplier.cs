using System;
using Cinderkeep.Gameplay;
using UnityEngine;

// CinderHeart 보상 스킬의 실제 효과를 적용하는 컴포넌트입니다.
// GameFlow와 UI는 선택 흐름만 맡고, 수치 적용은 이 컴포넌트가 담당합니다.
public sealed class CinderHeartSkillApplier : MonoBehaviour
{
    private const string EffectTypeAttackDamageAdd = "CinderHeartAttackDamageAdd";
    private const string EffectTypeMaxHealthAdd = "CinderHeartMaxHealthAdd";
    private const string EffectTypePlayerHealRate = "PlayerHealRate";

    [SerializeField] private CinderHeart _cinderHeart;
    [SerializeField] private PlayerStatus _playerStatus;

    public void SetTargets(CinderHeart cinderHeart, PlayerStatus playerStatus)
    {
        _cinderHeart = cinderHeart;
        _playerStatus = playerStatus;
    }

    public void ApplySkill(CinderHeartSkillData skillData)
    {
        // cinderheart_skills.json의 EffectType 문자열을 기준으로 실제 효과를 분기합니다.
        // 새 스킬을 추가할 때는 JSON의 effectType과 여기의 적용 메서드를 함께 맞춰야 합니다.
        if (skillData == null)
        {
            return;
        }

        if (IsEffectType(skillData, EffectTypeAttackDamageAdd) == true)
        {
            ApplyCinderHeartAttackDamageAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, EffectTypeMaxHealthAdd) == true)
        {
            ApplyCinderHeartMaxHealthAdd(skillData.Value);
            return;
        }

        if (IsEffectType(skillData, EffectTypePlayerHealRate) == true)
        {
            ApplyPlayerHealRate(skillData.Value);
            return;
        }

        Debug.LogWarning("[CinderHeartSkillApplier] 아직 연결되지 않은 스킬 효과입니다: " + skillData.EffectType);
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
        if (_playerStatus == null)
        {
            return;
        }

        if (rate <= 0f)
        {
            return;
        }

        float healAmount = _playerStatus.GetMaxHealth() * rate;
        _playerStatus.Heal(healAmount);
        Debug.Log("[CinderHeartSkillApplier] 플레이어 체력 회복: +" + healAmount);
    }
}
