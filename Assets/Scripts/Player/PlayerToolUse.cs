using Cinderkeep.Gameplay;
using UnityEngine;

// 1인칭 플레이어의 입력, 상태, 장착, 채집, 전투, 건축 중 한 흐름을 담당합니다.
// 입력 제어와 실제 효과를 분리해 퀵슬롯, 도구, 무기, 튜토리얼이 서로 얽히지 않게 합니다.
// 플레이어가 현재 들고 있는 도구로 자원을 채집하는 컴포넌트입니다.
// PlayerAttack은 적 공격만 담당하고, 도끼/곡괭이 좌클릭 채집은 이 클래스가 담당합니다.
// 도구 거리, 범위, 쿨타임은 tools.json 값이 있으면 그 값을 우선 사용합니다.
public sealed class PlayerToolUse : MonoBehaviour
{
    [Header("Tool Use Settings")]
    [Tooltip("도구 채집이 닿는 최대 거리입니다. tools.json 데이터가 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private float _toolUseDistance = 2.5f;
    [Tooltip("도구 채집 판정의 두께입니다. tools.json 데이터가 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private float _toolUseRadius = 0.35f;
    [Tooltip("도구를 한 번 사용한 뒤 다시 사용할 수 있을 때까지 기다리는 시간입니다. tools.json 데이터가 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private float _toolUseInterval = 0.5f;
    [Tooltip("도구 채집 판정에 사용할 레이어입니다.")]
    [SerializeField] private LayerMask _toolUseLayerMask = ~0;

    [Header("Connected Objects")]
    [Tooltip("도구 사용 Ray가 시작되는 위치입니다. 비어 있으면 자식 카메라를 찾아 사용합니다.")]
    [SerializeField] private Transform _toolOrigin;
    [Tooltip("현재 장착한 도구 정보를 제공하는 컴포넌트입니다.")]
    [SerializeField] private PlayerToolController _playerToolController;
    [Tooltip("좌클릭 채집 시 1인칭 도구 휘두르기 연출을 담당합니다.")]
    [SerializeField] private FirstPersonToolView _firstPersonToolView;

    private float _lastToolUseTime;

    private void Start()
    {
        ConnectComponents();
    }

    private void Update()
    {
        ReadToolUseInput();
    }

    public void TryUseTool()
    {
        if (CanUseToolBase() == false)
        {
            return;
        }

        ToolData toolData = _playerToolController.GetCurrentToolData();
        ResourceNode resourceNode = GetResourceNodeFromCast(toolData);

        if (CanUseToolByInterval(resourceNode, toolData) == false)
        {
            return;
        }

        // 채집 피드백은 적중 여부와 분리해서, 빈 공간을 찍어도 광질/도끼질 연출이 보이게 합니다.
        _lastToolUseTime = Time.time;
        PlayToolUseView();

        if (resourceNode == null)
        {
            return;
        }

        resourceNode.TryGatherWithTool(gameObject, _playerToolController.CurrentToolType, toolData);
    }

    private void ConnectComponents()
    {
        if (_playerToolController == null)
        {
            _playerToolController = GetComponent<PlayerToolController>();
        }

        if (_toolOrigin == null)
        {
            Camera camera = GetComponentInChildren<Camera>();
            if (camera != null)
            {
                _toolOrigin = camera.transform;
            }
        }

        if (_firstPersonToolView == null)
        {
            _firstPersonToolView = GetComponentInChildren<FirstPersonToolView>();
        }

        if (_firstPersonToolView == null)
        {
            _firstPersonToolView = FirstPersonToolView.EnsureSceneView();
        }
    }

    private void ReadToolUseInput()
    {
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            return;
        }

        if (CinderkeepInput.WasLeftMousePressedThisFrame())
        {
            TryUseTool();
        }
    }

    private bool CanUseToolBase()
    {
        if (_playerToolController == null)
        {
            return false;
        }

        if (_playerToolController.CurrentToolType == GatherToolType.None)
        {
            return false;
        }

        return true;
    }

    private bool CanUseToolByInterval(ResourceNode resourceNode, ToolData toolData)
    {
        float interval = GetToolUseInterval(resourceNode, toolData);
        return Time.time >= _lastToolUseTime + interval;
    }

    private ResourceNode GetResourceNodeFromCast(ToolData toolData)
    {
        if (_toolOrigin == null)
        {
            return null;
        }

        Ray toolRay = new Ray(_toolOrigin.position, _toolOrigin.forward);
        float useDistance = GetToolUseDistance(toolData);
        float useRadius = GetToolUseRadius(toolData);
        Physics.SyncTransforms();
        RaycastHit[] hitInfos = Physics.SphereCastAll(toolRay, useRadius, useDistance, _toolUseLayerMask);

        if (hitInfos == null || hitInfos.Length <= 0)
        {
            return null;
        }

        ResourceNode closestResourceNode = null;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < hitInfos.Length; i++)
        {
            Collider hitCollider = hitInfos[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            ResourceNode resourceNode = hitCollider.GetComponentInParent<ResourceNode>();
            if (resourceNode == null)
            {
                continue;
            }

            if (hitInfos[i].distance >= closestDistance)
            {
                continue;
            }

            closestResourceNode = resourceNode;
            closestDistance = hitInfos[i].distance;
        }

        return closestResourceNode;
    }

    private float GetToolUseDistance(ToolData toolData)
    {
        if (toolData != null && toolData.AttackDistance > 0f)
        {
            return toolData.AttackDistance;
        }

        return _toolUseDistance;
    }

    private float GetToolUseRadius(ToolData toolData)
    {
        if (toolData != null && toolData.AttackRadius > 0f)
        {
            return toolData.AttackRadius;
        }

        return _toolUseRadius;
    }

    private float GetToolUseInterval(ResourceNode resourceNode, ToolData toolData)
    {
        // tools.json의 AttackInterval을 기본 사용 시간으로 보고, ResourceNode의 자원별 배율로 속도를 조절합니다.
        // 도구 티어별 채집 체감은 tools.json의 Wood/Stone/Iron/Gold/Adamantium 배율을 수정해 조절합니다.
        float interval = _toolUseInterval;
        if (toolData != null && toolData.AttackInterval > 0f)
        {
            interval = toolData.AttackInterval;
        }

        if (resourceNode == null || toolData == null)
        {
            return interval;
        }

        float multiplier = resourceNode.GetToolGatherMultiplier(toolData);
        if (multiplier > 0f)
        {
            return interval / multiplier;
        }

        return interval;
    }

    private void PlayToolUseView()
    {
        if (_firstPersonToolView == null)
        {
            return;
        }

        _firstPersonToolView.PlaySwing();
    }
}
