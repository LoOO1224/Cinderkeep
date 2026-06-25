using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 각 데이터 카탈로그가 공통으로 쓰는 null-safe ID 조회 helper입니다.
    // GameDataManager는 허브 역할을 유지하고, 반복 조회 코드는 이곳에서 통일합니다.
    public static class GameDataCatalogLookup
    {
        public static TData GetById<TData>(IReadOnlyDictionary<string, TData> source, string id)
            where TData : GameDataBase
        {
            if (source == null || string.IsNullOrEmpty(id))
            {
                return null;
            }

            TData data;
            if (source.TryGetValue(id, out data) == false)
            {
                return null;
            }

            return data;
        }
    }
}
