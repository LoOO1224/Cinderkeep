using UnityEngine;

// 씬에 손돌 픽업을 보장하고 매일 같은 위치에 다시 활성화하는 런타임 부트스트랩입니다.
// 시작 주변 3개와 맵 전역 24개를 만들어 초반 채집 루프가 끊기지 않게 합니다.
public static class HandStonePickupSceneBootstrap
{
    private const string LegacyHandStonePickupName = "Pickup_HandStone_5_00";
    private const string StartPickupNamePrefix = "Pickup_HandStone_Start_";
    private const string WorldPickupNamePrefix = "Pickup_HandStone_World_";
    private const int StartPickupCount = 3;
    private const int WorldPickupCount = 24;

    private static readonly Vector3 PickupScale = new Vector3(0.42f, 0.24f, 0.42f);
    private static readonly Color PickupColor = new Color(0.42f, 0.44f, 0.46f, 1f);

    public static void EnsureHandStonePickup()
    {
        EnsureHandStonePickups();
    }

    public static void EnsureHandStonePickups()
    {
        RemoveLegacySinglePickup();
        EnsureStartPickups();
        EnsureWorldPickups();
    }

    public static void ResetDailyPickups()
    {
        EnsureHandStonePickups();
        ResetPickupGroup(StartPickupNamePrefix, StartPickupCount);
        ResetPickupGroup(WorldPickupNamePrefix, WorldPickupCount);
    }

    private static void EnsureStartPickups()
    {
        Vector3[] offsets =
        {
            new Vector3(1.6f, 0f, 2.1f),
            new Vector3(-1.4f, 0f, 2.7f),
            new Vector3(2.4f, 0f, -0.8f)
        };

        Transform playerTransform = FindPlayerTransform();
        Vector3 center = playerTransform == null ? Vector3.zero : playerTransform.position;
        Vector3 forward = playerTransform == null ? Vector3.forward : playerTransform.forward;
        Vector3 right = playerTransform == null ? Vector3.right : playerTransform.right;

        for (int i = 0; i < StartPickupCount; i++)
        {
            string pickupName = StartPickupNamePrefix + (i + 1).ToString("00");
            Vector3 offset = right * offsets[i].x + forward * offsets[i].z;
            EnsurePickup(pickupName, ProjectToGround(center + offset));
        }
    }

    private static void EnsureWorldPickups()
    {
        Vector3 center = FindWorldPickupCenter();
        for (int i = 0; i < WorldPickupCount; i++)
        {
            string pickupName = WorldPickupNamePrefix + (i + 1).ToString("00");
            Vector3 offset = GetWorldPickupOffset(i);
            EnsurePickup(pickupName, ProjectToGround(center + offset));
        }
    }

    private static void EnsurePickup(string pickupName, Vector3 position)
    {
        HandStonePickup pickup = FindScenePickup(pickupName);
        if (pickup != null)
        {
            ApplyPickupMaterial(pickup.gameObject);
            pickup.ResetPickup();
            return;
        }

        CreatePickup(pickupName, position);
    }

    private static void ResetPickupGroup(string pickupNamePrefix, int count)
    {
        for (int i = 0; i < count; i++)
        {
            HandStonePickup pickup = FindScenePickup(pickupNamePrefix + (i + 1).ToString("00"));
            if (pickup != null)
            {
                pickup.ResetPickup();
            }
        }
    }

    private static HandStonePickup FindScenePickup(string pickupName)
    {
        HandStonePickup[] pickups = Object.FindObjectsByType<HandStonePickup>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < pickups.Length; i++)
        {
            HandStonePickup pickup = pickups[i];
            if (pickup == null)
            {
                continue;
            }

            if (pickup.gameObject.scene.IsValid() == false)
            {
                continue;
            }

            if (pickup.gameObject.name == pickupName)
            {
                return pickup;
            }
        }

        return null;
    }

    private static void CreatePickup(string pickupName, Vector3 position)
    {
        GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickupObject.name = pickupName;
        pickupObject.transform.position = position;
        pickupObject.transform.localScale = PickupScale;

        ApplyPickupMaterial(pickupObject);
        pickupObject.AddComponent<HandStonePickup>();
    }

    private static Vector3 FindWorldPickupCenter()
    {
        Transform cinderHeartTransform = FindCinderHeartTransform();
        if (cinderHeartTransform != null)
        {
            return cinderHeartTransform.position;
        }

        Transform playerTransform = FindPlayerTransform();
        if (playerTransform != null)
        {
            return playerTransform.position;
        }

        return Vector3.zero;
    }

    private static Transform FindCinderHeartTransform()
    {
        GameObject cinderHeartObject = null;
        try
        {
            cinderHeartObject = GameObject.FindGameObjectWithTag("CinderHeart");
        }
        catch (UnityException)
        {
            cinderHeartObject = GameObject.Find("CinderHeart");
        }

        return cinderHeartObject == null ? null : cinderHeartObject.transform;
    }

    private static Transform FindPlayerTransform()
    {
        PlayerToolController toolController = Object.FindFirstObjectByType<PlayerToolController>();
        if (toolController != null)
        {
            return toolController.transform;
        }

        GameObject playerObject = GameObject.Find("Player");
        return playerObject == null ? null : playerObject.transform;
    }

    private static Vector3 GetWorldPickupOffset(int index)
    {
        float radius = 10f + (index % 4) * 7f + (index / 8) * 4f;
        float angle = index * 137.5f * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
    }

    private static Vector3 ProjectToGround(Vector3 rawPosition)
    {
        Ray ray = new Ray(rawPosition + Vector3.up * 12f, Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 30f))
        {
            return hitInfo.point + Vector3.up * 0.14f;
        }

        rawPosition.y = 0.18f;
        return rawPosition;
    }

    private static void ApplyPickupMaterial(GameObject pickupObject)
    {
        Renderer renderer = pickupObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        RuntimePrimitiveMaterial.ApplyColor(pickupObject, PickupColor, "MAT_Runtime_HandStone");
    }

    private static void RemoveLegacySinglePickup()
    {
        HandStonePickup legacyPickup = FindScenePickup(LegacyHandStonePickupName);
        if (legacyPickup == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(legacyPickup.gameObject);
            return;
        }

        Object.DestroyImmediate(legacyPickup.gameObject);
    }
}
