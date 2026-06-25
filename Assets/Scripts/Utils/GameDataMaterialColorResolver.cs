using System;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // JSON materialKey를 런타임 fallback 색상으로 바꾸는 작은 resolver입니다.
    // 실제 Material 에셋이 없어도 건축물과 자원 티어가 눈에 구분되도록 유지합니다.
    public static class GameDataMaterialColorResolver
    {
        public static bool TryResolveColor(string materialKey, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrEmpty(materialKey))
            {
                return false;
            }

            if (EqualsKey(materialKey, "MAT_4_0_Wood") || EqualsKey(materialKey, "MAT_4_0_Tree_Wood"))
            {
                color = new Color(0.42f, 0.26f, 0.14f);
                return true;
            }

            if (EqualsKey(materialKey, "MAT_4_0_Tool_Metal") || EqualsKey(materialKey, "MAT_4_0_Tree_Frostwood"))
            {
                color = new Color(0.45f, 0.58f, 0.68f);
                return true;
            }

            if (EqualsKey(materialKey, "MAT_4_0_GoldOre") || EqualsKey(materialKey, "MAT_4_0_Tree_Cinderwood"))
            {
                color = new Color(0.95f, 0.74f, 0.22f);
                return true;
            }

            if (EqualsKey(materialKey, "MAT_4_0_Rock_AdamantiumOre") || EqualsKey(materialKey, "MAT_4_0_Tree_Heartwood"))
            {
                color = new Color(0.58f, 0.28f, 0.82f);
                return true;
            }

            if (EqualsKey(materialKey, "MAT_4_0_Stone") || EqualsKey(materialKey, "MAT_4_0_Rock_Stone"))
            {
                color = new Color(0.46f, 0.48f, 0.50f);
                return true;
            }

            if (EqualsKey(materialKey, "MAT_4_0_Rock_IronOre"))
            {
                color = new Color(0.38f, 0.49f, 0.55f);
                return true;
            }

            return false;
        }

        private static bool EqualsKey(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }
}
