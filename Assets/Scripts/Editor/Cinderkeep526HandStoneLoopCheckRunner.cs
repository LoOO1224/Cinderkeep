using System;
using System.Reflection;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Unity play mode에서 손돌 픽업, 퀵슬롯 등록, 오른손 표시, 채집 증가를 한 번에 확인합니다.
// 에디터 전용 Check 러너이므로 실제 빌드 로직에는 포함되지 않습니다.
[InitializeOnLoad]
public static class Cinderkeep526HandStoneLoopCheckRunner
{
    private const string MenuPath = "Cinderkeep/5.26 Check/Run Hand Stone Loop Check";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string PendingKey = "Cinderkeep526HandStoneLoopCheck.Pending";
    private const string FrameKey = "Cinderkeep526HandStoneLoopCheck.Frame";
    private const int MaxWaitFrames = 360;

    static Cinderkeep526HandStoneLoopCheckRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem(MenuPath)]
    public static void RunHandStonePickupGatherCheck()
    {
        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        SessionState.SetBool(PendingKey, true);
        SessionState.SetInt(FrameKey, 0);

        if (EditorApplication.isPlaying)
        {
            return;
        }

        EditorApplication.EnterPlaymode();
    }

    private static void OnEditorUpdate()
    {
        if (SessionState.GetBool(PendingKey, false) == false)
        {
            return;
        }

        if (EditorApplication.isPlaying == false)
        {
            return;
        }

        int frame = SessionState.GetInt(FrameKey, 0) + 1;
        SessionState.SetInt(FrameKey, frame);

        GameManager gameManager = GameManager.Inst;
        if ((gameManager == null || gameManager.IsInitialized == false) && frame < MaxWaitFrames)
        {
            return;
        }

        try
        {
            RunPlayModeCheck();
            Complete(true);
        }
        catch (Exception exception)
        {
            Debug.LogError("[Cinderkeep 5.26] hand stone loop check failed.\n" + exception);
            Complete(false);
        }
    }

    private static void RunPlayModeCheck()
    {
        GameManager gameManager = Require(GameManager.Inst, "GameManager.Inst");
        Require(gameManager.IsInitialized, "GameManager initialized");

        PlayerToolController toolController = Require(UnityEngine.Object.FindFirstObjectByType<PlayerToolController>(), "PlayerToolController");
        PlayerToolUse toolUse = Require(toolController.GetComponent<PlayerToolUse>(), "PlayerToolUse");
        FirstPersonToolView toolView = Require(toolController.GetComponentInChildren<FirstPersonToolView>(true), "FirstPersonToolView");
        Transform toolOrigin = ResolveToolOrigin(toolController);

        HandStonePickupSceneBootstrap.EnsureHandStonePickup();
        HandStonePickup pickup = Require(UnityEngine.Object.FindFirstObjectByType<HandStonePickup>(FindObjectsInactive.Exclude), "active HandStonePickup");
        pickup.transform.position = toolController.transform.position + toolController.transform.forward * 1.2f + Vector3.up * 0.25f;

        int stoneBeforePickup = gameManager.PlayerModel.GetResourceAmount(PlayerModel.ResourceStone);
        pickup.Interact(toolController.gameObject);

        InventoryItemModel firstQuickSlot = gameManager.PlayerInventoryModel.GetQuickSlotItem(0);
        Require(firstQuickSlot != null && firstQuickSlot.IsEmpty == false, "quick slot 1 item");
        Require(firstQuickSlot.ItemId == PlayerToolController.HandStoneToolDataId, "hand stone in quick slot 1");
        Require(firstQuickSlot.ItemType == InventoryItemType.Tool, "hand stone item type");
        Require(toolController.CurrentToolDataId == PlayerToolController.HandStoneToolDataId, "hand stone equipped");
        Require(gameManager.PlayerModel.GetResourceAmount(PlayerModel.ResourceStone) > stoneBeforePickup, "pickup grants stone resource");

        RefreshToolView(toolView);
        Require(HasActiveHandStoneView(toolView.transform), "visible right-hand hand stone view");

        int stoneBeforeGather = gameManager.PlayerModel.GetResourceAmount(PlayerModel.ResourceStone);
        ResourceNode testResourceNode = CreateTestStoneNode(toolOrigin);
        ForceToolUseConnections(toolUse, toolOrigin, toolController, toolView);
        ForceToolUseReady(toolUse);
        ToolData currentToolData = toolController.GetCurrentToolData();
        ResourceNode castResourceNode = FindCastResourceNode(toolUse, currentToolData);
        Require(castResourceNode != null, BuildGatherDebugMessage(
            "tool cast did not find ResourceNode",
            toolController,
            currentToolData,
            toolOrigin,
            testResourceNode,
            false));
        toolUse.TryUseTool();

        int stoneAfterGather = gameManager.PlayerModel.GetResourceAmount(PlayerModel.ResourceStone);
        bool directGatherWouldWork = false;
        if (stoneAfterGather <= stoneBeforeGather)
        {
            directGatherWouldWork = testResourceNode.TryGatherWithTool(
                toolController.gameObject,
                toolController.CurrentToolType,
                currentToolData);
        }

        UnityEngine.Object.Destroy(testResourceNode.gameObject);
        Require(stoneAfterGather > stoneBeforeGather, BuildGatherDebugMessage(
            "left-click hand stone gathering did not increase stone",
            toolController,
            currentToolData,
            toolOrigin,
            testResourceNode,
            directGatherWouldWork));

        QuickSlotHud quickSlotHud = Require(QuickSlotHud.EnsureSceneHud(), "QuickSlotHud");
        quickSlotHud.RefreshNow();
        Require(quickSlotHud.gameObject.activeInHierarchy, "QuickSlotHud visible");

        Debug.Log("[Cinderkeep 5.26] hand stone loop check passed.");
    }

    private static Transform ResolveToolOrigin(PlayerToolController toolController)
    {
        Camera camera = toolController.GetComponentInChildren<Camera>();
        if (camera != null)
        {
            return camera.transform;
        }

        return toolController.transform;
    }

    private static ResourceNode CreateTestStoneNode(Transform toolOrigin)
    {
        GameObject resourceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        resourceObject.name = "Check_HandStoneGather_StoneNode";
        resourceObject.transform.position = toolOrigin.position + toolOrigin.forward * 1.35f;
        resourceObject.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        RuntimePrimitiveMaterial.ApplyColor(resourceObject, new Color(0.38f, 0.42f, 0.45f, 1f), "MAT_Check_StoneNode");

        ResourceNode resourceNode = resourceObject.AddComponent<ResourceNode>();
        SetPrivateField(resourceNode, "_useHarvestNodeData", false);
        SetPrivateField(resourceNode, "_resourceId", PlayerModel.ResourceStone);
        SetPrivateField(resourceNode, "_amount", 2);
        SetPrivateField(resourceNode, "_requiredToolType", GatherToolType.Axe);
        SetPrivateField(resourceNode, "_requiredToolTier", 1);
        SetPrivateField(resourceNode, "_disableAfterGather", false);
        SetPrivateField(resourceNode, "_canInteract", true);
        return resourceNode;
    }

    private static void ForceToolUseConnections(
        PlayerToolUse toolUse,
        Transform toolOrigin,
        PlayerToolController toolController,
        FirstPersonToolView toolView)
    {
        SetPrivateField(toolUse, "_toolOrigin", toolOrigin);
        SetPrivateField(toolUse, "_playerToolController", toolController);
        SetPrivateField(toolUse, "_firstPersonToolView", toolView);
    }

    private static void ForceToolUseReady(PlayerToolUse toolUse)
    {
        SetPrivateField(toolUse, "_lastToolUseTime", -999f);
    }

    private static ResourceNode FindCastResourceNode(PlayerToolUse toolUse, ToolData toolData)
    {
        MethodInfo methodInfo = typeof(PlayerToolUse).GetMethod("GetResourceNodeFromCast", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(methodInfo != null, "PlayerToolUse.GetResourceNodeFromCast");
        return methodInfo.Invoke(toolUse, new object[] { toolData }) as ResourceNode;
    }

    private static string BuildGatherDebugMessage(
        string reason,
        PlayerToolController toolController,
        ToolData toolData,
        Transform toolOrigin,
        ResourceNode testResourceNode,
        bool directGatherWouldWork)
    {
        string toolDataId = toolData == null ? "null" : toolData.Id;
        string toolTier = toolData == null ? "null" : toolData.Tier.ToString();
        string originPosition = toolOrigin == null ? "null" : toolOrigin.position.ToString("F3");
        string originForward = toolOrigin == null ? "null" : toolOrigin.forward.ToString("F3");
        string nodePosition = testResourceNode == null ? "null" : testResourceNode.transform.position.ToString("F3");

        return reason
            + " / currentToolDataId=" + toolController.CurrentToolDataId
            + " / currentToolType=" + toolController.CurrentToolType
            + " / toolData=" + toolDataId
            + " / toolTier=" + toolTier
            + " / origin=" + originPosition
            + " / forward=" + originForward
            + " / node=" + nodePosition
            + " / directGatherWouldWork=" + directGatherWouldWork;
    }

    private static void RefreshToolView(FirstPersonToolView toolView)
    {
        MethodInfo methodInfo = typeof(FirstPersonToolView).GetMethod("RefreshToolView", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(methodInfo != null, "FirstPersonToolView.RefreshToolView");
        methodInfo.Invoke(toolView, null);
    }

    private static bool HasActiveHandStoneView(Transform root)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child == null)
            {
                continue;
            }

            bool isConnectedHandStoneView = child.name == "GameObject_HandStoneView";
            bool isFallbackHandStoneView = child.name == "View_HandStone_Fallback";
            if (isConnectedHandStoneView == false && isFallbackHandStoneView == false)
            {
                continue;
            }

            return child.gameObject.activeInHierarchy;
        }

        return false;
    }

    private static void SetPrivateField(object targetObject, string fieldName, object value)
    {
        FieldInfo fieldInfo = targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Require(fieldInfo != null, targetObject.GetType().Name + "." + fieldName);
        fieldInfo.SetValue(targetObject, value);
    }

    private static T Require<T>(T value, string label)
        where T : class
    {
        if (value == null)
        {
            throw new InvalidOperationException(label + " is missing.");
        }

        return value;
    }

    private static void Require(bool condition, string label)
    {
        if (condition == false)
        {
            throw new InvalidOperationException(label + " check failed.");
        }
    }

    private static void Complete(bool isSuccess)
    {
        SessionState.SetBool(PendingKey, false);
        SessionState.SetInt(FrameKey, 0);
        EditorApplication.ExitPlaymode();

        if (Application.isBatchMode == false)
        {
            return;
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(isSuccess ? 0 : 1);
    }
}
