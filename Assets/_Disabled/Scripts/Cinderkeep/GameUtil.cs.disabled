using System;
using UnityEngine;

namespace OODong.Cinderkeep
{
    public static class GameUtil
    {
        public static string FormatClock(float seconds)
        {
            int clampedSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            int minutes = clampedSeconds / 60;
            int remainder = clampedSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        public static CinderkeepItemId ParseItemId(string value, CinderkeepItemId fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return Enum.TryParse(value, true, out CinderkeepItemId itemId) ? itemId : fallback;
        }

        public static Vector3 GetRandomRingPosition(Vector3 center, float radius)
        {
            Vector2 direction = UnityEngine.Random.insideUnitCircle;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector2.right;
            }

            direction.Normalize();
            return center + new Vector3(direction.x * radius, 0f, direction.y * radius);
        }
    }
}
