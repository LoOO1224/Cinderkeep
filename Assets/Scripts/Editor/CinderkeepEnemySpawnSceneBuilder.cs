using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
public static class CinderkeepEnemySpawnSceneBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string EnemyPrefabFolderPath = "Assets/Prefabs/Enemy";
    private const string MaterialFolderPath = "Assets/Materials/Generated";

    private const string MageRedPath = "Assets/ThirdParty/Free/CinderkeepExternalAssets/Mages/MageRed.prefab";
    private const string MagePurplePath = "Assets/ThirdParty/Free/CinderkeepExternalAssets/Mages/MagePurple.prefab";
    private const string PlantPath = "Assets/ThirdParty/Free/CinderkeepExternalAssets/CarnivorousPlant/Prefabs/Carnivorous Plant-Green.prefab";
    private const string FrostWolfPrefabPath =
        "Assets/Prefabs/Enemy/PF_Enemy_LowPoly_FrostWolf.prefab";

    [MenuItem("Cinderkeep/Setup Enemy Spawn Scene")]
    public static void SetupEnemySpawnScene()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(EnemyPrefabFolderPath);
        EnsureFolder("Assets/Materials");
        EnsureFolder(MaterialFolderPath);
        EnsureTag("Enemy");

        GameObject redMageEnemy = CreateEnemyPrefab(
            "PF_Enemy_LowPoly_RedMage",
            MageRedPath,
            GetOrCreateMaterial("MAT_Enemy_LowPoly_Red", new Color(0.55f, 0.08f, 0.06f, 1f)),
            new Vector3(0f, 0f, 0f),
            new Vector3(1.2f, 1.2f, 1.2f));

        GameObject purpleMageEnemy = CreateEnemyPrefab(
            "PF_Enemy_LowPoly_FrostMage",
            MagePurplePath,
            GetOrCreateMaterial("MAT_Enemy_LowPoly_Purple", new Color(0.28f, 0.18f, 0.55f, 1f)),
            new Vector3(0f, 0f, 0f),
            new Vector3(1.2f, 1.2f, 1.2f));

        GameObject plantEnemy = CreateEnemyPrefab(
            "PF_Enemy_LowPoly_IcePlant",
            PlantPath,
            GetOrCreateMaterial("MAT_Enemy_LowPoly_IcePlant", new Color(0.22f, 0.75f, 0.78f, 1f)),
            new Vector3(0f, 0f, 0f),
            new Vector3(1.4f, 1.4f, 1.4f));
        GameObject frostWolfEnemy = AssetDatabase.LoadAssetAtPath<GameObject>(FrostWolfPrefabPath);
        if (frostWolfEnemy == null)
        {
            Debug.LogError("CinderkeepEnemySpawnSceneBuilder: FrostWolf 프리팹이 없습니다.");
            return;
        }

        CopyEnemyPrefabToSandbox(redMageEnemy);
        CopyEnemyPrefabToSandbox(purpleMageEnemy);
        CopyEnemyPrefabToSandbox(plantEnemy);
        CopyEnemyPrefabToSandbox(frostWolfEnemy);

        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        GameObject runtimeRoot = GetOrCreateRootObject("MainGame_RuntimeManagers");
        GameObject spawnRoot = GetOrCreateRootObject("EnemySpawnPoints");
        GameObject candidateRoot = GetOrCreateRootObject("EnemySpawnCandidatePoints");

        GameManager gameManager = GetComponentInScene<GameManager>();
        GameObjectManager gameObjectManager = GetComponentInScene<GameObjectManager>();
        GameFlowController gameFlowController = GetOrCreateComponentObject<GameFlowController>(runtimeRoot.transform, "GameFlowController");
        GameFlowEnemySpawnDirector enemySpawnDirector = GetOrCreateComponentObject<GameFlowEnemySpawnDirector>(
            runtimeRoot.transform,
            "GameFlowEnemySpawnDirector");
        EnemyLoopConnector enemyLoopConnector = GetOrCreateComponentObject<EnemyLoopConnector>(runtimeRoot.transform, "EnemyLoopConnector");
        Transform cinderHeartTarget = GetOrCreateRootObject("CinderHeart").transform;
        Camera gameCamera = GetComponentInScene<Camera>();

        EnemySpawnPoint nearSpawnPoint = CreateOrUpdateSpawnPoint(
            spawnRoot.transform,
            "EnemySpawnPoint_Near_Required",
            new Vector3(18f, 0f, 18f),
            gameObjectManager,
            enemyLoopConnector,
            redMageEnemy,
            purpleMageEnemy,
            plantEnemy,
            frostWolfEnemy);

        SetSpawnRule(nearSpawnPoint, "_day1Rule", 999f, 1, 1, true);
        SetSpawnRule(nearSpawnPoint, "_day2Rule", 999f, 1, 1, true);
        SetSpawnRule(nearSpawnPoint, "_day3Rule", 999f, 1, 1, true);
        SetSpawnRule(nearSpawnPoint, "_night1Rule", 999f, 1, 1, true);
        SetSpawnRule(nearSpawnPoint, "_night2Rule", 999f, 1, 1, true);
        SetSpawnRule(nearSpawnPoint, "_night3Rule", 999f, 1, 1, true);
        SetSpawnRule(nearSpawnPoint, "_bossRule", 999f, 1, 1, true);

        Transform[] outerCandidates = CreateOuterCandidatePoints(candidateRoot.transform);
        EnemySpawnPoint outerSpawnPoint = CreateOrUpdateSpawnPoint(
            spawnRoot.transform,
            "EnemySpawnPoint_Outer_Random",
            Vector3.zero,
            gameObjectManager,
            enemyLoopConnector,
            redMageEnemy,
            purpleMageEnemy,
            plantEnemy,
            frostWolfEnemy);

        SetCandidatePoints(outerSpawnPoint, outerCandidates);

        ConnectGameFlowController(gameFlowController, enemySpawnDirector);
        ConnectGameFlowEnemySpawnDirector(enemySpawnDirector, new EnemySpawnPoint[] { nearSpawnPoint, outerSpawnPoint });
        ConnectGameManager(gameManager, gameFlowController);
        ConnectEnemyLoopConnector(enemyLoopConnector, GetComponentInScene<GameDataManager>(), cinderHeartTarget, gameCamera);
        ConnectGameLoopConnector(gameManager, GetComponentInScene<GameDataManager>(), gameObjectManager, enemyLoopConnector, cinderHeartTarget, gameCamera);
        gameFlowController.InitializeEnemySpawnPoints(gameObjectManager, enemyLoopConnector);

        EditorUtility.SetDirty(runtimeRoot);
        EditorUtility.SetDirty(spawnRoot);
        EditorUtility.SetDirty(candidateRoot);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CinderkeepEnemySpawnSceneBuilder: enemy prefabs and spawn points updated.");
    }

    private static GameObject CreateEnemyPrefab(string prefabName, string visualPrefabPath, Material fallbackMaterial, Vector3 visualPosition, Vector3 visualScale)
    {
        string prefabPath = EnemyPrefabFolderPath + "/" + prefabName + ".prefab";
        GameObject rootObject = new GameObject(prefabName);
        rootObject.tag = "Enemy";

        CapsuleCollider capsuleCollider = rootObject.AddComponent<CapsuleCollider>();
        capsuleCollider.center = new Vector3(0f, 1f, 0f);
        capsuleCollider.height = 2f;
        capsuleCollider.radius = 0.45f;

        Rigidbody rigidbody = rootObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        NavMeshAgent navMeshAgent = rootObject.AddComponent<NavMeshAgent>();
        navMeshAgent.radius = 0.45f;
        navMeshAgent.height = 2f;
        navMeshAgent.speed = 2f;

        rootObject.AddComponent<Damageable>();
        rootObject.AddComponent<EnemyStatus>();
        rootObject.AddComponent<EnemyDetector>();
        rootObject.AddComponent<EnemyAttack>();
        rootObject.AddComponent<EnemyMovement>();
        rootObject.AddComponent<EnemyBrain>();

        GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPrefabPath);
        if (visualPrefab != null)
        {
            GameObject visualObject = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab);
            visualObject.name = "Visual_" + prefabName;
            visualObject.transform.SetParent(rootObject.transform);
            visualObject.transform.localPosition = visualPosition;
            visualObject.transform.localRotation = Quaternion.identity;
            visualObject.transform.localScale = visualScale;
            ApplyMaterialToRenderers(visualObject, fallbackMaterial);
        }
        else
        {
            GameObject fallbackObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallbackObject.name = "Fallback_" + prefabName;
            fallbackObject.transform.SetParent(rootObject.transform);
            fallbackObject.transform.localPosition = new Vector3(0f, 1f, 0f);
            fallbackObject.transform.localRotation = Quaternion.identity;
            fallbackObject.transform.localScale = visualScale;
            ApplyMaterialToRenderers(fallbackObject, fallbackMaterial);
        }

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath);
        Object.DestroyImmediate(rootObject);
        return savedPrefab;
    }

    private static void CopyEnemyPrefabToSandbox(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(enemyPrefab);
        string sandboxFolderPath = "Assets/_Sandbox/4_0_CandidateAssets/Prefabs";
        string targetPath = sandboxFolderPath + "/" + enemyPrefab.name + ".prefab";

        if (AssetDatabase.IsValidFolder(sandboxFolderPath) == false)
        {
            return;
        }

        AssetDatabase.DeleteAsset(targetPath);
        AssetDatabase.CopyAsset(sourcePath, targetPath);
    }

    private static EnemySpawnPoint CreateOrUpdateSpawnPoint(
        Transform parent,
        string objectName,
        Vector3 position,
        GameObjectManager gameObjectManager,
        EnemyLoopConnector enemyLoopConnector,
        GameObject redMageEnemy,
        GameObject purpleMageEnemy,
        GameObject plantEnemy,
        GameObject frostWolfEnemy)
    {
        GameObject spawnObject = GetOrCreateChild(parent, objectName);
        spawnObject.transform.position = position;

        EnemySpawnPoint spawnPoint = spawnObject.GetComponent<EnemySpawnPoint>();
        if (spawnPoint == null)
        {
            spawnPoint = spawnObject.AddComponent<EnemySpawnPoint>();
        }

        SerializedObject serializedObject = new SerializedObject(spawnPoint);
        serializedObject.FindProperty("_gameObjectManager").objectReferenceValue = gameObjectManager;
        serializedObject.FindProperty("_enemyLoopConnector").objectReferenceValue = enemyLoopConnector;
        serializedObject.FindProperty("_isActive").boolValue = true;
        serializedObject.FindProperty("_step1EnemyDataId").stringValue = "ice_zombie";
        serializedObject.FindProperty("_step2EnemyDataId").stringValue = "frost_wolf";
        serializedObject.FindProperty("_step3EnemyDataId").stringValue = "ice_zombie";
        SetPrefabArray(serializedObject.FindProperty("_step1EnemyPrefabs"), new GameObject[] { plantEnemy });
        SetPrefabArray(serializedObject.FindProperty("_step2EnemyPrefabs"), new GameObject[] { frostWolfEnemy });
        SetPrefabArray(serializedObject.FindProperty("_step3EnemyPrefabs"), new GameObject[] { plantEnemy, redMageEnemy, purpleMageEnemy });
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        return spawnPoint;
    }

    private static Transform[] CreateOuterCandidatePoints(Transform parent)
    {
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0f, 0f, 55f),
            new Vector3(40f, 0f, 40f),
            new Vector3(55f, 0f, 0f),
            new Vector3(40f, 0f, -40f),
            new Vector3(0f, 0f, -55f),
            new Vector3(-40f, 0f, -40f),
            new Vector3(-55f, 0f, 0f),
            new Vector3(-40f, 0f, 40f)
        };

        Transform[] candidatePoints = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            string objectName = "EnemySpawnCandidate_" + (i + 1).ToString("00");
            GameObject candidateObject = GetOrCreateChild(parent, objectName);
            candidateObject.transform.position = positions[i];
            candidatePoints[i] = candidateObject.transform;
        }

        return candidatePoints;
    }

    private static void SetCandidatePoints(EnemySpawnPoint spawnPoint, Transform[] candidatePoints)
    {
        SerializedObject serializedObject = new SerializedObject(spawnPoint);
        SerializedProperty candidateProperty = serializedObject.FindProperty("_spawnCandidatePoints");
        candidateProperty.arraySize = candidatePoints.Length;

        for (int i = 0; i < candidatePoints.Length; i++)
        {
            candidateProperty.GetArrayElementAtIndex(i).objectReferenceValue = candidatePoints[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConnectGameFlowController(
        GameFlowController gameFlowController,
        GameFlowEnemySpawnDirector enemySpawnDirector)
    {
        SerializedObject serializedObject = new SerializedObject(gameFlowController);
        SerializedProperty enemySpawnDirectorProperty = serializedObject.FindProperty("_enemySpawnDirector");
        if (enemySpawnDirectorProperty != null)
        {
            enemySpawnDirectorProperty.objectReferenceValue = enemySpawnDirector;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConnectGameFlowEnemySpawnDirector(
        GameFlowEnemySpawnDirector enemySpawnDirector,
        EnemySpawnPoint[] spawnPoints)
    {
        SerializedObject serializedObject = new SerializedObject(enemySpawnDirector);
        SerializedProperty spawnPointProperty = serializedObject.FindProperty("_enemySpawnPoints");
        if (spawnPointProperty == null)
        {
            return;
        }

        spawnPointProperty.arraySize = spawnPoints.Length;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPointProperty.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConnectGameManager(GameManager gameManager, GameFlowController gameFlowController)
    {
        if (gameManager == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(gameManager);
        SerializedProperty gameFlowProperty = serializedObject.FindProperty("_gameFlowController");
        if (gameFlowProperty != null)
        {
            gameFlowProperty.objectReferenceValue = gameFlowController;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConnectEnemyLoopConnector(
        EnemyLoopConnector enemyLoopConnector,
        GameDataManager gameDataManager,
        Transform cinderHeartTarget,
        Camera gameCamera)
    {
        if (enemyLoopConnector == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(enemyLoopConnector);
        serializedObject.FindProperty("_gameDataManager").objectReferenceValue = gameDataManager;
        serializedObject.FindProperty("_cinderHeartTarget").objectReferenceValue = cinderHeartTarget;
        serializedObject.FindProperty("_gameCamera").objectReferenceValue = gameCamera;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConnectGameLoopConnector(
        GameManager gameManager,
        GameDataManager gameDataManager,
        GameObjectManager gameObjectManager,
        EnemyLoopConnector enemyLoopConnector,
        Transform cinderHeartTarget,
        Camera gameCamera)
    {
        CinderkeepGameLoopConnector gameLoopConnector = GetComponentInScene<CinderkeepGameLoopConnector>();
        if (gameLoopConnector == null)
        {
            return;
        }

        Transform connectorRoot = gameLoopConnector.transform;
        PlayerLoopConnector playerLoopConnector = GetOrCreateComponentObject<PlayerLoopConnector>(connectorRoot, "PlayerLoopConnector");
        ResourceLoopConnector resourceLoopConnector = GetOrCreateComponentObject<ResourceLoopConnector>(connectorRoot, "ResourceLoopConnector");
        GameFlowLoopConnector gameFlowLoopConnector = GetOrCreateComponentObject<GameFlowLoopConnector>(connectorRoot, "GameFlowLoopConnector");

        if (enemyLoopConnector.transform.parent != connectorRoot)
        {
            enemyLoopConnector.transform.SetParent(connectorRoot, true);
        }

        SetObjectReference(enemyLoopConnector, "_gameDataManager", gameDataManager);
        SetObjectReference(enemyLoopConnector, "_cinderHeartTarget", cinderHeartTarget);
        SetObjectReference(enemyLoopConnector, "_gameCamera", gameCamera);

        SetObjectReference(resourceLoopConnector, "_gameManager", gameManager);
        SetObjectReference(gameFlowLoopConnector, "_gameManager", gameManager);
        SetObjectReference(gameFlowLoopConnector, "_gameObjectManager", gameObjectManager);
        SetObjectReference(gameFlowLoopConnector, "_enemyLoopConnector", enemyLoopConnector);

        SetObjectReference(gameLoopConnector, "_playerLoopConnector", playerLoopConnector);
        SetObjectReference(gameLoopConnector, "_resourceLoopConnector", resourceLoopConnector);
        SetObjectReference(gameLoopConnector, "_enemyLoopConnector", enemyLoopConnector);
        SetObjectReference(gameLoopConnector, "_gameFlowLoopConnector", gameFlowLoopConnector);
    }

    private static void SetSpawnRule(EnemySpawnPoint spawnPoint, string fieldName, float interval, int count, int maxAlive, bool spawnOnStart)
    {
        SerializedObject serializedObject = new SerializedObject(spawnPoint);
        SerializedProperty ruleProperty = serializedObject.FindProperty(fieldName);
        ruleProperty.FindPropertyRelative("_spawnInterval").floatValue = interval;
        ruleProperty.FindPropertyRelative("_spawnCountPerWave").intValue = count;
        ruleProperty.FindPropertyRelative("_maxAliveEnemyCount").intValue = maxAlive;
        ruleProperty.FindPropertyRelative("_spawnOnModeStart").boolValue = spawnOnStart;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetPrefabArray(SerializedProperty property, GameObject[] prefabs)
    {
        property.arraySize = prefabs.Length;

        for (int i = 0; i < prefabs.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
        }
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

    private static T GetOrCreateComponentObject<T>(Transform parent, string objectName)
        where T : Component
    {
        GameObject targetObject = GetOrCreateChild(parent, objectName);
        T component = targetObject.GetComponent<T>();
        if (component == null)
        {
            component = targetObject.AddComponent<T>();
        }

        return component;
    }

    private static T GetComponentInScene<T>()
        where T : Component
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            T component = rootObjects[i].GetComponentInChildren<T>(true);
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }

    private static GameObject GetOrCreateRootObject(string objectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            if (rootObjects[i].name == objectName)
            {
                return rootObjects[i];
            }
        }

        return new GameObject(objectName);
    }

    private static GameObject GetOrCreateChild(Transform parent, string objectName)
    {
        Transform childTransform = parent.Find(objectName);
        if (childTransform != null)
        {
            return childTransform.gameObject;
        }

        GameObject childObject = new GameObject(objectName);
        childObject.transform.SetParent(parent);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject;
    }

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        string materialPath = MaterialFolderPath + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material != null)
        {
            return material;
        }

        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }

    private static void ApplyMaterialToRenderers(GameObject targetObject, Material material)
    {
        if (targetObject == null || material == null)
        {
            return;
        }

        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parentPath = System.IO.Path.GetDirectoryName(folderPath);
        string folderName = System.IO.Path.GetFileName(folderPath);
        parentPath = parentPath.Replace("\\", "/");
        AssetDatabase.CreateFolder(parentPath, folderName);
    }

    private static void EnsureTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            SerializedProperty tagProperty = tagsProperty.GetArrayElementAtIndex(i);
            if (tagProperty.stringValue == tagName)
            {
                return;
            }
        }

        tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
        SerializedProperty newTagProperty = tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1);
        newTagProperty.stringValue = tagName;
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }
}
