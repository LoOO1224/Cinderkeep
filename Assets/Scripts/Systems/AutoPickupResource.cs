using Cinderkeep.Gameplay;
using UnityEngine;

// 닫힌 플레이 루프 안에서 하나의 구체적인 게임플레이 기능을 실행합니다.
// 클래스 책임은 좁게 유지하고, 다른 시스템과는 명확한 메서드나 이벤트로만 연결합니다.
// 코인처럼 플레이어가 가까이 가면 자동으로 획득되는 자원 컴포넌트입니다.
// 나무/광석 채집과 섞지 않고, 자동 획득 역할만 담당합니다.
public sealed class AutoPickupResource : MonoBehaviour
{
    [Header("Resource Data")]
    [Tooltip("자동 획득 시 지급할 자원 ID입니다.")]
    [SerializeField] private string _resourceId = PlayerModel.ResourceStone;
    [Tooltip("자동 획득 시 지급할 자원 수량입니다.")]
    [SerializeField] private int _amount = 1;
    [Tooltip("획득 후 이 오브젝트를 비활성화할지 결정합니다.")]
    [SerializeField] private bool _disableAfterPickup = true;
    [Tooltip("현재 자동 획득이 가능한 상태인지 결정합니다.")]
    [SerializeField] private bool _canPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        if (CanPickup(other) == false)
        {
            return;
        }

        GiveResourceToPlayer();
        ProcessPickedUp();
    }

    private bool CanPickup(Collider other)
    {
        if (_canPickup == false)
        {
            return false;
        }

        if (other == null)
        {
            return false;
        }

        return other.CompareTag("Player");
    }

    private void GiveResourceToPlayer()
    {
        if (GameManager.Inst == null)
        {
            Debug.LogWarning("AutoPickupResource: GameManager가 없어 자원을 지급하지 못했습니다.");
            return;
        }

        GameManager.Inst.PlayerModel.AddResource(_resourceId, _amount);
        global::CinderkeepLog.Verbose("AutoPickupResource: " + _resourceId + " +" + _amount);
    }

    private void ProcessPickedUp()
    {
        _canPickup = false;

        if (_disableAfterPickup == false)
        {
            return;
        }

        gameObject.SetActive(false);
    }
}
