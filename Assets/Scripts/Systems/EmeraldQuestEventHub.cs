using System;

public static class EmeraldQuestEventHub
{
    public static event Action AttackInputRead;
    public static event Action AttackDamageApplied;
    public static event Action AttackBlockedByCooldown;
    public static event Action CombatFeedbackProvided;
    public static event Action EnemyHealthChanged;
    public static event Action EnemyDefeated;
    public static event Action PlayerDamaged;
    public static event Action PlayerHealed;
    public static event Action HealingPickupCollected;
    public static event Action PlayerDefeated;
    public static event Action ZoneEffectApplied;

    public static void RaiseAttackInputRead()
    {
        AttackInputRead?.Invoke();
    }

    public static void RaiseAttackDamageApplied()
    {
        AttackDamageApplied?.Invoke();
    }

    public static void RaiseAttackBlockedByCooldown()
    {
        AttackBlockedByCooldown?.Invoke();
    }

    public static void RaiseCombatFeedbackProvided()
    {
        CombatFeedbackProvided?.Invoke();
    }

    public static void RaiseEnemyHealthChanged()
    {
        EnemyHealthChanged?.Invoke();
    }

    public static void RaiseEnemyDefeated()
    {
        EnemyDefeated?.Invoke();
    }

    public static void RaisePlayerDamaged()
    {
        PlayerDamaged?.Invoke();
    }

    public static void RaisePlayerHealed()
    {
        PlayerHealed?.Invoke();
    }

    public static void RaiseHealingPickupCollected()
    {
        HealingPickupCollected?.Invoke();
    }

    public static void RaisePlayerDefeated()
    {
        PlayerDefeated?.Invoke();
    }

    public static void RaiseZoneEffectApplied()
    {
        ZoneEffectApplied?.Invoke();
    }
}
