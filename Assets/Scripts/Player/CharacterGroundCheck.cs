using UnityEngine;

public sealed class CharacterGroundCheck : MonoBehaviour
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
