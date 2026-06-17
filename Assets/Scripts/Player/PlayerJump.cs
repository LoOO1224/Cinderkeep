using UnityEngine;
using UnityEngine.Serialization;

public sealed class PlayerJump : MonoBehaviour
{
    [FormerlySerializedAs("m_jumpForce")]
    [SerializeField] private float _jumpForce = 5f;
    [FormerlySerializedAs("m_groundCheck")]
    [SerializeField] private CharacterGroundCheck CharacterGroundCheck_GroundCheck;

    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (_rigidbody == null || CharacterGroundCheck_GroundCheck == null)
        {
            return;
        }

        if (!CharacterGroundCheck_GroundCheck.GetIsGrounded())
        {
            return;
        }

        // 점프 직전에 y 속도를 0으로 맞춰서 점프 높이가 흔들리지 않게 합니다.
        _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }
}
