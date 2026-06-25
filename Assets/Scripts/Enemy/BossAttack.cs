using Cinderkeep.Gameplay;
using System.Collections.Generic;
using UnityEngine;

// 적의 감지, 타깃 판단, 이동, 공격, 보스 클리어 연결 중 한 역할을 담당합니다.
// AI 판단과 실제 행동 컴포넌트를 분리해 적 패턴이 늘어도 유지보수 가능하게 합니다.
public sealed class BossAttack : MonoBehaviour
{
    private readonly Dictionary<string, float> _patternCooldowns = new Dictionary<string, float>();
    private GameDataManager _gameDataManager;

    private float _lastBasicAttackTime;

    public void Initialize(GameDataManager gameDataManager)
    {
        _gameDataManager = gameDataManager;
        _patternCooldowns.Clear();
    }

    public bool TryBasicAttack(string bossId, Damageable target)
    {
        if (target == null || _gameDataManager == null)
        {
            return false;
        }

        BossData bossData = _gameDataManager.GetBoss(bossId);
        if (bossData == null)
        {
            return false;
        }

        if (Time.time < _lastBasicAttackTime + bossData.AttackInterval)
        {
            return false;
        }

        target.TakeDamage(bossData.AttackDamage);
        _lastBasicAttackTime = Time.time;

        return true;
    }

    public bool TryPatternAttack(string patternId, Damageable target)
    {
        if (target == null || _gameDataManager == null)
        {
            return false;
        }

        BossPatternData bossPatternData = _gameDataManager.GetBossPattern(patternId);
        if (bossPatternData == null)
        {
            return false;
        }

        if (IsPatternOnCooldown(patternId, bossPatternData.Cooldown))
        {
            return false;
        }

        target.TakeDamage(bossPatternData.Damage);
        PlayPatternEffect(bossPatternData.EffectKey);

        _patternCooldowns[bossPatternData.Id] = Time.time;

        return true;
    }

    private bool IsPatternOnCooldown(string patternId, float cooldown)
    {
        if (_patternCooldowns.TryGetValue(patternId, out float lastTime))
        {
            return Time.time < lastTime + cooldown;
        }

        return false;
    }

    private void PlayPatternEffect(string effectKey)
    {
        if (string.IsNullOrEmpty(effectKey))
        {
            return;
        }

        // 추후 스킬 이펙트 호출
    }
}
