using Cinderkeep.Gameplay;
using UnityEngine;

// CinderHeart 기준 동서남북에 함정 건축 스팟을 보정합니다.
// 실제 감속 효과는 플레이어가 함정을 제작/건축한 뒤 BuildingManager가 붙입니다.
public static class TrapZoneSceneBootstrap
{
    private const string RootName = "TrapBuildSpots_5_86";
    private const string TrapBuildingDataId = "wood_slow_trap";

    public static void EnsureTrapZones()
    {
        DisableExistingTrapZones();
        return;
    }

    private static void DisableExistingTrapZones()
    {
        GameObject existingRoot = GameObject.Find(RootName);
        if (existingRoot != null)
        {
            existingRoot.SetActive(false);
        }
    }

    public static void EnsureTrapZonesForLater()
    {
        Transform root = GetOrCreateRoot();
        Vector3 center = ResolveCenterPosition();

        CreateOrKeepTrapSpot(root, "BuildSpot_Trap_North", center + new Vector3(0f, 0.02f, 8.4f), new Vector3(4.2f, 0.08f, 2.4f));
        CreateOrKeepTrapSpot(root, "BuildSpot_Trap_South", center + new Vector3(0f, 0.02f, -8.4f), new Vector3(4.2f, 0.08f, 2.4f));
        CreateOrKeepTrapSpot(root, "BuildSpot_Trap_East", center + new Vector3(8.4f, 0.02f, 0f), new Vector3(2.4f, 0.08f, 4.2f));
        CreateOrKeepTrapSpot(root, "BuildSpot_Trap_West", center + new Vector3(-8.4f, 0.02f, 0f), new Vector3(2.4f, 0.08f, 4.2f));
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

    private static void CreateOrKeepTrapSpot(Transform root, string trapName, Vector3 position, Vector3 scale)
    {
        Transform existing = root.Find(trapName);
        if (existing != null)
        {
            ConfigureTrapSpot(existing.gameObject);
            return;
        }

        GameObject trapObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trapObject.name = trapName;
        trapObject.transform.SetParent(root);
        trapObject.transform.position = position;
        trapObject.transform.localScale = scale;

        BoxCollider boxCollider = trapObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
        }

        Renderer renderer = trapObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            RuntimePrimitiveMaterial.ApplyColor(trapObject, new Color(0.95f, 0.18f, 0.08f, 0.22f), "MAT_Runtime_TrapBuildSpot");
            renderer.enabled = false;
        }

        ConfigureTrapSpot(trapObject);
    }

    private static void ConfigureTrapSpot(GameObject trapObject)
    {
        if (trapObject == null)
        {
            return;
        }

        Renderer renderer = trapObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        BuildingSpot buildingSpot = trapObject.GetComponent<BuildingSpot>();
        if (buildingSpot == null)
        {
            buildingSpot = trapObject.AddComponent<BuildingSpot>();
        }

        buildingSpot.ConfigureBuildingDataId(TrapBuildingDataId);
        RegisterToBuildingManager(buildingSpot);
    }

    private static void RegisterToBuildingManager(BuildingSpot buildingSpot)
    {
        if (buildingSpot == null || GameManager.Inst == null)
        {
            return;
        }

        BuildingManager buildingManager = GameManager.Inst.GetBuildingManager();
        if (buildingManager == null)
        {
            return;
        }

        buildingManager.RegisterBuildingSpot(buildingSpot);
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
