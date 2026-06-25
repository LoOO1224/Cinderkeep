using UnityEngine;

// 순록 드롭이 붙기 전까지 생고기 테스트 픽업을 배치합니다.
// 매일 아침 다시 활성화해 음식/가열/섭취 루프를 Check할 수 있게 합니다.
public static class FoodPickupSceneBootstrap
{
    private const string RootName = "FoodPickups_5_11";
    private static readonly Vector3[] LocalOffsets =
    {
        new Vector3(3.5f, 0.25f, 2.5f),
        new Vector3(-3.5f, 0.25f, 2.5f),
        new Vector3(2.5f, 0.25f, -3.5f)
    };

    public static void EnsureFoodPickups()
    {
        Transform root = GetOrCreateRoot();
        Vector3 center = ResolveCenterPosition();

        for (int i = 0; i < LocalOffsets.Length; i++)
        {
            string pickupName = "Pickup_RawMeat_" + (i + 1).ToString("00");
            Transform existing = root.Find(pickupName);
            if (existing != null)
            {
                ConfigurePickup(existing.gameObject);
                continue;
            }

            CreatePickup(root, pickupName, center + LocalOffsets[i]);
        }
    }

    public static void ResetDailyFoodPickups()
    {
        EnsureFoodPickups();
        GameObject root = GameObject.Find(RootName);
        if (root == null)
        {
            return;
        }

        FoodPickup[] pickups = root.GetComponentsInChildren<FoodPickup>(true);
        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i] != null)
            {
                pickups[i].ResetPickup();
            }
        }
    }

    private static Transform GetOrCreateRoot()
    {
        GameObject root = GameObject.Find(RootName);
        if (root == null)
        {
            root = new GameObject(RootName);
        }

        return root.transform;
    }

    private static void CreatePickup(Transform root, string pickupName, Vector3 position)
    {
        GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickupObject.name = pickupName;
        pickupObject.transform.SetParent(root);
        pickupObject.transform.position = position;
        pickupObject.transform.localScale = new Vector3(0.45f, 0.22f, 0.32f);
        pickupObject.AddComponent<FoodPickup>();
        ConfigurePickup(pickupObject);
    }

    private static void ConfigurePickup(GameObject pickupObject)
    {
        if (pickupObject == null)
        {
            return;
        }

        pickupObject.transform.localScale = new Vector3(0.45f, 0.16f, 0.32f);
        RuntimePrimitiveMaterial.ApplyColor(pickupObject, new Color(0.38f, 0.08f, 0.07f, 1f), "MAT_Runtime_RawMeatPickup");
        Renderer renderer = pickupObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    private static Vector3 ResolveCenterPosition()
    {
        CinderHeart cinderHeart = Object.FindFirstObjectByType<CinderHeart>();
        if (cinderHeart != null)
        {
            return cinderHeart.transform.position;
        }

        return Vector3.zero;
    }
}
