namespace Cinderkeep.Gameplay
{
    // 음식 MVP에서 사용하는 고정 ID와 회복량입니다.
    // 음식 종류가 늘어나면 이 값을 JSON 카탈로그로 분리합니다.
    public static class FoodItemIds
    {
        public const string RawMeat = "raw_meat";
        public const string CookedMeat = "cooked_meat";

        public static bool IsFoodItem(string itemId)
        {
            return itemId == RawMeat || itemId == CookedMeat;
        }

        public static float GetSatietyRestoreAmount(string itemId)
        {
            if (itemId == CookedMeat)
            {
                return 50f;
            }

            if (itemId == RawMeat)
            {
                return 25f;
            }

            return 0f;
        }
    }
}
