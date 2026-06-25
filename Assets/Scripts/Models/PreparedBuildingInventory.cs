using System;
using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 제작 완료 후 아직 설치되지 않은 건축물 수량을 보관합니다.
    // PlayerInventoryModel은 슬롯 조작을 맡고, 건축 준비 수량 규칙은 이 클래스가 담당합니다.
    [Serializable]
    public sealed class PreparedBuildingInventory
    {
        private readonly Dictionary<string, int> _buildingCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void Clear()
        {
            _buildingCounts.Clear();
        }

        public bool TryAdd(string buildingId, int amount)
        {
            if (string.IsNullOrEmpty(buildingId) || amount <= 0)
            {
                return false;
            }

            _buildingCounts[buildingId] = GetCount(buildingId) + amount;
            return true;
        }

        public bool Has(string buildingId)
        {
            return GetCount(buildingId) > 0;
        }

        public int GetCount(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                return 0;
            }

            int amount;
            if (_buildingCounts.TryGetValue(buildingId, out amount) == false)
            {
                return 0;
            }

            return Math.Max(0, amount);
        }

        public bool TryConsume(string buildingId, int amount)
        {
            if (string.IsNullOrEmpty(buildingId) || amount <= 0)
            {
                return false;
            }

            int currentAmount = GetCount(buildingId);
            if (currentAmount < amount)
            {
                return false;
            }

            int nextAmount = currentAmount - amount;
            if (nextAmount <= 0)
            {
                _buildingCounts.Remove(buildingId);
                return true;
            }

            _buildingCounts[buildingId] = nextAmount;
            return true;
        }
    }
}
