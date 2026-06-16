using System.Collections.Generic;
using OODong.CharacterSelect;
using OODong.Shared;
using OODong.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace OODong.Cinderkeep.Editor
{
    // Cinderkeep 실제 게임 씬 전용 Editor Builder.
    // 중요: 작업 허브/워크스페이스를 건드리지 않고, Assets/Scenes/MainGame/Cinderkeep_Game.unity만 재생성한다.
    // 팀원은 허브 수정 요청이 별도로 있을 때만 CharacterSelectSceneBuilder 계열 전체 빌더를 사용한다.
    public static class CinderkeepPlayableSceneBuilder
    {
        private const string MainLobbyScenePath = "Assets/Scenes/Main_Lobby.unity";
        private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
        private const string GeneratedMaterialFolder = "Assets/MainAssets/Cinderkeep/GeneratedMaterials";
        private const string FontPath = "Assets/Fonts/ChosunCentennial.ttf";
        private const string ArrowModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/PolygonParticles/Models/FX_Arrow_01.fbx";
        private const string PlantPrefabPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/CarnivorousPlant/Prefabs/Carnivorous Plant-Green.prefab";
        private const string ApplePrefabPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FantasticFoodPack/prefabs/P_PROP_food_apple.prefab";
        private const string FireBowlModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FireBowl/KB3D_AOE_PropFireBowlON_A.fbx";
        private const string HeartModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/PolygonIce/FX_Heart_01.fbx";
        private const string FlameModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/PolygonIce/SM_Flame_FX.fbx";
        private const string IceCrystalModelPathA = "Assets/MainAssets/Cinderkeep/ExternalAssets/PolygonIce/FX_Crystal_01.fbx";
        private const string IceCrystalModelPathB = "Assets/MainAssets/Cinderkeep/ExternalAssets/PolygonIce/FX_Crystal_02.fbx";
        private const string IceCrystalModelPathC = "Assets/MainAssets/Cinderkeep/ExternalAssets/PolygonIce/FX_Crystal_03.fbx";
        private const string FrozenGateModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FrozenRuins/KB3D_DKF_Gate_A.fbx";
        private const string FrozenTowerModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FrozenRuins/KB3D_DKF_Tower_A.fbx";
        private const string FrozenTowerAltModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FrozenRuins/KB3D_DKF_Tower_E.fbx";
        private const string FrozenBuildingModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FrozenRuins/KB3D_DKF_BldgMD_A.fbx";
        private const string FrozenStatueModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FrozenRuins/KB3D_DKF_Statue_A.fbx";
        private const string FrozenFirewoodModelPath = "Assets/MainAssets/Cinderkeep/ExternalAssets/FrozenRuins/KB3D_DKF_Firewood_A.fbx";

        [MenuItem("OODong/Cinderkeep/Rebuild Playable Game Scene")]
        public static void RebuildPlayableGameScenes()
        {
            EnsureFolders();
            AssetDatabase.Refresh();
            RebuildPlayableScene(GameScenePath, "GameRoot_Cinderkeep", "Cinderkeep_Playable_V1");
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("OODong/Cinderkeep/Rebuild Cinderkeep Game Only")]
        public static void RebuildCinderkeepGameOnly()
        {
            // 사용자가 말한 "!! Play Our Game" 버튼이 로드하는 실제 게임 씬만 빌드업한다.
            // TODO(팀원 작업 요청): 게임 오브젝트 배치를 수동으로 다듬은 뒤에는 이 빌더를 돌리면 덮어써지므로 주의해 주세요.
            EnsureFolders();
            AssetDatabase.Refresh();
            RebuildPlayableScene(GameScenePath, "GameRoot_Cinderkeep", "Cinderkeep_Playable_V1");
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
        }

        public static void ValidatePlayableGameScenes()
        {
            ValidatePlayableScene(GameScenePath);
            Debug.Log("CinderkeepPlayableSceneBuilder: playable game scene validated.");
        }

        private static void RebuildPlayableScene(string scenePath, string sceneRootName, string title)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Light sun = ConfigureMoodLighting();
            CreateEventSystem();

            GameObject sceneRoot = new GameObject(sceneRootName);
            CreateManagers(sceneRoot.transform, out GameDataManager dataManager, out GameManager gameManager);
            GameObject player = CreatePlayer(sceneRoot.transform);
            CinderkeepInventory inventory = player.GetComponent<CinderkeepInventory>();
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            SceneCameraController sceneCameraController = playerCamera.GetComponent<SceneCameraController>();

            CreateField(sceneRoot.transform);
            CreateHorizon(sceneRoot.transform);
            CinderkeepFlameHeart flameHeart = CreateFlameHeart(sceneRoot.transform);
            CreateBuildSites(sceneRoot.transform);
            CreateGatheringNodes(sceneRoot.transform);
            CinderkeepEnemy enemyTemplate = CreateEnemyTemplate(sceneRoot.transform, flameHeart.transform);
            CinderkeepProjectile projectileTemplate = CreateProjectileTemplate(sceneRoot.transform);
            CinderkeepEnemySpawner enemySpawner = CreateEnemySpawner(sceneRoot.transform, enemyTemplate, flameHeart.transform, dataManager);
            Transform placedItemRoot = CreatePlacedItemRoot(sceneRoot.transform);
            GameObject stoneTemplate = CreatePlaceableTemplate(sceneRoot.transform, "Placeable_Template_Stone", GetOrCreateColorMaterial("Pickup_Stone.mat", new Color(0.42f, 0.44f, 0.43f, 1f)), new Vector3(0.6f, 0.35f, 0.6f));
            GameObject oreTemplate = CreatePlaceableTemplate(sceneRoot.transform, "Placeable_Template_Ore", GetOrCreateColorMaterial("Mineable_Ore.mat", new Color(0.12f, 0.48f, 0.9f, 1f)), new Vector3(0.8f, 0.8f, 0.8f));

            CinderkeepHudView hudView = CreateHud(title, inventory, sceneCameraController);
            CinderkeepPickaxeView pickaxeView = player.GetComponentInChildren<CinderkeepPickaxeView>();
            CinderkeepFirstPersonPlayer firstPersonPlayer = player.GetComponent<CinderkeepFirstPersonPlayer>();
            firstPersonPlayer.SetReferences(player.GetComponent<CharacterController>(), playerCamera, inventory, hudView, pickaxeView);
            EditorUtility.SetDirty(firstPersonPlayer);

            CinderkeepPlaceableItemFactory placeableFactory = player.GetComponent<CinderkeepPlaceableItemFactory>();
            placeableFactory.SetReferences(
                placedItemRoot,
                stoneTemplate,
                oreTemplate);
            EditorUtility.SetDirty(placeableFactory);

            CinderkeepAutoShooter autoShooter = player.GetComponent<CinderkeepAutoShooter>();
            autoShooter.SetFireOrigin(playerCamera.transform);
            autoShooter.SetProjectileTemplate(projectileTemplate);
            EditorUtility.SetDirty(autoShooter);

            gameManager.SetSceneReferences(dataManager, hudView, flameHeart, enemySpawner, inventory);
            EditorUtility.SetDirty(gameManager);

            GameFlowController flowController = CreateGameFlowController(sceneRoot.transform, gameManager, enemySpawner, flameHeart, sun);
            EditorUtility.SetDirty(flowController);

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static Light ConfigureMoodLighting()
        {
            Material skybox = GetOrCreateSkyboxMaterial();
            if (skybox != null)
            {
                RenderSettings.skybox = skybox;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.22f, 0.34f, 0.52f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.18f, 0.26f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.035f, 0.055f, 0.08f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.32f, 0.46f, 0.66f, 1f);
            RenderSettings.fogStartDistance = 28f;
            RenderSettings.fogEndDistance = 170f;

            GameObject lightObject = new GameObject("Sun_Directional_Light", typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.62f, 0.78f, 1f, 1f);
            light.intensity = 0.78f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(28f, -38f, 0f);

            GameObject fillLightObject = new GameObject("Frozen_Moon_Fill_Light", typeof(Light));
            Light fillLight = fillLightObject.GetComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.2f, 0.34f, 0.62f, 1f);
            fillLight.intensity = 0.22f;
            fillLight.shadows = LightShadows.None;
            fillLight.transform.rotation = Quaternion.Euler(18f, 142f, 0f);
            return light;
        }

        private static void CreateManagers(Transform parent, out GameDataManager dataManager, out GameManager gameManager)
        {
            GameObject managerRoot = new GameObject(
                "Managers",
                typeof(GameDataManager),
                typeof(GameObjectManager),
                typeof(ResourceManager),
                typeof(NetworkManager),
                typeof(AudioSource),
                typeof(SoundManager),
                typeof(GameManager));
            managerRoot.transform.SetParent(parent, false);
            dataManager = managerRoot.GetComponent<GameDataManager>();
            gameManager = managerRoot.GetComponent<GameManager>();
        }

        private static GameFlowController CreateGameFlowController(
            Transform parent,
            GameManager gameManager,
            CinderkeepEnemySpawner enemySpawner,
            CinderkeepFlameHeart flameHeart,
            Light sun)
        {
            GameObject controllerObject = new GameObject("GameFlow_Controller", typeof(GameFlowController));
            controllerObject.transform.SetParent(parent, false);
            GameFlowController controller = controllerObject.GetComponent<GameFlowController>();
            controller.SetReferences(gameManager, enemySpawner, flameHeart, sun);
            return controller;
        }

        private static GameObject CreatePlayer(Transform parent)
        {
            GameObject player = new GameObject(
                "FirstPerson_Player",
                typeof(CharacterController),
                typeof(CinderkeepInventory),
                typeof(CinderkeepFirstPersonPlayer),
                typeof(CinderkeepAutoShooter),
                typeof(CinderkeepPlaceableItemFactory));
            player.transform.SetParent(parent, false);
            player.transform.position = new Vector3(0f, 0.08f, 0f);

            CharacterController controller = player.GetComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.34f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            GameObject cameraObject = new GameObject("FirstPerson_Camera", typeof(Camera), typeof(AudioListener), typeof(SceneCameraController));
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            cameraObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 68f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 320f;

            SceneCameraController sceneCameraController = cameraObject.GetComponent<SceneCameraController>();
            sceneCameraController.SetCamera(camera);
            sceneCameraController.enabled = false;

            CreatePickaxeView(cameraObject.transform);
            return player;
        }

        private static void CreatePickaxeView(Transform cameraTransform)
        {
            GameObject pickaxeRoot = new GameObject("FirstPerson_Pickaxe_View", typeof(CinderkeepPickaxeView));
            pickaxeRoot.transform.SetParent(cameraTransform, false);
            pickaxeRoot.transform.localPosition = new Vector3(0.48f, -0.46f, 0.88f);
            pickaxeRoot.transform.localRotation = Quaternion.Euler(8f, -35f, -8f);
            pickaxeRoot.transform.localScale = Vector3.one * 0.78f;

            Material handleMaterial = GetOrCreateColorMaterial("Pickaxe_Handle.mat", new Color(0.34f, 0.21f, 0.12f, 1f));
            Material headMaterial = GetOrCreateColorMaterial("Pickaxe_Head.mat", new Color(0.55f, 0.58f, 0.62f, 1f));

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Pickaxe_Handle";
            handle.transform.SetParent(pickaxeRoot.transform, false);
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localRotation = Quaternion.Euler(0f, 0f, 34f);
            handle.transform.localScale = new Vector3(0.025f, 0.3f, 0.025f);
            handle.GetComponent<Renderer>().sharedMaterial = handleMaterial;
            Object.DestroyImmediate(handle.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Pickaxe_Head";
            head.transform.SetParent(pickaxeRoot.transform, false);
            head.transform.localPosition = new Vector3(0.11f, 0.13f, 0f);
            head.transform.localRotation = Quaternion.Euler(0f, 0f, 34f);
            head.transform.localScale = new Vector3(0.18f, 0.045f, 0.045f);
            head.GetComponent<Renderer>().sharedMaterial = headMaterial;
            Object.DestroyImmediate(head.GetComponent<Collider>());
        }

        private static Transform CreatePlacedItemRoot(Transform parent)
        {
            GameObject placedItemRoot = new GameObject("Placed_Item_Root");
            placedItemRoot.transform.SetParent(parent, false);
            return placedItemRoot.transform;
        }

        private static CinderkeepFlameHeart CreateFlameHeart(Transform parent)
        {
            Material baseMaterial = GetOrCreateColorMaterial("FlameHeart_Base.mat", new Color(0.12f, 0.12f, 0.14f, 1f));
            Material coreMaterial = GetOrCreateColorMaterial("FlameHeart_Core.mat", new Color(1f, 0.42f, 0.12f, 1f));

            GameObject heartObject = new GameObject(
                "FlameHeart_Core",
                typeof(CapsuleCollider),
                typeof(CinderkeepFlameHeart),
                typeof(CinderkeepWarmthAura),
                typeof(CinderkeepFlameBeam));
            heartObject.transform.SetParent(parent, false);
            heartObject.transform.position = new Vector3(0f, 0f, 7f);

            CapsuleCollider collider = heartObject.GetComponent<CapsuleCollider>();
            collider.height = 2.4f;
            collider.radius = 1.2f;
            collider.center = new Vector3(0f, 1.2f, 0f);

            CreateFlameHeartBaseVisual(heartObject.transform, baseMaterial);
            Renderer coreRenderer = CreateFlameHeartCoreVisual(heartObject.transform, coreMaterial);

            CinderkeepFlameHeart flameHeart = heartObject.GetComponent<CinderkeepFlameHeart>();
            flameHeart.SetCoreRenderer(coreRenderer);
            heartObject.GetComponent<CinderkeepFlameBeam>().SetFireOrigin(coreRenderer.transform);
            return flameHeart;
        }

        private static void CreateFlameHeartBaseVisual(Transform parent, Material baseMaterial)
        {
            GameObject fireBowlModel = AssetDatabase.LoadAssetAtPath<GameObject>(FireBowlModelPath);
            if (fireBowlModel != null)
            {
                GameObject fireBowl = PrefabUtility.InstantiatePrefab(fireBowlModel) as GameObject;
                if (fireBowl != null)
                {
                    fireBowl.name = "External_FireBowl_FlameHeart_Base";
                    fireBowl.transform.SetParent(parent, false);
                    fireBowl.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                    fireBowl.transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
                    FitVisualToLongestAxis(fireBowl, 2.8f);
                    ApplyMaterialToRenderers(fireBowl, baseMaterial);
                    DestroyCollidersInChildren(fireBowl);
                    return;
                }
            }

            GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseObject.name = "FlameHeart_Stone_Base";
            baseObject.transform.SetParent(parent, false);
            baseObject.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            baseObject.transform.localScale = new Vector3(1.6f, 0.25f, 1.6f);
            baseObject.GetComponent<Renderer>().sharedMaterial = baseMaterial;
            Object.DestroyImmediate(baseObject.GetComponent<Collider>());
        }

        private static Renderer CreateFlameHeartCoreVisual(Transform parent, Material coreMaterial)
        {
            // 프로젝트 내부에 heart/flame 전용 prefab이 없으므로 MVP용 절차형 FlameHeart를 만든다.
            // TODO(팀원 작업 요청): 전용 하트/불꽃 VFX prefab이 준비되면 이 메서드 내부를 prefab Instantiate로 교체해 주세요.
            GameObject coreRoot = new GameObject("FlameHeart_Procedural_HeartFlame");
            coreRoot.transform.SetParent(parent, false);
            coreRoot.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            GameObject heartModel = CreateExternalModelVisual(
                coreRoot.transform,
                HeartModelPath,
                "External_FlameHeart_Heart_Model",
                Vector3.zero,
                new Vector3(0f, 0f, 0f),
                1.25f,
                coreMaterial);

            GameObject flameModel = CreateExternalModelVisual(
                coreRoot.transform,
                FlameModelPath,
                "External_FlameHeart_Flame_Model",
                new Vector3(0f, 0.62f, 0f),
                new Vector3(0f, 0f, 0f),
                1.35f,
                coreMaterial);

            Renderer externalRenderer = GetFirstRenderer(heartModel);
            if (externalRenderer != null)
            {
                // TODO(팀 작업): 전용 FlameHeart prefab이 준비되면 이 모델 조합 대신 prefab Instantiate로 교체해 주세요.
                if (flameModel == null)
                {
                    CreateCoreFlameTongue(coreRoot.transform, coreMaterial);
                }

                CreateFlameHeartLight(coreRoot.transform);
                return externalRenderer;
            }

            Renderer mainRenderer = CreateCorePiece(coreRoot.transform, "Heart_Core_Left", new Vector3(-0.34f, 0.08f, 0f), new Vector3(0.78f, 0.72f, 0.62f), coreMaterial);
            CreateCorePiece(coreRoot.transform, "Heart_Core_Right", new Vector3(0.34f, 0.08f, 0f), new Vector3(0.78f, 0.72f, 0.62f), coreMaterial);
            CreateCorePiece(coreRoot.transform, "Heart_Core_Point", new Vector3(0f, -0.36f, 0f), new Vector3(0.82f, 0.9f, 0.58f), coreMaterial);

            CreateCoreFlameTongue(coreRoot.transform, coreMaterial);
            CreateFlameHeartLight(coreRoot.transform);
            return mainRenderer;
        }

        private static void CreateCoreFlameTongue(Transform parent, Material coreMaterial)
        {
            GameObject flameTongue = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            flameTongue.name = "Heart_Flame_Tongue";
            flameTongue.transform.SetParent(parent, false);
            flameTongue.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            flameTongue.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            flameTongue.transform.localScale = new Vector3(0.34f, 0.68f, 0.34f);
            flameTongue.GetComponent<Renderer>().sharedMaterial = coreMaterial;
            Object.DestroyImmediate(flameTongue.GetComponent<Collider>());
        }

        private static void CreateFlameHeartLight(Transform parent)
        {
            GameObject lightObject = new GameObject("FlameHeart_Orange_Point_Light", typeof(Light));
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.38f, 0.08f, 1f);
            light.intensity = 2.8f;
            light.range = 9f;
        }

        private static Renderer CreateCorePiece(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = localScale;
            Renderer renderer = piece.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            Object.DestroyImmediate(piece.GetComponent<Collider>());
            return renderer;
        }

        private static void CreateBuildSites(Transform parent)
        {
            CreateBuildSite(
                parent,
                "BuildSite_Wall_North",
                "wall",
                new Vector3(0f, 0f, 2.6f),
                new Vector3(3.6f, 0.25f, 0.45f),
                new Vector3(3.8f, 1.2f, 0.5f),
                GetOrCreateColorMaterial("BuildSite_Wall_Preview.mat", new Color(0.8f, 0.8f, 0.72f, 0.38f)));

            CreateBuildSite(
                parent,
                "BuildSite_Turret_East",
                "turret",
                new Vector3(4.3f, 0f, 7f),
                new Vector3(0.9f, 0.25f, 0.9f),
                new Vector3(1f, 1.8f, 1f),
                GetOrCreateColorMaterial("BuildSite_Turret_Preview.mat", new Color(0.96f, 0.48f, 0.2f, 0.45f)));

            CreateBuildSite(
                parent,
                "BuildSite_Trap_West",
                "trap",
                new Vector3(-4.2f, 0f, 6.6f),
                new Vector3(1.5f, 0.18f, 1.5f),
                new Vector3(1.5f, 0.28f, 1.5f),
                GetOrCreateColorMaterial("BuildSite_Trap_Preview.mat", new Color(0.7f, 0.88f, 0.95f, 0.42f)));
        }

        private static void CreateBuildSite(
            Transform parent,
            string name,
            string buildingDataId,
            Vector3 position,
            Vector3 previewScale,
            Vector3 structureScale,
            Material previewMaterial)
        {
            GameObject siteObject = new GameObject(name, typeof(BoxCollider), typeof(CinderkeepBuildSite));
            siteObject.transform.SetParent(parent, false);
            siteObject.transform.position = position;

            BoxCollider collider = siteObject.GetComponent<BoxCollider>();
            collider.size = new Vector3(2.4f, 1.4f, 2.4f);
            collider.center = new Vector3(0f, 0.7f, 0f);
            collider.isTrigger = true;

            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
            preview.name = "BuildSite_Preview";
            preview.transform.SetParent(siteObject.transform, false);
            preview.transform.localPosition = new Vector3(0f, 0.16f, 0f);
            preview.transform.localScale = previewScale;
            Renderer previewRenderer = preview.GetComponent<Renderer>();
            previewRenderer.sharedMaterial = previewMaterial;
            Object.DestroyImmediate(preview.GetComponent<Collider>());

            Material structureMaterial = GetOrCreateColorMaterial($"{buildingDataId}_Built.mat", new Color(0.72f, 0.62f, 0.46f, 1f));
            GameObject structure = GameObject.CreatePrimitive(buildingDataId == "turret" ? PrimitiveType.Cylinder : PrimitiveType.Cube);
            structure.name = $"Structure_{buildingDataId}";
            structure.transform.SetParent(siteObject.transform, false);
            structure.transform.localPosition = new Vector3(0f, structureScale.y * 0.5f, 0f);
            structure.transform.localScale = structureScale;
            Renderer structureRenderer = structure.GetComponent<Renderer>();
            structureRenderer.sharedMaterial = structureMaterial;
            CinderkeepStructure cinderkeepStructure = structure.AddComponent<CinderkeepStructure>();
            cinderkeepStructure.SetViewRenderer(structureRenderer);
            Object.DestroyImmediate(structure.GetComponent<Collider>());

            if (buildingDataId == "turret")
            {
                structure.AddComponent<CinderkeepFlameBeam>();
            }

            CinderkeepBuildSite buildSite = siteObject.GetComponent<CinderkeepBuildSite>();
            buildSite.SetBuildSite(buildingDataId, structure, cinderkeepStructure);
            buildSite.SetPreviewRenderer(previewRenderer);
            structure.SetActive(false);
        }

        private static GameObject CreatePlaceableTemplate(Transform parent, string name, Material material, Vector3 localScale)
        {
            GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
            template.name = name;
            template.transform.SetParent(parent, false);
            template.transform.position = new Vector3(0f, -12f, 0f);
            template.transform.localScale = localScale;
            template.GetComponent<Renderer>().sharedMaterial = material;
            template.SetActive(false);
            return template;
        }

        private static void CreateField(Transform parent)
        {
            Material snowMaterial = GetOrCreateColorMaterial("Cinderkeep_Ice_Snow.mat", new Color(0.72f, 0.84f, 0.94f, 1f));
            GameObject field = GameObject.CreatePrimitive(PrimitiveType.Cube);
            field.name = "Cinderkeep_Frozen_Wasteland";
            field.transform.SetParent(parent, false);
            field.transform.position = new Vector3(0f, -0.06f, 0f);
            field.transform.localScale = new Vector3(420f, 0.12f, 420f);
            field.GetComponent<Renderer>().sharedMaterial = snowMaterial;

            Material icePlateMaterial = GetOrCreateColorMaterial("Cinderkeep_Cracked_Ice_Plate.mat", new Color(0.42f, 0.62f, 0.78f, 1f));
            CreateBrokenIcePlates(parent, icePlateMaterial);

            Material ridgeMaterial = GetOrCreateColorMaterial("Cinderkeep_Frozen_Ridge.mat", new Color(0.16f, 0.25f, 0.36f, 1f));
            for (int i = 0; i < 14; i++)
            {
                float x = -154f + (i * 24f);
                float height = 12f + Mathf.Sin(i * 0.8f) * 5f;
                GameObject ridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ridge.name = $"Frozen_Horizon_Ridge_{i + 1:00}";
                ridge.transform.SetParent(parent, false);
                ridge.transform.position = new Vector3(x, height * 0.5f - 0.1f, 178f);
                ridge.transform.localScale = new Vector3(32f, height, 14f);
                ridge.GetComponent<Renderer>().sharedMaterial = ridgeMaterial;
            }

            Material treeTrunkMaterial = GetOrCreateColorMaterial("Dead_Frozen_Tree_Trunk.mat", new Color(0.12f, 0.1f, 0.1f, 1f));
            for (int i = 0; i < 24; i++)
            {
                float angle = i * 41f;
                float radius = 42f + (i % 6) * 16f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                CreateDeadTree(parent, position, treeTrunkMaterial, i);
            }

            Material iceCrystalMaterial = GetOrCreateColorMaterial("Frozen_Crystal_Cluster.mat", new Color(0.3f, 0.84f, 1f, 1f));
            CreateIceCrystalField(parent, iceCrystalMaterial);

            Material ruinMaterial = GetOrCreateColorMaterial("Frozen_Ruin_Stone.mat", new Color(0.2f, 0.24f, 0.3f, 1f));
            CreateFrozenRuins(parent, ruinMaterial);
        }

        private static void CreateBrokenIcePlates(Transform parent, Material icePlateMaterial)
        {
            for (int i = 0; i < 18; i++)
            {
                float x = -64f + (i % 6) * 26f;
                float z = -56f + (i / 6) * 42f;
                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = $"Cracked_Ice_Plate_{i + 1:00}";
                plate.transform.SetParent(parent, false);
                plate.transform.position = new Vector3(x, 0.005f, z);
                plate.transform.rotation = Quaternion.Euler(0f, i * 17f, 0f);
                plate.transform.localScale = new Vector3(15f + (i % 3) * 4f, 0.025f, 9f + (i % 4) * 3f);
                plate.GetComponent<Renderer>().sharedMaterial = icePlateMaterial;
                Object.DestroyImmediate(plate.GetComponent<Collider>());
            }
        }

        private static void CreateDeadTree(Transform parent, Vector3 position, Material trunkMaterial, int index)
        {
            GameObject root = new GameObject($"Dead_Frozen_Tree_{index + 1:00}");
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 1.45f, 0f);
            trunk.transform.localRotation = Quaternion.Euler(0f, 0f, (index % 2 == 0 ? 6f : -5f));
            trunk.transform.localScale = new Vector3(0.22f, 1.45f + (index % 4) * 0.2f, 0.22f);
            trunk.GetComponent<Renderer>().sharedMaterial = trunkMaterial;
            Object.DestroyImmediate(trunk.GetComponent<Collider>());

            for (int branchIndex = 0; branchIndex < 3; branchIndex++)
            {
                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                branch.name = $"Branch_{branchIndex + 1:00}";
                branch.transform.SetParent(root.transform, false);
                branch.transform.localPosition = new Vector3(0f, 2.2f + branchIndex * 0.35f, 0f);
                branch.transform.localRotation = Quaternion.Euler(0f, branchIndex * 78f + index * 11f, 26f - branchIndex * 9f);
                branch.transform.localScale = new Vector3(0.12f, 1.15f - branchIndex * 0.18f, 0.12f);
                branch.GetComponent<Renderer>().sharedMaterial = trunkMaterial;
                Object.DestroyImmediate(branch.GetComponent<Collider>());
            }
        }

        private static void CreateIceCrystalField(Transform parent, Material crystalMaterial)
        {
            string[] crystalPaths =
            {
                IceCrystalModelPathA,
                IceCrystalModelPathB,
                IceCrystalModelPathC
            };

            Vector3[] positions =
            {
                new Vector3(20f, 0f, 18f),
                new Vector3(-24f, 0f, 22f),
                new Vector3(52f, 0f, -28f),
                new Vector3(-58f, 0f, -36f),
                new Vector3(82f, 0f, 52f),
                new Vector3(-96f, 0f, 66f),
                new Vector3(118f, 0f, -72f),
                new Vector3(-128f, 0f, -84f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                string crystalPath = crystalPaths[i % crystalPaths.Length];
                GameObject crystal = CreateExternalModelVisual(parent, crystalPath, $"External_Ice_Crystal_Cluster_{i + 1:00}", positions[i], new Vector3(0f, i * 34f, 0f), 3.4f + (i % 3), crystalMaterial);
                if (crystal == null)
                {
                    CreateFallbackIceShard(parent, positions[i], crystalMaterial, i);
                }
            }
        }

        private static void CreateFallbackIceShard(Transform parent, Vector3 position, Material crystalMaterial, int index)
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shard.name = $"Fallback_Ice_Shard_{index + 1:00}";
            shard.transform.SetParent(parent, false);
            shard.transform.position = position + new Vector3(0f, 1.1f, 0f);
            shard.transform.rotation = Quaternion.Euler(0f, index * 31f, 14f);
            shard.transform.localScale = new Vector3(0.8f, 2.2f + (index % 3), 0.8f);
            shard.GetComponent<Renderer>().sharedMaterial = crystalMaterial;
            Object.DestroyImmediate(shard.GetComponent<Collider>());
        }

        private static void CreateFrozenRuins(Transform parent, Material ruinMaterial)
        {
            CreateExternalModelVisual(parent, FrozenGateModelPath, "Frozen_Ruin_Gate_North", new Vector3(0f, 0f, 84f), new Vector3(0f, 180f, 0f), 18f, ruinMaterial);
            CreateExternalModelVisual(parent, FrozenBuildingModelPath, "Frozen_Ruin_Building_West", new Vector3(-78f, 0f, 58f), new Vector3(0f, 35f, 0f), 24f, ruinMaterial);
            CreateExternalModelVisual(parent, FrozenTowerModelPath, "Frozen_Ruin_Tower_East", new Vector3(84f, 0f, 66f), new Vector3(0f, -28f, 0f), 22f, ruinMaterial);
            CreateExternalModelVisual(parent, FrozenTowerAltModelPath, "Frozen_Ruin_Tower_South", new Vector3(-64f, 0f, -82f), new Vector3(0f, 128f, 0f), 20f, ruinMaterial);
            CreateExternalModelVisual(parent, FrozenStatueModelPath, "Frozen_Ruin_Statue_Broken", new Vector3(42f, 0f, -56f), new Vector3(0f, -45f, 0f), 8f, ruinMaterial);
            CreateExternalModelVisual(parent, FrozenFirewoodModelPath, "Frozen_Ruin_Firewood_Near_Heart", new Vector3(2.4f, 0f, 9.8f), new Vector3(0f, 18f, 0f), 2.8f, ruinMaterial);
        }

        private static void CreateHorizon(Transform parent)
        {
            GameObject horizonRoot = new GameObject("Frozen_Apocalypse_Horizon");
            horizonRoot.transform.SetParent(parent, false);

            Material mistMaterial = GetOrCreateColorMaterial("Frozen_Horizon_Mist.mat", new Color(0.18f, 0.28f, 0.42f, 0.68f));
            Material silhouetteMaterial = GetOrCreateColorMaterial("Frozen_Horizon_Silhouette.mat", new Color(0.045f, 0.07f, 0.11f, 1f));
            Material glacierMaterial = GetOrCreateColorMaterial("Frozen_Horizon_Glacier.mat", new Color(0.34f, 0.52f, 0.68f, 1f));

            for (int i = 0; i < 5; i++)
            {
                GameObject mist = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mist.name = $"Frozen_Horizon_Mist_Band_{i + 1:00}";
                mist.transform.SetParent(horizonRoot.transform, false);
                mist.transform.position = new Vector3(0f, 5f + i * 3.4f, 190f + i * 2f);
                mist.transform.localScale = new Vector3(340f - i * 32f, 2f, 0.35f);
                mist.GetComponent<Renderer>().sharedMaterial = mistMaterial;
                Object.DestroyImmediate(mist.GetComponent<Collider>());
            }

            for (int i = 0; i < 10; i++)
            {
                float x = -150f + i * 33f;
                GameObject glacier = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glacier.name = $"Frozen_Glacier_Wall_{i + 1:00}";
                glacier.transform.SetParent(horizonRoot.transform, false);
                glacier.transform.position = new Vector3(x, 10f + (i % 3) * 2f, 202f);
                glacier.transform.rotation = Quaternion.Euler(0f, i * 6f, (i % 2 == 0 ? -8f : 7f));
                glacier.transform.localScale = new Vector3(24f, 22f + (i % 4) * 5f, 10f);
                glacier.GetComponent<Renderer>().sharedMaterial = glacierMaterial;
                Object.DestroyImmediate(glacier.GetComponent<Collider>());
            }

            CreateExternalModelVisual(horizonRoot.transform, FrozenTowerModelPath, "Frozen_Horizon_Ruined_Tower_Left", new Vector3(-126f, 0f, 176f), new Vector3(0f, 15f, 0f), 28f, silhouetteMaterial);
            CreateExternalModelVisual(horizonRoot.transform, FrozenBuildingModelPath, "Frozen_Horizon_Ruined_Building_Center", new Vector3(12f, 0f, 184f), new Vector3(0f, -8f, 0f), 34f, silhouetteMaterial);
            CreateExternalModelVisual(horizonRoot.transform, FrozenTowerAltModelPath, "Frozen_Horizon_Ruined_Tower_Right", new Vector3(132f, 0f, 178f), new Vector3(0f, -22f, 0f), 30f, silhouetteMaterial);
        }

        private static void CreateGatheringNodes(Transform parent)
        {
            Material stoneMaterial = GetOrCreateColorMaterial("Pickup_Stone.mat", new Color(0.42f, 0.44f, 0.43f, 1f));
            Material oreMaterial = GetOrCreateColorMaterial("Mineable_Ore.mat", new Color(0.12f, 0.48f, 0.9f, 1f));
            Material chestMaterial = GetOrCreateColorMaterial("Chest_Yellow.mat", new Color(0.86f, 0.62f, 0.16f, 1f));

            Vector3[] stonePositions =
            {
                new Vector3(1.8f, 0.25f, 2.8f),
                new Vector3(-2.4f, 0.25f, 3.1f),
                new Vector3(3.8f, 0.25f, -1.6f)
            };

            for (int i = 0; i < stonePositions.Length; i++)
            {
                GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = $"Pickup_Stone_{i + 1:00}";
                stone.transform.SetParent(parent, false);
                stone.transform.position = stonePositions[i];
                stone.transform.localScale = new Vector3(0.46f, 0.32f, 0.42f);
                stone.GetComponent<Renderer>().sharedMaterial = stoneMaterial;
                stone.AddComponent<CinderkeepPickupNode>().SetPickup(CinderkeepItemId.Stone, 1, "Stone");
            }

            Vector3[] orePositions =
            {
                new Vector3(8f, 0.85f, 7f),
                new Vector3(-10f, 0.85f, 8f),
                new Vector3(14f, 0.85f, -9f),
                new Vector3(-16f, 0.85f, -6f)
            };

            for (int i = 0; i < orePositions.Length; i++)
            {
                GameObject ore = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ore.name = $"Mineable_Blue_Ore_{i + 1:00}";
                ore.transform.SetParent(parent, false);
                ore.transform.position = orePositions[i];
                ore.transform.rotation = Quaternion.Euler(0f, 35f * i, 11f);
                ore.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
                ore.GetComponent<Renderer>().sharedMaterial = oreMaterial;
                ore.AddComponent<CinderkeepMineableNode>().SetMineable("Blue Ore", 3, CinderkeepItemId.Ore, 1);
            }

            Vector3[] chestPositions =
            {
                new Vector3(6f, 0.55f, -5f),
                new Vector3(-8f, 0.55f, -8f)
            };

            for (int i = 0; i < chestPositions.Length; i++)
            {
                GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chest.name = $"Chest_Yellow_Box_{i + 1:00}";
                chest.transform.SetParent(parent, false);
                chest.transform.position = chestPositions[i];
                chest.transform.localScale = new Vector3(1.4f, 1f, 1f);
                chest.GetComponent<Renderer>().sharedMaterial = chestMaterial;
                chest.AddComponent<CinderkeepChestNode>().SetReward(i == 0 ? CinderkeepItemId.Apple : CinderkeepItemId.Stone, i == 0 ? 1 : 2);
                CreateAppleVisual(chest.transform, i);
            }
        }

        private static void CreateAppleVisual(Transform parent, int index)
        {
            GameObject applePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ApplePrefabPath);
            if (applePrefab == null || index != 0)
            {
                return;
            }

            GameObject apple = PrefabUtility.InstantiatePrefab(applePrefab) as GameObject;
            if (apple == null)
            {
                return;
            }

            apple.name = "External_Apple_Reward_Visual";
            apple.transform.SetParent(parent, false);
            apple.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            apple.transform.localScale = Vector3.one * 2f;
        }

        private static CinderkeepEnemy CreateEnemyTemplate(Transform parent, Transform target)
        {
            GameObject enemyRoot = new GameObject(
                "Enemy_Template_CarnivorousPlant",
                typeof(CapsuleCollider),
                typeof(Rigidbody),
                typeof(CinderkeepEnemyMovement),
                typeof(CinderkeepEnemyAttack),
                typeof(CinderkeepEnemy));
            enemyRoot.transform.SetParent(parent, false);
            enemyRoot.transform.position = new Vector3(0f, 0.05f, 0f);

            CapsuleCollider collider = enemyRoot.GetComponent<CapsuleCollider>();
            collider.height = 2.2f;
            collider.radius = 0.55f;
            collider.center = new Vector3(0f, 1.1f, 0f);

            Rigidbody rigidbody = enemyRoot.GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            CinderkeepEnemy enemy = enemyRoot.GetComponent<CinderkeepEnemy>();
            enemy.SetTarget(target);
            enemy.Configure(GameDataManager.Instance != null ? GameDataManager.Instance.GetEnemyData("plant") : null, false);

            GameObject plantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlantPrefabPath);
            if (plantPrefab != null)
            {
                GameObject visual = PrefabUtility.InstantiatePrefab(plantPrefab) as GameObject;
                if (visual != null)
                {
                    visual.name = "External_CarnivorousPlant_Visual";
                    visual.transform.SetParent(enemyRoot.transform, false);
                    visual.transform.localPosition = Vector3.zero;
                    FitVisualToHeight(visual, 2.1f);
                }
            }
            else
            {
                Material fallbackMaterial = GetOrCreateColorMaterial("Enemy_Red.mat", new Color(0.76f, 0.12f, 0.1f, 1f));
                GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.name = "Fallback_Red_Enemy_Visual";
                fallback.transform.SetParent(enemyRoot.transform, false);
                fallback.transform.localPosition = new Vector3(0f, 1.05f, 0f);
                fallback.transform.localScale = new Vector3(1f, 1.1f, 1f);
                fallback.GetComponent<Renderer>().sharedMaterial = fallbackMaterial;
                Object.DestroyImmediate(fallback.GetComponent<Collider>());
            }

            enemyRoot.SetActive(false);
            return enemy;
        }

        private static void CreateInitialEnemies(Transform parent, CinderkeepEnemy enemyTemplate, Transform target)
        {
            Vector3[] positions =
            {
                new Vector3(18f, 0.05f, 15f),
                new Vector3(-20f, 0.05f, 18f),
                new Vector3(22f, 0.05f, -14f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                CinderkeepEnemy enemy = Object.Instantiate(enemyTemplate, positions[i], Quaternion.identity, parent);
                enemy.name = $"Enemy_CarnivorousPlant_{i + 1:00}";
                enemy.SetTarget(target);
                enemy.gameObject.SetActive(true);
            }
        }

        private static CinderkeepEnemySpawner CreateEnemySpawner(Transform parent, CinderkeepEnemy enemyTemplate, Transform target, GameDataManager dataManager)
        {
            GameObject spawnerObject = new GameObject("Enemy_Spawner_NightWave", typeof(CinderkeepEnemySpawner));
            spawnerObject.transform.SetParent(parent, false);
            CinderkeepEnemySpawner spawner = spawnerObject.GetComponent<CinderkeepEnemySpawner>();
            spawner.SetEnemyTemplate(enemyTemplate);
            spawner.SetTarget(target);
            spawner.SetGameDataManager(dataManager);
            spawner.SetSpawningEnabled(false);
            EditorUtility.SetDirty(spawner);
            return spawner;
        }

        private static CinderkeepProjectile CreateProjectileTemplate(Transform parent)
        {
            Material projectileMaterial = GetOrCreateColorMaterial("Projectile_Arrow.mat", new Color(0.68f, 0.56f, 0.38f, 1f));
            GameObject projectileObject = new GameObject(
                "Projectile_Template_Arrow",
                typeof(SphereCollider),
                typeof(Rigidbody),
                typeof(CinderkeepProjectile));
            projectileObject.transform.SetParent(parent, false);

            SphereCollider collider = projectileObject.GetComponent<SphereCollider>();
            collider.radius = 0.16f;
            collider.isTrigger = true;

            Rigidbody rigidbody = projectileObject.GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            CreateArrowVisual(projectileObject.transform, projectileMaterial);

            CinderkeepProjectile projectile = projectileObject.GetComponent<CinderkeepProjectile>();
            projectileObject.SetActive(false);
            return projectile;
        }

        private static void CreateArrowVisual(Transform parent, Material projectileMaterial)
        {
            GameObject arrowModel = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowModelPath);
            if (arrowModel != null)
            {
                GameObject visual = PrefabUtility.InstantiatePrefab(arrowModel) as GameObject;
                if (visual != null)
                {
                    visual.name = "External_FX_Arrow_Visual";
                    visual.transform.SetParent(parent, false);
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    ApplyMaterialToRenderers(visual, projectileMaterial);
                    DestroyCollidersInChildren(visual);
                    FitVisualToLongestAxis(visual, 0.75f);
                    return;
                }

            }

            CreatePrimitiveArrowVisual(parent, projectileMaterial);
        }

        private static void CreatePrimitiveArrowVisual(Transform parent, Material projectileMaterial)
        {
            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Fallback_Arrow_Shaft";
            shaft.transform.SetParent(parent, false);
            shaft.transform.localPosition = Vector3.zero;
            shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shaft.transform.localScale = new Vector3(0.025f, 0.32f, 0.025f);
            shaft.GetComponent<Renderer>().sharedMaterial = projectileMaterial;
            Object.DestroyImmediate(shaft.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Fallback_Arrow_Head";
            head.transform.SetParent(parent, false);
            head.transform.localPosition = new Vector3(0f, 0f, 0.38f);
            head.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            head.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            head.GetComponent<Renderer>().sharedMaterial = projectileMaterial;
            Object.DestroyImmediate(head.GetComponent<Collider>());
        }

        private static CinderkeepHudView CreateHud(string title, CinderkeepInventory inventory, SceneCameraController sceneCameraController)
        {
            GameObject canvasObject = new GameObject(
                "Canvas_Cinderkeep_HUD",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(UIManager),
                typeof(CinderkeepHudView));
            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            UIManager uiManager = canvasObject.GetComponent<UIManager>();
            uiManager.SetCameraController(sceneCameraController);

            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            CreateText("HUD_Title", root, title, 28, new Color(0.92f, 0.95f, 0.9f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold, font, new Vector2(0.03f, 0.92f), new Vector2(0.42f, 0.98f));
            Text phaseText = CreateText("HUD_Phase_Text", root, "Cinderkeep Day 1/3  03:00", 22, new Color(0.95f, 0.82f, 0.48f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold, font, new Vector2(0.03f, 0.865f), new Vector2(0.54f, 0.915f));
            Image phaseBack = CreateImage("HUD_Phase_Progress_Back", root, new Color(0.01f, 0.03f, 0.05f, 0.72f), new Vector2(0.03f, 0.852f), new Vector2(0.54f, 0.862f));
            Image phaseFill = CreateImage("HUD_Phase_Progress_Fill", phaseBack.rectTransform, new Color(0.23f, 0.76f, 1f, 0.92f), Vector2.zero, Vector2.one);
            phaseFill.type = Image.Type.Filled;
            phaseFill.fillMethod = Image.FillMethod.Horizontal;
            phaseFill.fillAmount = 0f;
            Text flameHeartText = CreateText("HUD_FlameHeart_Text", root, "FlameHeart 180/180", 20, new Color(1f, 0.5f, 0.22f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold, font, new Vector2(0.03f, 0.815f), new Vector2(0.46f, 0.86f));
            Text objectiveText = CreateText("HUD_Objective_Text", root, "Gather resources and build fixed defense sites.", 18, new Color(0.78f, 0.9f, 0.82f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold, font, new Vector2(0.03f, 0.77f), new Vector2(0.62f, 0.812f));
            Text statusText = CreateText("HUD_Status_Text", root, "Tab/I: Inventory, E: Interact, Left Click: Use quick slot", 18, new Color(0.92f, 0.9f, 0.78f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold, font, new Vector2(0.03f, 0.725f), new Vector2(0.7f, 0.765f));

            Text crosshair = CreateText("HUD_Crosshair", root, "+", 32, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, font, new Vector2(0.49f, 0.48f), new Vector2(0.51f, 0.52f));
            crosshair.raycastTarget = false;
            Text promptText = CreateText("HUD_Interaction_Prompt", root, string.Empty, 24, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, font, new Vector2(0.34f, 0.18f), new Vector2(0.66f, 0.24f));

            Image miningBack = CreateImage("HUD_Mining_Back", root, new Color(0f, 0f, 0f, 0.58f), new Vector2(0.36f, 0.13f), new Vector2(0.64f, 0.16f));
            Image miningFill = CreateImage("HUD_Mining_Fill", miningBack.rectTransform, new Color(0.16f, 0.62f, 0.95f, 0.9f), Vector2.zero, Vector2.one);
            miningFill.type = Image.Type.Filled;
            miningFill.fillMethod = Image.FillMethod.Horizontal;
            miningFill.fillAmount = 0f;
            Text miningText = CreateText("HUD_Mining_Text", root, string.Empty, 18, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, font, new Vector2(0.36f, 0.13f), new Vector2(0.64f, 0.16f));

            CinderkeepQuickSlotDropTarget[] quickSlots = CreateQuickSlots(root, font);
            CinderkeepInventoryItemDragView[] itemRows = CreateInventoryPanel(root, font, out RectTransform inventoryPanel);
            CreateBackToLobbyButton(root, font);

            CinderkeepHudView hudView = canvasObject.GetComponent<CinderkeepHudView>();
            hudView.SetViewReferences(promptText, statusText, miningText, phaseText, flameHeartText, objectiveText, phaseFill, miningFill, inventoryPanel.gameObject, itemRows, quickSlots);
            hudView.SetInventory(inventory);
            hudView.SetInventoryOpen(false);
            EditorUtility.SetDirty(hudView);
            return hudView;
        }

        private static CinderkeepQuickSlotDropTarget[] CreateQuickSlots(RectTransform root, Font font)
        {
            RectTransform panel = CreateRect("HUD_QuickSlot_Panel", root, new Vector2(0.28f, 0.03f), new Vector2(0.72f, 0.12f));
            CinderkeepQuickSlotDropTarget[] quickSlots = new CinderkeepQuickSlotDropTarget[CinderkeepInventoryModel.QuickSlotCount];

            for (int i = 0; i < quickSlots.Length; i++)
            {
                float width = 1f / quickSlots.Length;
                RectTransform slotRoot = CreateRect($"QuickSlot_{i + 1}", panel, new Vector2(i * width + 0.005f, 0.04f), new Vector2((i + 1) * width - 0.005f, 0.96f));
                Image frame = CreateImage("Frame", slotRoot, new Color(0.08f, 0.12f, 0.14f, 0.9f), Vector2.zero, Vector2.one);
                Text label = CreateText("Label", slotRoot, $"{i + 1}\nEmpty", 17, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, font, Vector2.zero, Vector2.one);
                CinderkeepQuickSlotDropTarget dropTarget = slotRoot.gameObject.AddComponent<CinderkeepQuickSlotDropTarget>();
                dropTarget.SetSlotIndex(i);
                dropTarget.SetViewReferences(label, frame);
                quickSlots[i] = dropTarget;
            }

            return quickSlots;
        }

        private static CinderkeepInventoryItemDragView[] CreateInventoryPanel(RectTransform root, Font font, out RectTransform panel)
        {
            panel = CreateRect("HUD_Inventory_Panel", root, new Vector2(0.77f, 0.42f), new Vector2(0.97f, 0.86f));
            CreateImage("Inventory_Background", panel, new Color(0.02f, 0.04f, 0.05f, 0.78f), Vector2.zero, Vector2.one);
            CreateText("Inventory_Title", panel, "Inventory / Drag to 1-7", 20, new Color(0.95f, 0.75f, 0.34f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold, font, new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.98f));

            CinderkeepItemId[] itemIds =
            {
                CinderkeepItemId.Arrow,
                CinderkeepItemId.Pickaxe,
                CinderkeepItemId.Stone,
                CinderkeepItemId.Ore,
                CinderkeepItemId.Apple
            };

            CinderkeepInventoryItemDragView[] rows = new CinderkeepInventoryItemDragView[itemIds.Length];
            for (int i = 0; i < itemIds.Length; i++)
            {
                float top = 0.8f - (i * 0.14f);
                RectTransform row = CreateRect($"Inventory_Item_{itemIds[i]}", panel, new Vector2(0.07f, top - 0.1f), new Vector2(0.93f, top));
                Image frame = CreateImage("Frame", row, new Color(0.08f, 0.08f, 0.08f, 0.75f), Vector2.zero, Vector2.one);
                Text label = CreateText("Label", row, $"{CinderkeepItemCatalog.GetDisplayName(itemIds[i])} x0", 18, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold, font, new Vector2(0.08f, 0f), Vector2.one);
                CanvasGroup canvasGroup = row.gameObject.AddComponent<CanvasGroup>();
                CinderkeepInventoryItemDragView dragView = row.gameObject.AddComponent<CinderkeepInventoryItemDragView>();
                dragView.SetItem(itemIds[i]);
                dragView.SetDefaultQuickSlotIndex(GetDefaultQuickSlotIndex(itemIds[i]));
                dragView.SetViewReferences(label, frame, canvasGroup);
                rows[i] = dragView;
            }

            CreateText("Inventory_Hint", panel, "Double click: Arrow=1, Pickaxe=2", 15, new Color(0.74f, 0.82f, 0.8f, 1f), TextAnchor.MiddleCenter, FontStyle.Normal, font, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.1f));
            return rows;
        }

        private static int GetDefaultQuickSlotIndex(CinderkeepItemId itemId)
        {
            switch (itemId)
            {
                case CinderkeepItemId.Arrow:
                    return 0;
                case CinderkeepItemId.Pickaxe:
                    return 1;
                default:
                    return 0;
            }
        }

        private static void CreateBackToLobbyButton(RectTransform root, Font font)
        {
            RectTransform buttonRoot = CreateRect("Back_To_Lobby_Button", root, new Vector2(0.84f, 0.91f), new Vector2(0.97f, 0.97f));
            Image image = buttonRoot.gameObject.AddComponent<Image>();
            image.color = new Color(0.9f, 0.62f, 0.16f, 0.94f);
            Button button = buttonRoot.gameObject.AddComponent<Button>();
            Text label = CreateText("Label", buttonRoot, "Back To Hub", 18, new Color(0.06f, 0.06f, 0.06f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold, font, Vector2.zero, Vector2.one);
            label.raycastTarget = false;

            CharacterSceneLoadButton loader = buttonRoot.gameObject.AddComponent<CharacterSceneLoadButton>();
            loader.SetSceneName("Main_Lobby");
            UnityEventTools.AddPersistentListener(button.onClick, loader.LoadScene);
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform CreateRect(string name, RectTransform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static Image CreateImage(string name, RectTransform parent, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.rectTransform.anchorMin = anchorMin;
            image.rectTransform.anchorMax = anchorMax;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            return image;
        }

        private static Text CreateText(
            string name,
            RectTransform parent,
            string value,
            int size,
            Color color,
            TextAnchor alignment,
            FontStyle style,
            Font font,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.rectTransform.anchorMin = anchorMin;
            text.rectTransform.anchorMax = anchorMax;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return text;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static Material GetOrCreateSkyboxMaterial()
        {
            string path = $"{GeneratedMaterialFolder}/Cinderkeep_Procedural_Skybox.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Skybox/Procedural");
                if (shader == null)
                {
                    return null;
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            SetMaterialColor(material, "_SkyTint", new Color(0.24f, 0.38f, 0.62f, 1f));
            if (material.HasProperty("_GroundColor"))
            {
                material.SetColor("_GroundColor", new Color(0.035f, 0.07f, 0.12f, 1f));
            }

            if (material.HasProperty("_Exposure"))
            {
                material.SetFloat("_Exposure", 0.86f);
            }

            if (material.HasProperty("_SunSize"))
            {
                material.SetFloat("_SunSize", 0.028f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateColorMaterial(string fileName, Color color)
        {
            string path = $"{GeneratedMaterialFolder}/{fileName}";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            SetMaterialColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateTextureMaterial(string fileName, Texture2D texture)
        {
            string path = $"{GeneratedMaterialFolder}/{fileName}";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Unlit/Texture");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                }

                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            SetMaterialColor(material, "_BaseColor", color);
            SetMaterialColor(material, "_Color", color);
        }

        private static void SetMaterialColor(Material material, string propertyName, Color color)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void ApplyMaterialToRenderers(GameObject visual, Material material)
        {
            if (visual == null || material == null)
            {
                return;
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
        }

        private static GameObject CreateExternalModelVisual(
            Transform parent,
            string assetPath,
            string name,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            float targetLongestAxis,
            Material material)
        {
            GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (sourceModel == null)
            {
                return null;
            }

            GameObject visual = PrefabUtility.InstantiatePrefab(sourceModel) as GameObject;
            if (visual == null)
            {
                return null;
            }

            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = Quaternion.Euler(localEulerAngles);
            FitVisualToLongestAxis(visual, targetLongestAxis);
            ApplyMaterialToRenderers(visual, material);
            DestroyCollidersInChildren(visual);
            return visual;
        }

        private static Renderer GetFirstRenderer(GameObject visual)
        {
            return visual != null ? visual.GetComponentInChildren<Renderer>() : null;
        }

        private static void DestroyCollidersInChildren(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Collider[] colliders = visual.GetComponentsInChildren<Collider>();
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void FitVisualToHeight(GameObject visual, float targetHeight)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (bounds.size.y <= 0.01f)
            {
                return;
            }

            float scale = targetHeight / bounds.size.y;
            visual.transform.localScale *= scale;
        }

        private static void FitVisualToLongestAxis(GameObject visual, float targetSize)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float longestAxis = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (longestAxis <= 0.01f)
            {
                return;
            }

            float scale = targetSize / longestAxis;
            visual.transform.localScale *= scale;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/MainAssets"))
            {
                AssetDatabase.CreateFolder("Assets", "MainAssets");
            }

            if (!AssetDatabase.IsValidFolder("Assets/MainAssets/Cinderkeep"))
            {
                AssetDatabase.CreateFolder("Assets/MainAssets", "Cinderkeep");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedMaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets/MainAssets/Cinderkeep", "GeneratedMaterials");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes/MainGame"))
            {
                AssetDatabase.CreateFolder("Assets/Scenes", "MainGame");
            }
        }

        private static void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            AddBuildScene(scenes, MainLobbyScenePath);
            AddBuildScene(scenes, "Assets/Scenes/CharacterScenes/SampleScene.unity");
            AddBuildScene(scenes, GameScenePath);

            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < currentScenes.Length; i++)
            {
                AddBuildScene(scenes, currentScenes[i].path);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddBuildScene(List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return;
            }

            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        private static void ValidatePlayableScene(string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (Object.FindFirstObjectByType<CinderkeepFirstPersonPlayer>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no first person player.");
            }

            if (Object.FindFirstObjectByType<CinderkeepHudView>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no HUD view.");
            }

            if (Object.FindFirstObjectByType<CinderkeepEnemySpawner>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no enemy spawner.");
            }

            if (Object.FindFirstObjectByType<GameManager>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no GameManager.");
            }

            if (Object.FindFirstObjectByType<GameFlowController>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no GameFlowController.");
            }

            if (Object.FindFirstObjectByType<GameDataManager>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no GameDataManager.");
            }

            if (Object.FindFirstObjectByType<GameObjectManager>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no GameObjectManager.");
            }

            if (Object.FindFirstObjectByType<CinderkeepFlameHeart>() == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no FlameHeart.");
            }

            if (GameObject.Find("Sun_Directional_Light") == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no mood directional light.");
            }

            if (GameObject.Find("Cinderkeep_Frozen_Wasteland") == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no frozen wasteland field.");
            }

            if (GameObject.Find("Frozen_Apocalypse_Horizon") == null)
            {
                throw new System.InvalidOperationException($"{scenePath} has no frozen apocalypse horizon.");
            }

            if (!HasSceneGameObject("Projectile_Template_Arrow"))
            {
                throw new System.InvalidOperationException($"{scenePath} has no arrow projectile template.");
            }

            if (!HasSceneGameObject("BuildSite_Wall_North"))
            {
                throw new System.InvalidOperationException($"{scenePath} has no fixed wall build site.");
            }
        }

        private static bool HasSceneGameObject(string objectName)
        {
            GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject gameObject = gameObjects[i];
                if (gameObject.name == objectName && gameObject.scene.IsValid())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
