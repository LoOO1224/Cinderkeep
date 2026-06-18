using Cinderkeep.Gameplay;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CinderkeepHierarchyOrganizer
{
    private const string ScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";

    [MenuItem("Cinderkeep/Main Game/Organize Hierarchy")]
    public static void OrganizeHierarchy()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath);
        OrganizeScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("CinderkeepHierarchyOrganizer: hierarchy organized.");
    }

    public static void OrganizeHierarchyBatch()
    {
        OrganizeHierarchy();
    }

    private static void OrganizeScene(Scene scene)
    {
        SetupLoopConnectors(scene);

        SetRootOrder(scene, new string[]
        {
            "MainGame_Managers",
            "MainGame_LoopConnector",
            "MainGame_RuntimeManagers",
            "Player",
            "CinderHeart",
            "EnemySpawnPoints",
            "MainGame_RuntimeObjects",
            "Canvas_GameHUD",
            "EnemySpawnCandidatePoints"
        });

        SetChildOrder(scene, "MainGame_LoopConnector", new string[]
        {
            "PlayerLoopConnector",
            "ResourceLoopConnector",
            "EnemyLoopConnector",
            "GameFlowLoopConnector"
        });

        SetChildOrder(scene, "MainGame_Managers", new string[]
        {
            "GameManager",
            "GameDataManager",
            "GameObjectManager",
            "UIManager",
            "SoundManager",
            "MapManager",
            "Transform_RuntimeObjectRoot"
        });

        SetChildOrder(scene, "MainGame_RuntimeManagers", new string[]
        {
            "GameFlowController"
        });

        SetChildOrder(scene, "Player", new string[]
        {
            "Transform_CameraRoot_FirstPerson",
            "PlayerVisual_FallbackCapsule",
            "Transform_GroundCheck"
        });

        SetChildOrder(scene, "CinderHeart", new string[]
        {
            "Visual_CinderHeart_Core",
            "Light_CinderHeart_Beacon"
        });

        SetChildOrder(scene, "EnemySpawnPoints", new string[]
        {
            "EnemySpawnPoint_Near_Required",
            "EnemySpawnPoint_Outer_Random"
        });

        SetChildOrder(scene, "MainGame_RuntimeObjects", new string[]
        {
            "Enemies_GameLoop",
            "Resources_GameLoop",
            "BuildingPreview_GameLoop"
        });

        SetChildOrder(scene, "Canvas_GameHUD", new string[]
        {
            "Panel_HUDRoot",
            "Panel_CinderHeartHUD",
            "Panel_EnemyTargetHUD",
            "Panel_InventoryRoot_Disabled",
            "Panel_GameOver_Disabled"
        });

        SetChildOrder(scene, "EnemySpawnCandidatePoints", new string[]
        {
            "EnemySpawnCandidate_01",
            "EnemySpawnCandidate_02",
            "EnemySpawnCandidate_03",
            "EnemySpawnCandidate_04",
            "EnemySpawnCandidate_05",
            "EnemySpawnCandidate_06",
            "EnemySpawnCandidate_07",
            "EnemySpawnCandidate_08"
        });
    }

    private static void SetupLoopConnectors(Scene scene)
    {
        GameObject loopRoot = GetOrCreateRootObject(scene, "MainGame_LoopConnector");
        CinderkeepGameLoopConnector loopConnector = EnsureComponent<CinderkeepGameLoopConnector>(loopRoot);

        PlayerLoopConnector playerLoopConnector = EnsureComponent<PlayerLoopConnector>(
            GetOrCreateChild(loopRoot.transform, "PlayerLoopConnector").gameObject);
        ResourceLoopConnector resourceLoopConnector = EnsureComponent<ResourceLoopConnector>(
            GetOrCreateChild(loopRoot.transform, "ResourceLoopConnector").gameObject);
        EnemyLoopConnector enemyLoopConnector = EnsureEnemyLoopConnector(scene, loopRoot.transform);
        GameFlowLoopConnector gameFlowLoopConnector = EnsureComponent<GameFlowLoopConnector>(
            GetOrCreateChild(loopRoot.transform, "GameFlowLoopConnector").gameObject);

        GameObject playerObject = FindRootObject(scene, "Player");
        GameObject cinderHeartObject = FindRootObject(scene, "CinderHeart");
        GameObject managerRoot = FindRootObject(scene, "MainGame_Managers");

        PlayerStatus playerStatus = GetComponentFromObject<PlayerStatus>(playerObject);
        PlayerHUD playerHud = GetComponentByName<PlayerHUD>(scene, "Panel_PlayerHUD");
        ResourceUI resourceUi = GetComponentByName<ResourceUI>(scene, "Panel_ResourceUI");
        GameManager gameManager = GetComponentInRootChild<GameManager>(managerRoot, "GameManager");
        GameDataManager gameDataManager = GetComponentInRootChild<GameDataManager>(managerRoot, "GameDataManager");
        GameObjectManager gameObjectManager = GetComponentInRootChild<GameObjectManager>(managerRoot, "GameObjectManager");
        Camera gameCamera = GetComponentInChildrenFromObject<Camera>(playerObject);
        Transform cinderHeartTarget = GetTransformFromObject(cinderHeartObject);

        SetObjectReference(playerLoopConnector, "_playerStatus", playerStatus);
        SetObjectReference(playerLoopConnector, "_playerHud", playerHud);

        SetObjectReference(resourceLoopConnector, "_gameManager", gameManager);
        SetObjectReference(resourceLoopConnector, "_resourceUi", resourceUi);

        SetObjectReference(enemyLoopConnector, "_gameDataManager", gameDataManager);
        SetObjectReference(enemyLoopConnector, "_cinderHeartTarget", cinderHeartTarget);
        SetObjectReference(enemyLoopConnector, "_gameCamera", gameCamera);

        SetObjectReference(gameFlowLoopConnector, "_gameManager", gameManager);
        SetObjectReference(gameFlowLoopConnector, "_gameObjectManager", gameObjectManager);
        SetObjectReference(gameFlowLoopConnector, "_enemyLoopConnector", enemyLoopConnector);

        SetObjectReference(loopConnector, "_playerLoopConnector", playerLoopConnector);
        SetObjectReference(loopConnector, "_resourceLoopConnector", resourceLoopConnector);
        SetObjectReference(loopConnector, "_enemyLoopConnector", enemyLoopConnector);
        SetObjectReference(loopConnector, "_gameFlowLoopConnector", gameFlowLoopConnector);
    }

    private static EnemyLoopConnector EnsureEnemyLoopConnector(Scene scene, Transform parent)
    {
        GameObject enemyConnectorObject = GetSceneObjectByName(scene, "EnemyLoopConnector");
        if (enemyConnectorObject == null)
        {
            enemyConnectorObject = GetOrCreateChild(parent, "EnemyLoopConnector").gameObject;
        }

        if (enemyConnectorObject.transform.parent != parent)
        {
            enemyConnectorObject.transform.SetParent(parent, true);
            EditorUtility.SetDirty(enemyConnectorObject);
        }

        return EnsureComponent<EnemyLoopConnector>(enemyConnectorObject);
    }

    private static void SetRootOrder(Scene scene, IReadOnlyList<string> orderedNames)
    {
        Dictionary<string, GameObject> rootsByName = GetRootObjectsByName(scene);
        for (int i = 0; i < orderedNames.Count; i++)
        {
            GameObject rootObject;
            if (rootsByName.TryGetValue(orderedNames[i], out rootObject))
            {
                rootObject.transform.SetSiblingIndex(i);
                EditorUtility.SetDirty(rootObject);
            }
        }
    }

    private static void SetChildOrder(Scene scene, string rootName, IReadOnlyList<string> orderedNames)
    {
        GameObject rootObject = FindRootObject(scene, rootName);
        if (rootObject == null)
        {
            return;
        }

        Dictionary<string, Transform> childrenByName = new Dictionary<string, Transform>();
        for (int i = 0; i < rootObject.transform.childCount; i++)
        {
            Transform child = rootObject.transform.GetChild(i);
            if (childrenByName.ContainsKey(child.name) == false)
            {
                childrenByName.Add(child.name, child);
            }
        }

        for (int i = 0; i < orderedNames.Count; i++)
        {
            Transform child;
            if (childrenByName.TryGetValue(orderedNames[i], out child))
            {
                child.SetSiblingIndex(i);
                EditorUtility.SetDirty(child.gameObject);
            }
        }
    }

    private static Dictionary<string, GameObject> GetRootObjectsByName(Scene scene)
    {
        Dictionary<string, GameObject> rootsByName = new Dictionary<string, GameObject>();
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            if (rootsByName.ContainsKey(rootObjects[i].name) == false)
            {
                rootsByName.Add(rootObjects[i].name, rootObjects[i]);
            }
        }

        return rootsByName;
    }

    private static GameObject FindRootObject(Scene scene, string objectName)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            if (rootObjects[i].name == objectName)
            {
                return rootObjects[i];
            }
        }

        return null;
    }

    private static GameObject GetOrCreateRootObject(Scene scene, string objectName)
    {
        GameObject rootObject = FindRootObject(scene, objectName);
        if (rootObject != null)
        {
            return rootObject;
        }

        rootObject = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(rootObject, scene);
        EditorUtility.SetDirty(rootObject);
        return rootObject;
    }

    private static Transform GetOrCreateChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        childObject.transform.SetParent(parent, false);
        EditorUtility.SetDirty(childObject);
        return childObject.transform;
    }

    private static GameObject GetSceneObjectByName(Scene scene, string objectName)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject sceneObject = GetSceneObjectByNameRecursive(rootObjects[i].transform, objectName);
            if (sceneObject != null)
            {
                return sceneObject;
            }
        }

        return null;
    }

    private static GameObject GetSceneObjectByNameRecursive(Transform currentTransform, string objectName)
    {
        if (currentTransform.name == objectName)
        {
            return currentTransform.gameObject;
        }

        for (int i = 0; i < currentTransform.childCount; i++)
        {
            GameObject sceneObject = GetSceneObjectByNameRecursive(currentTransform.GetChild(i), objectName);
            if (sceneObject != null)
            {
                return sceneObject;
            }
        }

        return null;
    }

    private static TComponent EnsureComponent<TComponent>(GameObject targetObject)
        where TComponent : Component
    {
        TComponent component = targetObject.GetComponent<TComponent>();
        if (component != null)
        {
            return component;
        }

        component = targetObject.AddComponent<TComponent>();
        EditorUtility.SetDirty(targetObject);
        return component;
    }

    private static TComponent GetComponentFromObject<TComponent>(GameObject targetObject)
        where TComponent : Component
    {
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.GetComponent<TComponent>();
    }

    private static TComponent GetComponentInChildrenFromObject<TComponent>(GameObject targetObject)
        where TComponent : Component
    {
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.GetComponentInChildren<TComponent>(true);
    }

    private static TComponent GetComponentByName<TComponent>(Scene scene, string objectName)
        where TComponent : Component
    {
        GameObject targetObject = GetSceneObjectByName(scene, objectName);
        return GetComponentFromObject<TComponent>(targetObject);
    }

    private static TComponent GetComponentInRootChild<TComponent>(GameObject rootObject, string childName)
        where TComponent : Component
    {
        if (rootObject == null)
        {
            return null;
        }

        Transform child = rootObject.transform.Find(childName);
        if (child == null)
        {
            return null;
        }

        return child.GetComponent<TComponent>();
    }

    private static Transform GetTransformFromObject(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.transform;
    }

    private static void SetObjectReference(Object targetObject, string propertyName, Object value)
    {
        if (targetObject == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(targetObject);
    }
}
