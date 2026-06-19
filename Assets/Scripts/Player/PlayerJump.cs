using UnityEngine;
using UnityEngine.Serialization;

// 플레이어 점프 입력을 담당하는 컴포넌트입니다.
// 실제 이동 계산은 PlayerMovement가 가진 CharacterController 흐름을 사용합니다.
public sealed class PlayerJump : MonoBehaviour
{
    [FormerlySerializedAs("m_jumpForce")]
    [Tooltip("Space 입력 시 플레이어에게 적용되는 점프 힘입니다.")]
    [SerializeField] private float _jumpForce = 5f;

    [Header("Connected Components")]
    [FormerlySerializedAs("PlayerMovement_PlayerMovement")]
    [Tooltip("실제 점프 이동을 처리하는 PlayerMovement입니다. 비어 있으면 같은 오브젝트에서 찾습니다.")]
    [SerializeField] private PlayerMovement _playerMovement;

    private void Start()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (CinderkeepInput.WasKeyPressedThisFrame(KeyCode.Space))
        {
            Jump();
        }
    }

    private void ResolveReferences()
    {
        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }
    }

    private void Jump()
    {
        if (_playerMovement == null)
        {
            return;
        }

        _playerMovement.Jump(_jumpForce);
    }
}
