using UnityEngine;

// 닫힌 플레이 루프 안에서 하나의 구체적인 게임플레이 기능을 실행합니다.
// 클래스 책임은 좁게 유지하고, 다른 시스템과는 명확한 메서드나 이벤트로만 연결합니다.
// 플레이어 상태와 플레이어 HUD를 연결하는 컴포넌트입니다.
// 이동, 공격, 체력 계산은 각 전용 컴포넌트가 담당하고 여기서는 HUD 연결만 담당합니다.
public sealed class PlayerLoopConnector : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStatus _playerStatus;

    [Header("UI")]
    [SerializeField] private PlayerHUD _playerHud;

    public void Initialize(PlayerStatus playerStatus, PlayerHUD playerHud)
    {
        _playerStatus = playerStatus;
        _playerHud = playerHud;
    }

    public void ConnectPlayerHud()
    {
        if (_playerHud == null)
        {
            return;
        }

        _playerHud.SetPlayerStatus(_playerStatus);
    }
}
