using UnityEngine;

// 플레이어가 현재 들고 있는 도구를 관리하는 컴포넌트입니다.
// 현재 단계에서는 1번 도끼, 2번 곡괭이, 3번 맨손으로만 단순하게 연결합니다.
public sealed class PlayerToolController : MonoBehaviour
{
    [Header("Tool Keys")]
    [Tooltip("도끼를 장착하는 입력 키입니다.")]
    [SerializeField] private KeyCode _axeKey = KeyCode.Alpha1;
    [Tooltip("곡괭이를 장착하는 입력 키입니다.")]
    [SerializeField] private KeyCode _pickaxeKey = KeyCode.Alpha2;
    [Tooltip("맨손으로 전환하는 입력 키입니다.")]
    [SerializeField] private KeyCode _emptyHandKey = KeyCode.Alpha3;

    [Header("Current Tool")]
    [Tooltip("현재 플레이어가 장착한 도구입니다.")]
    [SerializeField] private GatherToolType _currentToolType = GatherToolType.Axe;

    public GatherToolType CurrentToolType
    {
        get
        {
            return _currentToolType;
        }
    }

    private void Update()
    {
        ReadToolInput();
    }

    public void EquipTool(GatherToolType toolType)
    {
        _currentToolType = toolType;
    }

    public bool HasRequiredTool(GatherToolType requiredToolType)
    {
        if (requiredToolType == GatherToolType.None)
        {
            return true;
        }

        return _currentToolType == requiredToolType;
    }

    private void ReadToolInput()
    {
        if (CinderkeepInput.WasKeyPressedThisFrame(_axeKey))
        {
            EquipTool(GatherToolType.Axe);
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(_pickaxeKey))
        {
            EquipTool(GatherToolType.Pickaxe);
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(_emptyHandKey))
        {
            EquipTool(GatherToolType.None);
        }
    }
}
