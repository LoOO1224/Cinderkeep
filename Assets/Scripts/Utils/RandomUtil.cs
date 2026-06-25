using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 가중치 랜덤처럼 여러 시스템에서 반복될 수 있는 선택 로직을 모아둡니다.
    // 보상, 드롭, 스폰 후보 선택은 이 유틸을 통해 중복 없는 흐름으로 확장합니다.
    public static class RandomUtil
    {
        public static T PickWeighted<T>(IReadOnlyList<T> candidates, Func<T, int> getWeight) where T : class
        {
            if (candidates == null || candidates.Count <= 0 || getWeight == null)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                T candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                totalWeight += Mathf.Max(1, getWeight(candidate));
            }

            if (totalWeight <= 0)
            {
                return candidates[0];
            }

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                T candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                currentWeight += Mathf.Max(1, getWeight(candidate));
                if (randomWeight < currentWeight)
                {
                    return candidate;
                }
            }

            return candidates[candidates.Count - 1];
        }
    }
}
