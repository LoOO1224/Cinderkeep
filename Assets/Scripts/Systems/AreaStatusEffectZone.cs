using System.Collections.Generic;
using UnityEngine;

public sealed class AreaStatusEffectZone : MonoBehaviour
{
    private enum ZoneEffectType
    {
        Damage,
        Heal
    }

    [SerializeField] private ZoneEffectType _effectType = ZoneEffectType.Damage;
    [SerializeField] private float _effectPerSecond = 8f;
    [SerializeField] private float _tickInterval = 0.5f;
    [SerializeField] private LayerMask _targetLayerMask = ~0;

    private readonly Dictionary<PlayerStatus, float> _nextTickTimeByPlayer = new Dictionary<PlayerStatus, float>();

    private void OnTriggerStay(Collider otherCollider)
    {
        TryApplyZoneEffect(otherCollider);
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        PlayerStatus playerStatus = GetPlayerStatus(otherCollider);
        if (playerStatus == null)
        {
            return;
        }

        _nextTickTimeByPlayer.Remove(playerStatus);
    }

    private void TryApplyZoneEffect(Collider otherCollider)
    {
        if (IsLayerAllowed(otherCollider) == false)
        {
            return;
        }

        PlayerStatus playerStatus = GetPlayerStatus(otherCollider);
        if (playerStatus == null)
        {
            return;
        }

        if (CanApplyTick(playerStatus) == false)
        {
            return;
        }

        ApplyEffect(playerStatus);
        _nextTickTimeByPlayer[playerStatus] = Time.time + Mathf.Max(0.05f, _tickInterval);
    }

    private bool IsLayerAllowed(Collider otherCollider)
    {
        if (otherCollider == null)
        {
            return false;
        }

        int layerMask = 1 << otherCollider.gameObject.layer;
        return (_targetLayerMask.value & layerMask) != 0;
    }

    private bool CanApplyTick(PlayerStatus playerStatus)
    {
        if (_nextTickTimeByPlayer.ContainsKey(playerStatus) == false)
        {
            return true;
        }

        return Time.time >= _nextTickTimeByPlayer[playerStatus];
    }

    private void ApplyEffect(PlayerStatus playerStatus)
    {
        float amount = Mathf.Max(0f, _effectPerSecond) * Mathf.Max(0.05f, _tickInterval);
        if (amount <= 0f)
        {
            return;
        }

        if (_effectType == ZoneEffectType.Heal)
        {
            playerStatus.Heal(amount);
        }
        else
        {
            playerStatus.TakeDamage(amount);
        }

        EmeraldQuestEventHub.RaiseZoneEffectApplied();
        Debug.Log("[AreaStatusEffectZone] " + _effectType + " applied: " + amount);
    }

    private PlayerStatus GetPlayerStatus(Collider otherCollider)
    {
        if (otherCollider == null)
        {
            return null;
        }

        PlayerStatus playerStatus = otherCollider.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            return playerStatus;
        }

        return otherCollider.GetComponentInParent<PlayerStatus>();
    }
}
