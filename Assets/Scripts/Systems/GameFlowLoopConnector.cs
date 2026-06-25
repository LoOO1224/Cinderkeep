using UnityEngine;
using Cinderkeep.Gameplay;

// 닫힌 플레이 루프 안에서 하나의 구체적인 게임플레이 기능을 실행합니다.
// 클래스 책임은 좁게 유지하고, 다른 시스템과는 명확한 메서드나 이벤트로만 연결합니다.
// GameFlowController와 스폰 Director를 연결하고 게임 시작 흐름을 호출하는 컴포넌트입니다.
// 시간 진행과 페이즈 전환은 GameFlowController가 담당하고 여기서는 시작 연결만 담당합니다.
public sealed class GameFlowLoopConnector : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObjectManager _gameObjectManager;

    [Header("Enemies")]
    [SerializeField] private EnemyLoopConnector _enemyLoopConnector;

    public void Initialize(
        GameManager gameManager,
        GameObjectManager gameObjectManager,
        EnemyLoopConnector enemyLoopConnector)
    {
        _gameManager = gameManager;
        _gameObjectManager = gameObjectManager;
        _enemyLoopConnector = enemyLoopConnector;
    }

    public void ConnectGameFlow()
    {
        if (_gameManager == null)
        {
            return;
        }

        GameFlowController gameFlowController = _gameManager.GetGameFlowController();
        if (gameFlowController != null)
        {
            gameFlowController.InitializeEnemySpawnPoints(_gameObjectManager, _enemyLoopConnector);
        }

        _gameManager.StartNewGame();
    }
}
