using UnityEngine;

// 닫힌 플레이 루프 안에서 하나의 구체적인 게임플레이 기능을 실행합니다.
// 클래스 책임은 좁게 유지하고, 다른 시스템과는 명확한 메서드나 이벤트로만 연결합니다.
public sealed class CinderHeartPlayerRecoveryAura : MonoBehaviour
{
    [SerializeField] private float _healRadius = 7f;
    [SerializeField] private float _healPerSecond = 1.5f;

    private CinderHeart _cinderHeart;
    private PlayerStatus _playerStatus;

    public static void EnsureSceneAura()
    {
        CinderHeart cinderHeart = Object.FindFirstObjectByType<CinderHeart>();
        if (cinderHeart == null)
        {
            return;
        }

        CinderHeartPlayerRecoveryAura aura = cinderHeart.GetComponent<CinderHeartPlayerRecoveryAura>();
        if (aura == null)
        {
            aura = cinderHeart.gameObject.AddComponent<CinderHeartPlayerRecoveryAura>();
        }

        aura.ConnectTargets();
    }

    private void Awake()
    {
        ConnectTargets();
    }

    private void Update()
    {
        ConnectTargets();
        TryHealPlayer();
    }

    private void ConnectTargets()
    {
        if (_cinderHeart == null)
        {
            _cinderHeart = GetComponent<CinderHeart>();
        }

        if (_playerStatus == null)
        {
            _playerStatus = Object.FindFirstObjectByType<PlayerStatus>();
        }
    }

    private void TryHealPlayer()
    {
        if (_cinderHeart == null || _playerStatus == null)
        {
            return;
        }

        if (_cinderHeart.IsDestroyed || _playerStatus.IsDead())
        {
            return;
        }

        float distanceSqr = (_playerStatus.transform.position - _cinderHeart.transform.position).sqrMagnitude;
        if (distanceSqr > _healRadius * _healRadius)
        {
            return;
        }

        _playerStatus.Heal(_healPerSecond * Time.deltaTime);
    }
}
