using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // JSON Resources 로드와 ID 등록 규칙을 모든 데이터 카탈로그에서 공유하는 로더입니다.
    // 빈 ID와 중복 ID를 경고로 남겨 데이터가 조용히 깨지는 상황을 줄입니다.
    public static class GameDataCatalogLoader
    {
        public static void LoadCatalog<TData, TCatalog>(
            Dictionary<string, TData> target,
            string resourcePath,
            string catalogName,
            Func<TCatalog, List<TData>> getItems)
            where TData : GameDataBase
        {
            if (target == null)
            {
                Debug.LogWarning("GameDataCatalogLoader: target dictionary is null for " + catalogName + ".");
                return;
            }

            target.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: " + catalogName + " JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            TCatalog catalog = JsonUtility.FromJson<TCatalog>(jsonAsset.text);
            List<TData> items = catalog == null || getItems == null ? null : getItems(catalog);
            if (items == null)
            {
                Debug.LogWarning("GameDataManager: " + catalogName + " JSON format is invalid.");
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                AddData(target, items[i], catalogName, i);
            }
        }

        private static void AddData<TData>(Dictionary<string, TData> target, TData data, string catalogName, int index)
            where TData : GameDataBase
        {
            if (target == null || data == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(data.Id))
            {
                Debug.LogWarning("GameDataCatalogLoader: empty id skipped. catalog=" + catalogName + ", index=" + index);
                return;
            }

            if (target.ContainsKey(data.Id))
            {
                Debug.LogWarning("GameDataCatalogLoader: duplicate id overwritten. catalog=" + catalogName + ", id=" + data.Id);
                target[data.Id] = data;
                return;
            }

            target.Add(data.Id, data);
        }
    }
}
