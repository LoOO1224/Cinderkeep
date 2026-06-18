using UnityEngine;
using UnityEngine.Serialization;

// 플레이어 점프 입력을 담당하는 컴포넌트입니다.
// 실제 이동 계산은 PlayerMovement가 가진 CharacterController 흐름을 사용합니다.
public sealed class PlayerJump : MonoBehaviour
{
    [FormerlySerializedAs("m_jumpForce")]
    [SerializeField] private float _jumpForce = 5f;

    [Header("Connected Components")]
    [FormerlySerializedAs("PlayerMovement_PlayerMovement")]
    [SerializeField] private PlayerMovement PlayerMovement_PlayerMovement;

    private void Start()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void ResolveReferences()
    {
        if (PlayerMovement_PlayerMovement == null)
        {
            PlayerMovement_PlayerMovement = GetComponent<PlayerMovement>();
        }
    }

    private void Jump()
    {
        if (PlayerMovement_PlayerMovement == null)
        {
            return;
        }

        PlayerMovement_PlayerMovement.Jump(_jumpForce);
    }
}
