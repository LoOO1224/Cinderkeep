using UnityEngine;

// 메인 게임 루프 씬에서 전용 Connector들의 실행 순서만 관리하는 컴포넌트입니다.
// 실제 연결 작업은 Player, Resource, Enemy, GameFlow 전용 Connector가 각각 담당합니다.
public sealed class CinderkeepGameLoopConnector : MonoBehaviour
{
    [Header("Loop Connectors")]
    [SerializeField] private PlayerLoopConnector _playerLoopConnector;
    [SerializeField] private ResourceLoopConnector _resourceLoopConnector;
    [SerializeField] private EnemyLoopConnector _enemyLoopConnector;
    [SerializeField] private GameFlowLoopConnector _gameFlowLoopConnector;

    private void Start()
    {
        ConnectGameLoop();
    }

    public void ConnectGameLoop()
    {
        ConnectPlayerLoop();
        ConnectResourceLoop();
        ConnectEnemyLoop();
        ConnectGameFlowLoop();
    }

    private void ConnectPlayerLoop()
    {
        if (_playerLoopConnector == null)
        {
            return;
        }

        _playerLoopConnector.ConnectPlayerHud();
    }

    private void ConnectResourceLoop()
    {
        if (_resourceLoopConnector == null)
        {
            return;
        }

        _resourceLoopConnector.ConnectResourceHud();
    }

    private void ConnectEnemyLoop()
    {
        if (_enemyLoopConnector == null)
        {
            return;
        }

        _enemyLoopConnector.InitializeEnemies();
    }

    private void ConnectGameFlowLoop()
    {
        if (_gameFlowLoopConnector == null)
        {
            return;
        }

        _gameFlowLoopConnector.ConnectGameFlow();
    }
}
