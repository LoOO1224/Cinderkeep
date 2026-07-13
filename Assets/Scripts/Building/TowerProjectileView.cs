using UnityEngine;

// 타워 공격 지점에서 적까지 화살 시각물을 이동시킵니다.
// 정식 화살 프리팹을 우선 사용하고, 로드할 수 없을 때만 구체 폴백을 생성합니다.
public sealed class TowerProjectileView : MonoBehaviour
{
    private const string ProjectilePrefabResourcePath = "Cinderkeep/prefabs/vfx/PF_VFX_TowerArrow";
    private const float DefaultDuration = 0.18f;
    private const float ProjectileScale = 0.18f;

    private static GameObject _projectilePrefab;
    private static bool _didTryLoadProjectilePrefab;
    private static Material _projectileMaterial;
    private static Material _trailMaterial;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _duration = DefaultDuration;
    private float _elapsedTime;

    public static void Play(Vector3 startPosition, Vector3 targetPosition)
    {
        GameObject projectileObject = CreateProjectileObject();
        projectileObject.transform.position = startPosition;
        SetProjectileRotation(projectileObject.transform, startPosition, targetPosition);
        EnsureTrailRenderer(projectileObject);

        TowerProjectileView projectileView = projectileObject.GetComponent<TowerProjectileView>();
        if (projectileView == null)
        {
            projectileView = projectileObject.AddComponent<TowerProjectileView>();
        }

        projectileView.Initialize(startPosition, targetPosition, DefaultDuration);
    }

    private static GameObject CreateProjectileObject()
    {
        GameObject projectilePrefab = GetProjectilePrefabCanBeNull();
        if (projectilePrefab != null)
        {
            GameObject projectileObject = Instantiate(projectilePrefab);
            projectileObject.name = "VFX_TowerProjectile_Arrow";
            return projectileObject;
        }

        return CreateFallbackProjectileObject();
    }

    private static GameObject GetProjectilePrefabCanBeNull()
    {
        if (_didTryLoadProjectilePrefab)
        {
            return _projectilePrefab;
        }

        _didTryLoadProjectilePrefab = true;
        _projectilePrefab = Resources.Load<GameObject>(ProjectilePrefabResourcePath);
        return _projectilePrefab;
    }

    private static GameObject CreateFallbackProjectileObject()
    {
        GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectileObject.name = "VFX_TowerProjectile_Fallback";
        projectileObject.transform.localScale = Vector3.one * ProjectileScale;

        Collider projectileCollider = projectileObject.GetComponent<Collider>();
        if (projectileCollider != null)
        {
            Destroy(projectileCollider);
        }

        MeshRenderer renderer = projectileObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = GetProjectileMaterial();
        }

        return projectileObject;
    }

    private static void EnsureTrailRenderer(GameObject projectileObject)
    {
        TrailRenderer trailRenderer = projectileObject.GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = projectileObject.AddComponent<TrailRenderer>();
        }

        ConfigureTrail(trailRenderer);
    }

    private static void ConfigureTrail(TrailRenderer trailRenderer)
    {
        trailRenderer.time = 0.16f;
        trailRenderer.startWidth = 0.12f;
        trailRenderer.endWidth = 0.02f;
        trailRenderer.sharedMaterial = GetTrailMaterial();
        trailRenderer.startColor = new Color(1f, 0.86f, 0.2f, 0.95f);
        trailRenderer.endColor = new Color(1f, 0.35f, 0.05f, 0f);
    }

    private static void SetProjectileRotation(
        Transform projectileTransform,
        Vector3 startPosition,
        Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - startPosition;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        projectileTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void Initialize(Vector3 startPosition, Vector3 targetPosition, float duration)
    {
        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _duration = Mathf.Max(0.05f, duration);
        _elapsedTime = 0f;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsedTime / _duration);
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, progress);

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private static Material GetProjectileMaterial()
    {
        if (_projectileMaterial != null)
        {
            return _projectileMaterial;
        }

        _projectileMaterial = CreateMaterial(new Color(1f, 0.78f, 0.05f, 1f));
        return _projectileMaterial;
    }

    private static Material GetTrailMaterial()
    {
        if (_trailMaterial != null)
        {
            return _trailMaterial;
        }

        _trailMaterial = CreateMaterial(new Color(1f, 0.45f, 0.05f, 0.85f));
        ConfigureTransparentMaterial(_trailMaterial);
        return _trailMaterial;
    }

    private static void ConfigureTransparentMaterial(Material material)
    {
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = 3000;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
