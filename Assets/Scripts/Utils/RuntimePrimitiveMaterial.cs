using UnityEngine;

// Runtime primitive fallback objects use this helper so URP projects do not show missing-shader magenta.
public static class RuntimePrimitiveMaterial
{
    public static void ApplyColor(GameObject targetObject, Color color, string materialName)
    {
        if (targetObject == null)
        {
            return;
        }

        Renderer targetRenderer = targetObject.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            return;
        }

        Material material = new Material(ResolveShader());
        material.name = string.IsNullOrEmpty(materialName) ? "MAT_Runtime_Primitive" : materialName;
        ApplyMaterialColor(material, color);
        targetRenderer.material = material;
    }

    private static Shader ResolveShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null)
        {
            return shader;
        }

        shader = Shader.Find("Standard");
        if (shader != null)
        {
            return shader;
        }

        return Shader.Find("Sprites/Default");
    }

    private static void ApplyMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }
}
