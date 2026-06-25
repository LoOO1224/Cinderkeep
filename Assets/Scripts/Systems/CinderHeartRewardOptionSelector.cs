using System;
using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 아침 보상 3택 후보를 고르는 순수 선택 로직입니다.
    // GameFlowController는 페이즈 전환만 맡고, 보상 필터와 가중치 선택은 이 클래스가 담당합니다.
    public sealed class CinderHeartRewardOptionSelector
    {
        private readonly string[] _fallbackSkillIds;
        private readonly int _optionCount;

        public CinderHeartRewardOptionSelector(string[] fallbackSkillIds, int optionCount)
        {
            _fallbackSkillIds = fallbackSkillIds;
            _optionCount = Math.Max(1, optionCount);
        }

        public List<CinderHeartSkillData> SelectOptions(
            GameDataManager gameDataManager,
            int currentDay,
            bool isPlayerDead)
        {
            List<CinderHeartSkillData> skillOptions = new List<CinderHeartSkillData>();
            if (gameDataManager == null)
            {
                return skillOptions;
            }

            List<CinderHeartSkillData> candidates = GetImplementedMorningRewardCandidates(gameDataManager, currentDay, isPlayerDead);
            AddRequiredReviveRewardIfNeeded(candidates, skillOptions, isPlayerDead);
            if (isPlayerDead && skillOptions.Count > 0)
            {
                return skillOptions;
            }

            PickWeightedSkillOptions(candidates, skillOptions);

            if (skillOptions.Count > 0)
            {
                return skillOptions;
            }

            AddFallbackMorningRewardSkillOptions(gameDataManager, skillOptions);
            return skillOptions;
        }

        private List<CinderHeartSkillData> GetImplementedMorningRewardCandidates(
            GameDataManager gameDataManager,
            int currentDay,
            bool isPlayerDead)
        {
            List<CinderHeartSkillData> candidates = new List<CinderHeartSkillData>();
            if (gameDataManager.CinderHeartSkillDataList == null)
            {
                return candidates;
            }

            foreach (KeyValuePair<string, CinderHeartSkillData> pair in gameDataManager.CinderHeartSkillDataList)
            {
                CinderHeartSkillData skillData = pair.Value;
                if (CanUseAsMorningReward(skillData, currentDay, isPlayerDead))
                {
                    candidates.Add(skillData);
                }
            }

            return candidates;
        }

        private bool CanUseAsMorningReward(CinderHeartSkillData skillData, int currentDay, bool isPlayerDead)
        {
            if (skillData == null)
            {
                return false;
            }

            if (skillData.RequiredDay > currentDay)
            {
                return false;
            }

            if (IsReviveReward(skillData) && isPlayerDead == false)
            {
                return false;
            }

            return GameDataCheckRules.IsImplementedCinderHeartRewardEffect(skillData.EffectType);
        }

        private void AddRequiredReviveRewardIfNeeded(
            List<CinderHeartSkillData> candidates,
            List<CinderHeartSkillData> skillOptions,
            bool isPlayerDead)
        {
            if (isPlayerDead == false || candidates == null || skillOptions == null)
            {
                return;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                CinderHeartSkillData skillData = candidates[i];
                if (IsReviveReward(skillData))
                {
                    skillOptions.Add(skillData);
                    candidates.RemoveAt(i);
                    return;
                }
            }
        }

        private void PickWeightedSkillOptions(List<CinderHeartSkillData> candidates, List<CinderHeartSkillData> skillOptions)
        {
            if (candidates == null || skillOptions == null)
            {
                return;
            }

            while (skillOptions.Count < _optionCount && candidates.Count > 0)
            {
                CinderHeartSkillData pickedSkill = RandomUtil.PickWeighted(candidates, skillData => skillData.Weight);
                if (pickedSkill == null)
                {
                    return;
                }

                skillOptions.Add(pickedSkill);
                candidates.Remove(pickedSkill);
            }
        }

        private void AddFallbackMorningRewardSkillOptions(
            GameDataManager gameDataManager,
            List<CinderHeartSkillData> skillOptions)
        {
            if (gameDataManager == null || _fallbackSkillIds == null || skillOptions == null)
            {
                return;
            }

            for (int i = 0; i < _fallbackSkillIds.Length && skillOptions.Count < _optionCount; i++)
            {
                CinderHeartSkillData skillData = gameDataManager.GetCinderHeartSkill(_fallbackSkillIds[i]);
                if (skillData != null && GameDataCheckRules.IsImplementedCinderHeartRewardEffect(skillData.EffectType))
                {
                    skillOptions.Add(skillData);
                }
            }
        }

        private bool IsReviveReward(CinderHeartSkillData skillData)
        {
            return skillData != null
                && string.Equals(
                    skillData.EffectType,
                    GameDataCheckRules.RewardEffectPlayerReviveRate,
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}
