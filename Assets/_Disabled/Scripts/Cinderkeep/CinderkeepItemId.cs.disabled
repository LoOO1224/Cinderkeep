namespace OODong.Cinderkeep
{
    public enum CinderkeepItemId
    {
        None = 0,
        Pickaxe = 1,
        Stone = 2,
        Ore = 3,
        Apple = 4,
        Arrow = 5
    }

    public static class CinderkeepItemCatalog
    {
        public static string GetDisplayName(CinderkeepItemId itemId)
        {
            switch (itemId)
            {
                case CinderkeepItemId.Pickaxe:
                    return "Pickaxe";
                case CinderkeepItemId.Stone:
                    return "Stone";
                case CinderkeepItemId.Ore:
                    return "Ore";
                case CinderkeepItemId.Apple:
                    return "Apple";
                case CinderkeepItemId.Arrow:
                    return "Arrow";
                default:
                    return "Empty";
            }
        }

        public static bool CanAssignQuickSlot(CinderkeepItemId itemId)
        {
            return itemId != CinderkeepItemId.None;
        }
    }
}
