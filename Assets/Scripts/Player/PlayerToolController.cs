using UnityEngine;

// 플레이어가 현재 들고 있는 도구를 관리하는 컴포넌트입니다.
// 4.00 현재 단계에서는 1번 도끼, 2번 곡괭이, 3번 맨손으로만 단순하게 연결합니다.
public sealed class PlayerToolController : MonoBehaviour
{
    [Header("Tool Keys")]
    [SerializeField] private KeyCode _axeKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode _pickaxeKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode _emptyHandKey = KeyCode.Alpha3;

    [Header("Current Tool")]
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
        if (Input.GetKeyDown(_axeKey))
        {
            EquipTool(GatherToolType.Axe);
            return;
        }

        if (Input.GetKeyDown(_pickaxeKey))
        {
            EquipTool(GatherToolType.Pickaxe);
            return;
        }

        if (Input.GetKeyDown(_emptyHandKey))
        {
            EquipTool(GatherToolType.None);
        }
    }
}
