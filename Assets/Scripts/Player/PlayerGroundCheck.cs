using UnityEngine;

// 1인칭 플레이어의 입력, 상태, 장착, 채집, 전투, 건축 중 한 흐름을 담당합니다.
// 입력 제어와 실제 효과를 분리해 퀵슬롯, 도구, 무기, 튜토리얼이 서로 얽히지 않게 합니다.
public sealed class PlayerGroundCheck : MonoBehaviour
{
    private Collider _playerCollider;
    private Collider _rootCollider;
    private int _groundContactCount;

    private void Start()
    {
        _playerCollider = GetComponent<Collider>();
        _rootCollider = GetComponentInParent<Collider>();
    }

    public bool GetIsGrounded()
    {
        return _groundContactCount > 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsIgnoredCollider(other))
        {
            return;
        }

        _groundContactCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsIgnoredCollider(other))
        {
            return;
        }

        _groundContactCount = Mathf.Max(0, _groundContactCount - 1);
    }

    private bool IsIgnoredCollider(Collider other)
    {
        return other == _playerCollider || other == _rootCollider;
    }
}
