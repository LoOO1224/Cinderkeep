using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;

// 건축 지점 위에 어떤 건물을 지을 수 있는지 표시하는 월드 라벨입니다.
// BuildingSpot 상태만 읽고, 실제 건축 가능 여부와 비용 처리는 BuildingManager가 담당합니다.
public sealed class BuildingSpotLabel : MonoBehaviour
{
    private const string LabelObjectName = "WorldLabel_BuildingSpot";

    [SerializeField] private BuildingSpot _buildingSpot;
    [SerializeField] private TextMeshPro _labelText;
    [SerializeField] private Vector3 _localOffset = new Vector3(0f, 1.35f, 0f);

    private Camera _mainCamera;

    public static BuildingSpotLabel EnsureForSpot(BuildingSpot buildingSpot)
    {
        if (buildingSpot == null)
        {
            return null;
        }

        BuildingSpotLabel existing = buildingSpot.GetComponentInChildren<BuildingSpotLabel>(true);
        if (existing != null)
        {
            existing._buildingSpot = buildingSpot;
            return existing;
        }

        GameObject labelObject = new GameObject(LabelObjectName);
        labelObject.transform.SetParent(buildingSpot.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one;

        TextMeshPro text = labelObject.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 2.4f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(1f, 0.95f, 0.72f, 1f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        BuildingSpotLabel label = labelObject.AddComponent<BuildingSpotLabel>();
        label._buildingSpot = buildingSpot;
        label._labelText = text;
        label.RefreshLabel();
        return label;
    }

    private void Awake()
    {
        ConnectComponents();
    }

    private void LateUpdate()
    {
        FaceCamera();
    }

    public void RefreshLabel()
    {
        ConnectComponents();
        if (_labelText == null || _buildingSpot == null)
        {
            return;
        }

        bool shouldShow = _buildingSpot.IsEmpty;
        _labelText.gameObject.SetActive(shouldShow);
        if (shouldShow == false)
        {
            return;
        }

        transform.localPosition = _localOffset;
        _labelText.text = ResolveLabelText();
    }

    private void ConnectComponents()
    {
        if (_buildingSpot == null)
        {
            _buildingSpot = GetComponentInParent<BuildingSpot>();
        }

        if (_labelText == null)
        {
            _labelText = GetComponent<TextMeshPro>();
        }
    }

    private string ResolveLabelText()
    {
        BuildingData buildingData = GetBuildingData();
        if (buildingData == null)
        {
            return FormatLabel(_buildingSpot.BuildingDataId, "건축 지점");
        }

        string koreanName = ResolveKoreanName(buildingData);
        return FormatLabel(koreanName, "E 길게");
    }

    private BuildingData GetBuildingData()
    {
        if (_buildingSpot == null || GameManager.Inst == null || GameManager.Inst.GetGameDataManager() == null)
        {
            return null;
        }

        return GameManager.Inst.GetGameDataManager().GetBuilding(_buildingSpot.BuildingDataId);
    }

    private string ResolveKoreanName(BuildingData buildingData)
    {
        if (buildingData == null)
        {
            return "건축물";
        }

        if (string.Equals(buildingData.BuildingType, "Wall", System.StringComparison.OrdinalIgnoreCase))
        {
            return "벽";
        }

        if (string.Equals(buildingData.BuildingType, "Tower", System.StringComparison.OrdinalIgnoreCase))
        {
            return "타워";
        }

        if (string.Equals(buildingData.BuildingType, "Trap", System.StringComparison.OrdinalIgnoreCase))
        {
            return "함정";
        }

        string id = buildingData.Id;
        if (string.Equals(id, "workbench", System.StringComparison.OrdinalIgnoreCase))
        {
            return "제작대";
        }

        if (string.Equals(id, "furnace", System.StringComparison.OrdinalIgnoreCase))
        {
            return "용광로";
        }

        if (string.Equals(id, "anvil", System.StringComparison.OrdinalIgnoreCase))
        {
            return "모루";
        }

        return string.IsNullOrEmpty(buildingData.DisplayName) ? "건축물" : buildingData.DisplayName;
    }

    private string FormatLabel(string title, string hint)
    {
        if (string.IsNullOrEmpty(title))
        {
            title = "건축물";
        }

        return title + "\n<size=70%>" + hint + "</size>";
    }

    private void FaceCamera()
    {
        if (_labelText == null || _labelText.gameObject.activeSelf == false)
        {
            return;
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera == null)
        {
            return;
        }

        Vector3 direction = transform.position - _mainCamera.transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
