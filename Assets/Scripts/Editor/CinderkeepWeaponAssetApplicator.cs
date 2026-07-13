using UnityEditor;
using UnityEngine;

// 외부 저폴리 무기 메시와 팔레트 텍스처를 Cinderkeep용 URP 프리팹으로 변환합니다.
// 전투 수치와 피격 판정은 건드리지 않고 장착 화면에 사용할 시각 에셋만 만듭니다.
public static class CinderkeepWeaponAssetApplicator
{
    private const string AssetRoot =
        "Assets/ThirdParty/Free/CinderkeepExternalAssets/FantasyKingdomWeapon/";
    private const string ModelPath =
        AssetRoot + "Models/SM_Frostbound_StoneSword.fbx";
    private const string AlbedoPath =
        AssetRoot + "Textures/FantasyKingdom_Weapons_Albedo.png";
    private const string NormalPath =
        AssetRoot + "Textures/FantasyKingdom_Weapons_Normal.png";
    private const string MaterialPath =
        AssetRoot + "Materials/MAT_Frostbound_StoneSword.mat";
    private const string PrefabPath =
        AssetRoot + "Prefabs/PF_Frostbound_StoneSword.prefab";

    [MenuItem("Cinderkeep/Assets/Equipment/Build Frostbound Stone Sword")]
    public static void BuildStoneSwordAsset()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureModelImporter();
        ConfigureNormalTexture();

        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        Texture2D albedoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath);
        Texture2D normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (model == null || albedoTexture == null || normalTexture == null || shader == null)
        {
            Debug.LogError("[CinderkeepWeaponAssetApplicator] 검 프리팹 생성에 필요한 에셋을 찾지 못했습니다.");
            return;
        }

        Material material = GetOrCreateMaterial(shader);
        material.SetTexture("_BaseMap", albedoTexture);
        material.SetTexture("_BumpMap", normalTexture);
        material.SetFloat("_BumpScale", 0.7f);
        material.SetFloat("_Metallic", 0.08f);
        material.SetFloat("_Smoothness", 0.24f);
        material.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(material);

        GameObject prefabRoot = new GameObject("PF_Frostbound_StoneSword");
        GameObject visual = Object.Instantiate(model, prefabRoot.transform);
        visual.name = "Visual_Frostbound_StoneSword";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;
        ApplyMaterial(visual, material);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        Object.DestroyImmediate(prefabRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CinderkeepWeaponAssetApplicator] 서리결 돌검 프리팹을 생성했습니다.");
    }

    private static void ConfigureModelImporter()
    {
        ModelImporter modelImporter = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
        if (modelImporter == null)
        {
            return;
        }

        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
        modelImporter.importAnimation = false;
        modelImporter.SaveAndReimport();
    }

    private static void ConfigureNormalTexture()
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(NormalPath) as TextureImporter;
        if (textureImporter == null || textureImporter.textureType == TextureImporterType.NormalMap)
        {
            return;
        }

        textureImporter.textureType = TextureImporterType.NormalMap;
        textureImporter.SaveAndReimport();
    }

    private static Material GetOrCreateMaterial(Shader shader)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material != null)
        {
            material.shader = shader;
            return material;
        }

        material = new Material(shader);
        material.name = "MAT_Frostbound_StoneSword";
        AssetDatabase.CreateAsset(material, MaterialPath);
        return material;
    }

    private static void ApplyMaterial(GameObject rootObject, Material material)
    {
        Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            Material[] materials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }
}
