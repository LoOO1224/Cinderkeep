using UnityEngine;

// 자원 오브젝트가 공통으로 가질 수 있는 기본 데이터 컴포넌트입니다.
// 4.00에서는 ResourceNode가 실제 지급을 맡고, 이 컴포넌트는 데이터 확인용으로 남깁니다.
public sealed class ResourceBase : MonoBehaviour
{
    [SerializeField] private string _resourceId = "Wood";
    [SerializeField] private int _amount = 1;

    public string ResourceId
    {
        get
        {
            return _resourceId;
        }
    }

    public int Amount
    {
        get
        {
            return _amount;
        }
    }

    public void Initialize(string resourceId, int amount)
    {
        _resourceId = resourceId;
        _amount = Mathf.Max(0, amount);
    }
}
