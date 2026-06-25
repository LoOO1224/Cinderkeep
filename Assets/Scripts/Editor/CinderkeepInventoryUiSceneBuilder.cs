using Cinderkeep.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
// Cinderkeep_Game 씬에 인벤토리/장비 UI 기준 오브젝트를 배치하는 에디터 도구입니다.
// 런타임에는 UI를 새로 만들지 않고, 이 도구로 미리 만든 오브젝트를 켜고 끄는 방식으로 사용합니다.
public static class CinderkeepInventoryUiSceneBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF3.asset";
    private const string CanvasName = "Canvas_GameHUD";

    [MenuItem("Cinderkeep/Main Game/Apply Inventory UI")]
    public static void ApplyInventoryUI()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        Canvas canvas = GetComponentInScene<Canvas>(CanvasName);
        UIManager uiManager = GetComponentInScene<UIManager>("UIManager");
        if (canvas == null || uiManager == null)
        {
            Debug.LogWarning("CinderkeepInventoryUiSceneBuilder: Canvas_GameHUD 또는 UIManager를 찾지 못했습니다.");
            return;
        }

        EnsureCanvasInput(canvas);
        EnsureEventSystem();

        bool isInventoryRootCreated;
        RectTransform inventoryRoot = GetOrCreateInventoryRoot(canvas.transform, out isInventoryRootCreated);
        if (isInventoryRootCreated)
        {
            ClearChildren(inventoryRoot);
            SetupInventoryRoot(inventoryRoot);
            BuildInventoryContent(inventoryRoot, uiManager);
        }
        else
        {
            SetupInventoryReferences(inventoryRoot, uiManager);
        }

        inventoryRoot.gameObject.SetActive(false);
        EditorUtility.SetDirty(inventoryRoot.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CinderkeepInventoryUiSceneBuilder: inventory UI setup finished.");
    }

    private static void EnsureCanvasInput(Canvas canvas)
    {
        GraphicRaycaster raycaster = GetOrAddComponent<GraphicRaycaster>(canvas.gameObject);
        raycaster.ignoreReversedGraphics = true;
    }

    private static void EnsureEventSystem()
    {
        GameObject eventSystemObject = GetSceneObjectByName("EventSystem_GameUI");
        if (eventSystemObject == null)
        {
            eventSystemObject = new GameObject("EventSystem_GameUI");
        }

        GetOrAddComponent<EventSystem>(eventSystemObject);
        InputSystemUIInputModule inputModule = GetOrAddComponent<InputSystemUIInputModule>(eventSystemObject);
        inputModule.AssignDefaultActions();
        EditorUtility.SetDirty(eventSystemObject);
    }

    private static RectTransform GetOrCreateInventoryRoot(Transform canvasTransform, out bool shouldRebuild)
    {
        Transform inventoryRoot = canvasTransform.Find("Panel_InventoryRoot");
        if (inventoryRoot != null)
        {
            shouldRebuild = false;
            return inventoryRoot.GetComponent<RectTransform>();
        }

        inventoryRoot = canvasTransform.Find("Panel_InventoryRoot_Disabled");
        if (inventoryRoot != null)
        {
            inventoryRoot.name = "Panel_InventoryRoot";
            shouldRebuild = true;
            return inventoryRoot.GetComponent<RectTransform>();
        }

        GameObject inventoryRootObject = new GameObject("Panel_InventoryRoot");
        inventoryRootObject.transform.SetParent(canvasTransform);
        RectTransform rectTransform = inventoryRootObject.AddComponent<RectTransform>();
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        shouldRebuild = true;
        return rectTransform;
    }

    private static void SetupInventoryRoot(RectTransform inventoryRoot)
    {
        inventoryRoot.anchorMin = new Vector2(0.5f, 0.5f);
        inventoryRoot.anchorMax = new Vector2(0.5f, 0.5f);
        inventoryRoot.pivot = new Vector2(0.5f, 0.5f);
        inventoryRoot.anchoredPosition = Vector2.zero;
        inventoryRoot.sizeDelta = new Vector2(980f, 580f);
        inventoryRoot.localScale = Vector3.one;

        Image backgroundImage = GetOrAddComponent<Image>(inventoryRoot.gameObject);
        backgroundImage.color = new Color(0.03f, 0.04f, 0.05f, 0.92f);
    }

    private static void BuildInventoryContent(RectTransform inventoryRoot, UIManager uiManager)
    {
        TMP_Text titleText = CreateText(inventoryRoot, "Text_InventoryTitle", "Inventory / Equipment", 28f, TextAlignmentOptions.Center);
        SetRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(900f, 40f), new Vector2(0f, -34f));

        TMP_Text messageText = CreateText(inventoryRoot, "Text_InventoryMessage", "인벤토리 아이템을 장비 칸이나 퀵슬롯으로 드래그하세요.", 18f, TextAlignmentOptions.Center);
        SetRect(messageText.rectTransform, new Vector2(0.5f, 0f), new Vector2(900f, 30f), new Vector2(0f, 24f));

        RectTransform bodyPanel = CreatePanel(inventoryRoot, "Panel_EquipmentBody", new Color(0.09f, 0.11f, 0.13f, 0.92f));
        SetRect(bodyPanel, new Vector2(0f, 0.5f), new Vector2(330f, 430f), new Vector2(190f, 12f));

        RectTransform inventoryPanel = CreatePanel(inventoryRoot, "Panel_InventorySlots", new Color(0.07f, 0.09f, 0.11f, 0.92f));
        SetRect(inventoryPanel, new Vector2(1f, 0.55f), new Vector2(560f, 360f), new Vector2(-300f, -2f));

        RectTransform quickSlotPanel = CreatePanel(inventoryRoot, "Panel_QuickSlotRoot", new Color(0.07f, 0.10f, 0.12f, 0.92f));
        SetRect(quickSlotPanel, new Vector2(0.5f, 0f), new Vector2(710f, 82f), new Vector2(0f, 72f));

        EquipmentSlotView[] equipmentSlotViews = BuildEquipmentSlots(bodyPanel);
        InventorySlotView[] inventorySlotViews = BuildInventorySlots(inventoryPanel);
        QuickSlotView[] quickSlotViews = BuildQuickSlots(quickSlotPanel);

        InventoryUI inventoryUI = GetOrAddComponent<InventoryUI>(inventoryRoot.gameObject);
        SerializedObject serializedInventoryUI = new SerializedObject(inventoryUI);
        serializedInventoryUI.FindProperty("_rootObject").objectReferenceValue = inventoryRoot.gameObject;
        serializedInventoryUI.FindProperty("_titleText").objectReferenceValue = titleText;
        serializedInventoryUI.FindProperty("_messageText").objectReferenceValue = messageText;
        SetObjectArray(serializedInventoryUI.FindProperty("_equipmentSlotViews"), equipmentSlotViews);
        SetObjectArray(serializedInventoryUI.FindProperty("_inventorySlotViews"), inventorySlotViews);
        SetObjectArray(serializedInventoryUI.FindProperty("_quickSlotViews"), quickSlotViews);
        serializedInventoryUI.ApplyModifiedPropertiesWithoutUndo();

        SetupUIManager(uiManager, inventoryRoot.gameObject, inventoryUI);
    }

    private static void SetupInventoryReferences(RectTransform inventoryRoot, UIManager uiManager)
    {
        InventoryUI inventoryUI = inventoryRoot.GetComponent<InventoryUI>();
        if (inventoryUI == null)
        {
            return;
        }

        SetupUIManager(uiManager, inventoryRoot.gameObject, inventoryUI);
    }

    private static EquipmentSlotView[] BuildEquipmentSlots(RectTransform bodyPanel)
    {
        TMP_Text guideText = CreateText(bodyPanel, "Text_EquipmentGuide", "Equipment", 22f, TextAlignmentOptions.Center);
        SetRect(guideText.rectTransform, new Vector2(0.5f, 1f), new Vector2(280f, 34f), new Vector2(0f, -28f));

        EquipmentSlotView[] slotViews = new EquipmentSlotView[4];
        slotViews[0] = CreateEquipmentSlot(bodyPanel, "Slot_Equipment_Helmet", EquipmentSlotType.Helmet, new Vector2(0f, 116f));
        slotViews[1] = CreateEquipmentSlot(bodyPanel, "Slot_Equipment_Armor", EquipmentSlotType.Armor, new Vector2(0f, -6f));
        slotViews[2] = CreateEquipmentSlot(bodyPanel, "Slot_Equipment_Weapon", EquipmentSlotType.Weapon, new Vector2(-108f, -6f));
        slotViews[3] = CreateEquipmentSlot(bodyPanel, "Slot_Equipment_Boots", EquipmentSlotType.Boots, new Vector2(0f, -132f));
        return slotViews;
    }

    private static EquipmentSlotView CreateEquipmentSlot(RectTransform parent, string objectName, EquipmentSlotType slotType, Vector2 anchoredPosition)
    {
        RectTransform slotRoot = CreatePanel(parent, objectName, new Color(0.11f, 0.11f, 0.13f, 0.9f));
        SetRect(slotRoot, new Vector2(0.5f, 0.5f), new Vector2(96f, 74f), anchoredPosition);

        TMP_Text slotText = CreateText(slotRoot, "Text_" + objectName, "", 15f, TextAlignmentOptions.Center);
        SetRect(slotText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(88f, 64f), Vector2.zero);

        EquipmentSlotView slotView = GetOrAddComponent<EquipmentSlotView>(slotRoot.gameObject);
        SerializedObject serializedSlot = new SerializedObject(slotView);
        serializedSlot.FindProperty("_slotType").enumValueIndex = (int)slotType;
        serializedSlot.FindProperty("_slotText").objectReferenceValue = slotText;
        serializedSlot.FindProperty("_backgroundImage").objectReferenceValue = slotRoot.GetComponent<Image>();
        serializedSlot.ApplyModifiedPropertiesWithoutUndo();
        return slotView;
    }

    private static InventorySlotView[] BuildInventorySlots(RectTransform inventoryPanel)
    {
        GridLayoutGroup gridLayout = GetOrAddComponent<GridLayoutGroup>(inventoryPanel.gameObject);
        gridLayout.cellSize = new Vector2(84f, 58f);
        gridLayout.spacing = new Vector2(8f, 8f);
        gridLayout.padding = new RectOffset(20, 20, 20, 20);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 6;

        InventorySlotView[] slotViews = new InventorySlotView[PlayerInventoryModel.InventorySlotCount];
        for (int i = 0; i < slotViews.Length; i++)
        {
            string slotName = "Slot_Inventory_" + i.ToString("00");
            RectTransform slotRoot = CreatePanel(inventoryPanel, slotName, new Color(0.10f, 0.13f, 0.16f, 0.78f));
            TMP_Text itemText = CreateText(slotRoot, "Text_" + slotName, "", 12f, TextAlignmentOptions.Center);
            SetRect(itemText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(78f, 52f), Vector2.zero);

            InventorySlotView slotView = GetOrAddComponent<InventorySlotView>(slotRoot.gameObject);
            SerializedObject serializedSlot = new SerializedObject(slotView);
            serializedSlot.FindProperty("_itemText").objectReferenceValue = itemText;
            serializedSlot.FindProperty("_backgroundImage").objectReferenceValue = slotRoot.GetComponent<Image>();
            serializedSlot.ApplyModifiedPropertiesWithoutUndo();
            slotViews[i] = slotView;
        }

        return slotViews;
    }

    private static QuickSlotView[] BuildQuickSlots(RectTransform quickSlotPanel)
    {
        HorizontalLayoutGroup horizontalLayout = GetOrAddComponent<HorizontalLayoutGroup>(quickSlotPanel.gameObject);
        horizontalLayout.padding = new RectOffset(14, 14, 10, 10);
        horizontalLayout.spacing = 8f;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childControlWidth = true;
        horizontalLayout.childForceExpandHeight = true;
        horizontalLayout.childForceExpandWidth = true;

        QuickSlotView[] slotViews = new QuickSlotView[PlayerInventoryModel.QuickSlotCount];
        for (int i = 0; i < slotViews.Length; i++)
        {
            string slotName = "Button_QuickSlot_" + (i + 1).ToString();
            RectTransform slotRoot = CreatePanel(quickSlotPanel, slotName, new Color(0.08f, 0.10f, 0.12f, 0.86f));
            GetOrAddComponent<Button>(slotRoot.gameObject);
            TMP_Text slotText = CreateText(slotRoot, "Text_" + slotName, "", 13f, TextAlignmentOptions.Center);
            SetRect(slotText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(82f, 54f), Vector2.zero);

            QuickSlotView slotView = GetOrAddComponent<QuickSlotView>(slotRoot.gameObject);
            SerializedObject serializedSlot = new SerializedObject(slotView);
            serializedSlot.FindProperty("_slotIndex").intValue = i;
            serializedSlot.FindProperty("_slotText").objectReferenceValue = slotText;
            serializedSlot.FindProperty("_backgroundImage").objectReferenceValue = slotRoot.GetComponent<Image>();
            serializedSlot.ApplyModifiedPropertiesWithoutUndo();
            slotViews[i] = slotView;
        }

        return slotViews;
    }

    private static RectTransform CreatePanel(Transform parent, string objectName, Color color)
    {
        GameObject panelObject = new GameObject(objectName);
        panelObject.transform.SetParent(parent);
        panelObject.transform.localRotation = Quaternion.identity;
        panelObject.transform.localScale = Vector3.one;

        RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
        Image image = panelObject.AddComponent<Image>();
        image.color = color;
        return rectTransform;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent);
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = alignment;
        tmpText.color = Color.white;
        tmpText.raycastTarget = false;
        ApplyTmpFont(tmpText);
        return tmpText;
    }

    private static void SetupUIManager(UIManager uiManager, GameObject inventoryRoot, InventoryUI inventoryUI)
    {
        SerializedObject serializedObject = new SerializedObject(uiManager);
        serializedObject.FindProperty("_inventoryRoot").objectReferenceValue = inventoryRoot;
        serializedObject.FindProperty("_inventoryUI").objectReferenceValue = inventoryUI;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(uiManager);
    }

    private static void SetObjectArray<TComponent>(SerializedProperty arrayProperty, TComponent[] components)
        where TComponent : Object
    {
        arrayProperty.arraySize = components.Length;
        for (int i = 0; i < components.Length; i++)
        {
            SerializedProperty itemProperty = arrayProperty.GetArrayElementAtIndex(i);
            itemProperty.objectReferenceValue = components[i];
        }
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchor, Vector2 size, Vector2 anchoredPosition)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = anchor;
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
    }

    private static void ApplyTmpFont(TMP_Text text)
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset == null)
        {
            return;
        }

        text.font = fontAsset;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static T GetOrAddComponent<T>(GameObject targetObject)
        where T : Component
    {
        T component = targetObject.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return targetObject.AddComponent<T>();
    }

    private static T GetComponentInScene<T>(string objectName)
        where T : Component
    {
        GameObject sceneObject = GetSceneObjectByName(objectName);
        if (sceneObject == null)
        {
            return null;
        }

        return sceneObject.GetComponent<T>();
    }

    private static GameObject GetSceneObjectByName(string objectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject result = GetChildByName(rootObjects[i].transform, objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static GameObject GetChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform.name == objectName)
        {
            return rootTransform.gameObject;
        }

        for (int i = 0; i < rootTransform.childCount; i++)
        {
            GameObject result = GetChildByName(rootTransform.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
