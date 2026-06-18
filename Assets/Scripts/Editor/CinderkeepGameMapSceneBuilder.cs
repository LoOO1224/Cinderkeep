using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cinderkeep.MainGame.Editor
{
    // Cinderkeep_Game 씬에 기본 플레이어, CinderHeart, HUD 뼈대를 배치하는 에디터 도구입니다.
    // 씬 오브젝트 이름도 실제 게임 기준 네이밍으로 맞춰 팀원이 Hierarchy를 쉽게 읽게 합니다.
    public static class CinderkeepGameMapSceneBuilder
    {
        private const string _gameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
        private const string _materialFolderPath = "Assets/Materials/MainGame";
        private const string _iceMaterialPath = _materialFolderPath + "/GameMap_Ice.mat";
        private const string _wallMaterialPath = _materialFolderPath + "/GameMap_Wall.mat";
        private const string _markerMaterialPath = _materialFolderPath + "/GameMap_Marker.mat";
        private const string _cinderHeartMaterialPath = _materialFolderPath + "/CinderHeart_Red.mat";
        private const string _horizonMaterialPath = _materialFolderPath + "/GameMap_Horizon.mat";
        private const string _centerChunkPrefabPath = "Assets/Prefabs/Map/PF_Map_GameMapGroup.prefab";
        private const string _cinderHeartTagName = "CinderHeart";

        [MenuItem("Cinderkeep/Main Game/Rebuild First Person Game Map")]
        public static void RebuildFirstPersonGameMap()
        {
            Scene scene = EditorSceneManager.OpenScene(_gameScenePath, OpenSceneMode.Single);
            ClearScene(scene);

            Material iceMaterial = GetOrCreateMaterial(_iceMaterialPath, new Color(0.48f, 0.68f, 0.8f, 1f));
            Material wallMaterial = GetOrCreateMaterial(_wallMaterialPath, new Color(0.12f, 0.17f, 0.22f, 1f));
            Material markerMaterial = GetOrCreateMaterial(_markerMaterialPath, new Color(1f, 0.47f, 0.12f, 1f));
            Material cinderHeartMaterial = GetOrCreateMaterial(_cinderHeartMaterialPath, new Color(0.95f, 0.05f, 0.03f, 1f));
            Material horizonMaterial = GetOrCreateMaterial(_horizonMaterialPath, new Color(0.22f, 0.32f, 0.42f, 1f));

            EnsureTag(_cinderHeartTagName);

            GameObject root = GetOrCreateRoot(scene, "MainGame_GameMapGroup");
            ClearChildren(root.transform);

            CreateGameMap(root.transform, iceMaterial, wallMaterial, markerMaterial, cinderHeartMaterial, horizonMaterial);
            CreatePlayerRig(scene);
            CreateRuntimeManagers(scene);
            CreateHud(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("CinderkeepGameMapSceneBuilder: first person game map was rebuilt.");
        }

        [MenuItem("Cinderkeep/Main Game/Normalize Main Game Hierarchy")]
        public static void NormalizeMainGameHierarchy()
        {
            Scene scene = EditorSceneManager.OpenScene(_gameScenePath, OpenSceneMode.Single);

            DestroyRootObjectIfExists(scene, "Legacy_MainGame_GameMapGroup_Disabled");
            RenameSceneObjects(scene);
            MoveObjectUnderManagerGroup(scene, "MapManager");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            RenameCenterChunkPrefabObjects();
            AssetDatabase.SaveAssets();

            Debug.Log("CinderkeepGameMapSceneBuilder: main game hierarchy was normalized.");
        }

        private static void CreateGameMap(
            Transform root,
            Material iceMaterial,
            Material wallMaterial,
            Material markerMaterial,
            Material cinderHeartMaterial,
            Material horizonMaterial)
        {
            GameObject ground = CreatePrimitive(
                PrimitiveType.Cube,
                "Ground_IcePlane",
                root,
                new Vector3(0f, -0.05f, 0f),
                new Vector3(120f, 0.1f, 120f),
                iceMaterial);

            ground.isStatic = true;

            CreatePrimitive(PrimitiveType.Cube, "HorizonRidge_North", root, new Vector3(0f, 0.55f, 54f), new Vector3(120f, 1.1f, 2f), horizonMaterial);
            CreatePrimitive(PrimitiveType.Cube, "HorizonRidge_South", root, new Vector3(0f, 0.45f, -54f), new Vector3(120f, 0.9f, 2f), horizonMaterial);
            CreatePrimitive(PrimitiveType.Cube, "HorizonRidge_East", root, new Vector3(54f, 0.45f, 0f), new Vector3(2f, 0.9f, 120f), horizonMaterial);
            CreatePrimitive(PrimitiveType.Cube, "HorizonRidge_West", root, new Vector3(-54f, 0.45f, 0f), new Vector3(2f, 0.9f, 120f), horizonMaterial);

            GameObject cinderHeart = CreatePrimitive(PrimitiveType.Cube, "CinderHeart", root, new Vector3(0f, 1f, 0f), new Vector3(1.8f, 1.8f, 1.8f), cinderHeartMaterial);
            cinderHeart.tag = _cinderHeartTagName;
            CreatePointLight("Light_CinderHeart_Red", cinderHeart.transform, new Vector3(0f, 1.2f, 0f), new Color(1f, 0.12f, 0.04f, 1f), 3f, 9f);
            CreateMarker(root, "Marker_PlayerSpawn", new Vector3(0f, 0f, -8f));
            CreateMarker(root, "Marker_EnemySpawn_North", new Vector3(0f, 0f, 15f));
            CreateMarker(root, "Marker_EnemySpawn_East", new Vector3(15f, 0f, 0f));

            CreatePrimitive(PrimitiveType.Cube, "ResourceNode_Stone_01", root, new Vector3(-7f, 0.45f, -2f), new Vector3(1.4f, 0.9f, 1.4f), wallMaterial);
            CreatePrimitive(PrimitiveType.Cube, "ResourceNode_Wood_01", root, new Vector3(7f, 0.75f, -2f), new Vector3(0.8f, 1.5f, 0.8f), wallMaterial);
            CreatePrimitive(PrimitiveType.Cylinder, "BuildPoint_Wall_North", root, new Vector3(-4f, 0.1f, 5f), new Vector3(1.2f, 0.2f, 1.2f), markerMaterial);
            CreatePrimitive(PrimitiveType.Cylinder, "BuildPoint_Turret_East", root, new Vector3(4f, 0.1f, 5f), new Vector3(1.2f, 0.2f, 1.2f), markerMaterial);
        }

        private static void CreatePlayerRig(Scene scene)
        {
            GameObject player = GetOrCreateRoot(scene, "Player");
            ClearChildren(player.transform);

            player.transform.position = new Vector3(0f, 0.05f, -8f);
            player.transform.rotation = Quaternion.identity;

            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = player.AddComponent<CharacterController>();
            }

            characterController.center = new Vector3(0f, 0.85f, 0f);
            characterController.height = 1.7f;
            characterController.radius = 0.35f;
            characterController.stepOffset = 0.35f;
            characterController.slopeLimit = 45f;

            GameObject visual = CreatePrimitive(
                PrimitiveType.Capsule,
                "PlayerVisual_FallbackCapsule",
                player.transform,
                new Vector3(0f, 0.85f, 0f),
                new Vector3(0.7f, 0.85f, 0.7f),
                GetOrCreateMaterial(_markerMaterialPath, new Color(1f, 0.47f, 0.12f, 1f)));
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());

            GameObject cameraRoot = CreateEmpty("Transform_CameraRoot_FirstPerson", player.transform, new Vector3(0f, 1.55f, 0f));
            GameObject cameraObject = CreateEmpty("Camera_FirstPerson", cameraRoot.transform, Vector3.zero);
            Camera camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 500f;
            camera.depth = 20f;
            camera.tag = "MainCamera";

            AudioListener audioListener = cameraObject.GetComponent<AudioListener>();
            if (audioListener == null)
            {
                audioListener = cameraObject.AddComponent<AudioListener>();
            }

            DisableOtherCamerasAndListeners(scene, camera, audioListener);
        }

        private static void CreateRuntimeManagers(Scene scene)
        {
            GameObject group = GetOrCreateRoot(scene, "MainGame_Managers");
            ClearChildren(group.transform);

            GameObject gameManagerObject = CreateEmpty("GameManager", group.transform, Vector3.zero);
            GameObject dataManagerObject = CreateEmpty("GameDataManager", group.transform, Vector3.zero);
            GameObject objectManagerObject = CreateEmpty("GameObjectManager", group.transform, Vector3.zero);
            GameObject uiManagerObject = CreateEmpty("UIManager", group.transform, Vector3.zero);
            GameObject soundManagerObject = CreateEmpty("SoundManager", group.transform, Vector3.zero);
            GameObject objectRoot = CreateEmpty("Transform_RuntimeObjectRoot", group.transform, Vector3.zero);

            GameManager gameManager = GetOrAddComponent<GameManager>(gameManagerObject);
            GameDataManager dataManager = GetOrAddComponent<GameDataManager>(dataManagerObject);
            GameObjectManager objectManager = GetOrAddComponent<GameObjectManager>(objectManagerObject);
            UIManager uiManager = GetOrAddComponent<UIManager>(uiManagerObject);
            SoundManager soundManager = GetOrAddComponent<SoundManager>(soundManagerObject);

            AudioSource bgmSource = GetOrAddComponent<AudioSource>(CreateEmpty("AudioSource_Bgm", soundManagerObject.transform, Vector3.zero));
            AudioSource effectSource = GetOrAddComponent<AudioSource>(CreateEmpty("AudioSource_Effect", soundManagerObject.transform, Vector3.zero));
            bgmSource.playOnAwake = false;
            effectSource.playOnAwake = false;

            SetObjectReference(gameManager, "_gameDataManager", dataManager);
            SetObjectReference(gameManager, "_gameObjectManager", objectManager);
            SetObjectReference(gameManager, "_uiManager", uiManager);
            SetObjectReference(gameManager, "_soundManager", soundManager);
            SetObjectReference(objectManager, "_objectRoot", objectRoot.transform);
            SetObjectReference(soundManager, "_bgmAudioSource", bgmSource);
            SetObjectReference(soundManager, "_effectAudioSource", effectSource);
        }

        private static void CreateHud(Scene scene)
        {
            GameObject canvasObject = GetOrCreateRoot(scene, "Canvas_GameHUD");
            ClearChildren(canvasObject.transform);

            Canvas canvas = GetOrAddComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = GetOrAddComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GetOrAddComponent<GraphicRaycaster>(canvasObject);

            GameObject hudRoot = CreateUiPanel("Panel_HUDRoot", canvasObject.transform, new Vector2(20f, -20f), new Vector2(360f, 120f), new Color(0f, 0f, 0f, 0.45f));
            CreateUiText("Text_HUD_Guide", hudRoot.transform, "HP / Stamina HUD 연결 자리", 24, TextAnchor.MiddleLeft);
            GameObject inventoryRoot = CreateUiPanel("Panel_InventoryRoot_Disabled", canvasObject.transform, new Vector2(20f, -160f), new Vector2(360f, 120f), new Color(0f, 0f, 0f, 0.35f));
            GameObject gameOverRoot = CreateUiPanel("Panel_GameOver_Disabled", canvasObject.transform, new Vector2(-240f, 140f), new Vector2(480f, 180f), new Color(0.08f, 0.02f, 0.02f, 0.75f));
            CreateUiText("Text_GameOver_Guide", gameOverRoot.transform, "GameOver UI 연결 자리", 24, TextAnchor.MiddleCenter);
            inventoryRoot.SetActive(false);
            gameOverRoot.SetActive(false);

            GameObject managerGroup = GetOrCreateRoot(scene, "MainGame_Managers");
            Transform uiManagerTransform = FindChildByName(managerGroup.transform, "UIManager");
            if (uiManagerTransform == null)
            {
                return;
            }

            UIManager uiManager = uiManagerTransform.GetComponent<UIManager>();
            if (uiManager == null)
            {
                return;
            }

            SetObjectReference(uiManager, "_hudRoot", hudRoot);
            SetObjectReference(uiManager, "_inventoryRoot", inventoryRoot);
            SetObjectReference(uiManager, "_gameOverPanel", gameOverRoot);
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject createdObject = GameObject.CreatePrimitive(type);
            createdObject.name = objectName;
            createdObject.transform.SetParent(parent);
            createdObject.transform.localPosition = localPosition;
            createdObject.transform.localRotation = Quaternion.identity;
            createdObject.transform.localScale = localScale;

            Renderer renderer = createdObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return createdObject;
        }

        private static GameObject CreateEmpty(string objectName, Transform parent, Vector3 localPosition)
        {
            GameObject createdObject = new GameObject(objectName);
            createdObject.transform.SetParent(parent);
            createdObject.transform.localPosition = localPosition;
            createdObject.transform.localRotation = Quaternion.identity;
            createdObject.transform.localScale = Vector3.one;
            return createdObject;
        }

        private static void CreateMarker(Transform parent, string objectName, Vector3 position)
        {
            GameObject marker = CreateEmpty(objectName, parent, position);
            marker.transform.localScale = Vector3.one;
        }

        private static void CreatePointLight(string objectName, Transform parent, Vector3 localPosition, Color color, float intensity, float range)
        {
            GameObject lightObject = CreateEmpty(objectName, parent, localPosition);
            Light pointLight = lightObject.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = color;
            pointLight.intensity = intensity;
            pointLight.range = range;
        }

        private static GameObject CreateUiPanel(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(objectName);
            panel.transform.SetParent(parent);

            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static void CreateUiText(string objectName, Transform parent, string message, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(16f, 12f);
            rectTransform.offsetMax = new Vector2(-16f, -12f);

            Text text = textObject.AddComponent<Text>();
            text.text = message;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                material.color = color;
                EditorUtility.SetDirty(material);
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material createdMaterial = new Material(shader);
            createdMaterial.color = color;
            AssetDatabase.CreateAsset(createdMaterial, path);
            return createdMaterial;
        }

        private static GameObject GetOrCreateRoot(Scene scene, string objectName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].name == objectName)
                {
                    return rootObjects[i];
                }
            }

            GameObject createdObject = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(createdObject, scene);
            return createdObject;
        }

        private static Transform FindChildByName(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindChildByName(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void RenameSceneObjects(Scene scene)
        {
            RenameObjectInScene(scene, "Cube_Ground_IcePlane", "Ground_IcePlane");
            RenameObjectInScene(scene, "Cube_HorizonRidge_North", "HorizonRidge_North");
            RenameObjectInScene(scene, "Cube_HorizonRidge_South", "HorizonRidge_South");
            RenameObjectInScene(scene, "Cube_HorizonRidge_East", "HorizonRidge_East");
            RenameObjectInScene(scene, "Cube_HorizonRidge_West", "HorizonRidge_West");
            RenameObjectInScene(scene, "Cube_ResourceNode_Stone_01", "ResourceNode_Stone_01");
            RenameObjectInScene(scene, "Cube_ResourceNode_Wood_01", "ResourceNode_Wood_01");
            RenameObjectInScene(scene, "Cylinder_BuildPoint_Wall_North", "BuildPoint_Wall_North");
            RenameObjectInScene(scene, "Cylinder_BuildPoint_Turret_East", "BuildPoint_Turret_East");
            RenameObjectInScene(scene, "GroundCheck", "Transform_GroundCheck");
            RenameObjectInScene(scene, "MainGame_RuntimeManagers", "MainGame_Managers");
        }

        private static void RenameCenterChunkPrefabObjects()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(_centerChunkPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogWarning("CinderkeepGameMapSceneBuilder: center chunk prefab was not found.");
                return;
            }

            RenameObjectInChildren(prefabRoot.transform, "Cube_Ground_IcePlane", "Ground_IcePlane");
            RenameObjectInChildren(prefabRoot.transform, "Cube_HorizonRidge_North", "HorizonRidge_North");
            RenameObjectInChildren(prefabRoot.transform, "Cube_HorizonRidge_South", "HorizonRidge_South");
            RenameObjectInChildren(prefabRoot.transform, "Cube_HorizonRidge_East", "HorizonRidge_East");
            RenameObjectInChildren(prefabRoot.transform, "Cube_HorizonRidge_West", "HorizonRidge_West");
            RenameObjectInChildren(prefabRoot.transform, "Cube_ResourceNode_Stone_01", "ResourceNode_Stone_01");
            RenameObjectInChildren(prefabRoot.transform, "Cube_ResourceNode_Wood_01", "ResourceNode_Wood_01");
            RenameObjectInChildren(prefabRoot.transform, "Cylinder_BuildPoint_Wall_North", "BuildPoint_Wall_North");
            RenameObjectInChildren(prefabRoot.transform, "Cylinder_BuildPoint_Turret_East", "BuildPoint_Turret_East");

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, _centerChunkPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        private static void RenameObjectInScene(Scene scene, string beforeName, string afterName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                RenameObjectInChildren(rootObjects[i].transform, beforeName, afterName);
            }
        }

        private static void RenameObjectInChildren(Transform root, string beforeName, string afterName)
        {
            if (root.name == beforeName)
            {
                root.name = afterName;
                EditorUtility.SetDirty(root.gameObject);
            }

            for (int i = 0; i < root.childCount; i++)
            {
                RenameObjectInChildren(root.GetChild(i), beforeName, afterName);
            }
        }

        private static void DestroyRootObjectIfExists(Scene scene, string objectName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].name == objectName)
                {
                    UnityEngine.Object.DestroyImmediate(rootObjects[i]);
                    return;
                }
            }
        }

        private static void MoveObjectUnderManagerGroup(Scene scene, string objectName)
        {
            Transform target = FindTransformInScene(scene, objectName);
            if (target == null)
            {
                return;
            }

            GameObject managerGroup = GetOrCreateRoot(scene, "MainGame_Managers");
            if (target.parent == managerGroup.transform)
            {
                return;
            }

            target.SetParent(managerGroup.transform);
            EditorUtility.SetDirty(target.gameObject);
            EditorUtility.SetDirty(managerGroup);
        }

        private static Transform FindTransformInScene(Scene scene, string objectName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].name == objectName)
                {
                    return rootObjects[i].transform;
                }

                Transform found = FindChildByName(rootObjects[i].transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void ClearChildren(Transform parent)
        {
            while (parent.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(parent.GetChild(0).gameObject);
            }
        }

        private static TComponent GetOrAddComponent<TComponent>(GameObject targetObject)
            where TComponent : Component
        {
            TComponent component = targetObject.GetComponent<TComponent>();
            if (component == null)
            {
                component = targetObject.AddComponent<TComponent>();
            }

            return component;
        }

        private static void SetObjectReference(Object targetObject, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning("CinderkeepGameMapSceneBuilder: property was not found. " + propertyName);
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void EnsureTag(string tagName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; i++)
            {
                SerializedProperty tag = tags.GetArrayElementAtIndex(i);
                if (tag.stringValue == tagName)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            SerializedProperty newTag = tags.GetArrayElementAtIndex(tags.arraySize - 1);
            newTag.stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }

        private static void DisableOtherCamerasAndListeners(Scene scene, Camera keepCamera, AudioListener keepListener)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                DisableOtherCamerasAndListenersInChildren(rootObjects[i].transform, keepCamera, keepListener);
            }
        }

        private static void DisableOtherCamerasAndListenersInChildren(Transform root, Camera keepCamera, AudioListener keepListener)
        {
            Camera camera = root.GetComponent<Camera>();
            if (camera != null && camera != keepCamera)
            {
                camera.enabled = false;
            }

            AudioListener audioListener = root.GetComponent<AudioListener>();
            if (audioListener != null && audioListener != keepListener)
            {
                audioListener.enabled = false;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                DisableOtherCamerasAndListenersInChildren(root.GetChild(i), keepCamera, keepListener);
            }
        }

        private static void ClearScene(Scene scene)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(rootObjects[i]);
            }
        }

    }
}
