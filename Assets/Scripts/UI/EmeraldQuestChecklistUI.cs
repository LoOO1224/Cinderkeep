using TMPro;
using UnityEngine;

public sealed class EmeraldQuestChecklistUI : MonoBehaviour
{
    private const int ObjectiveCount = 9;
    private const int AttackInputIndex = 0;
    private const int EnemyDamageIndex = 1;
    private const int CooldownIndex = 2;
    private const int HealingPickupIndex = 3;
    private const int ZoneEffectIndex = 4;
    private const int PlayerDamageIndex = 5;
    private const int EnemyDeathIndex = 6;
    private const int HealthVisibleIndex = 7;
    private const int FeedbackIndex = 8;

    private static readonly string[] ObjectiveNames =
    {
        "공격 입력 감지",
        "적 HP 피해 적용",
        "공격 쿨타임으로 연타 제한",
        "회복 오브젝트로 플레이어 HP 증가",
        "영역 진입 시 지속 효과 적용",
        "플레이어 피격으로 HP 감소",
        "적 HP 0 이하 사망 처리",
        "HP 변화 UI/로그 확인",
        "전투 피드백 1종 이상 제공"
    };

    [SerializeField] private TMP_Text[] _objectiveTexts;
    [SerializeField] private Color _completeColor = new Color(0.4f, 1f, 0.65f, 1f);
    [SerializeField] private Color _incompleteColor = new Color(0.78f, 0.82f, 0.88f, 1f);

    private readonly bool[] _isObjectiveComplete = new bool[ObjectiveCount];

    private void OnEnable()
    {
        BindEvents();
        RefreshAllRows();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    public void ResetChecklist()
    {
        for (int i = 0; i < _isObjectiveComplete.Length; i++)
        {
            _isObjectiveComplete[i] = false;
        }

        RefreshAllRows();
    }

    private void BindEvents()
    {
        EmeraldQuestEventHub.AttackInputRead += CompleteAttackInput;
        EmeraldQuestEventHub.AttackDamageApplied += CompleteEnemyDamage;
        EmeraldQuestEventHub.AttackBlockedByCooldown += CompleteCooldown;
        EmeraldQuestEventHub.HealingPickupCollected += CompleteHealingPickup;
        EmeraldQuestEventHub.ZoneEffectApplied += CompleteZoneEffect;
        EmeraldQuestEventHub.PlayerDamaged += CompletePlayerDamage;
        EmeraldQuestEventHub.EnemyDefeated += CompleteEnemyDeath;
        EmeraldQuestEventHub.EnemyHealthChanged += CompleteHealthVisible;
        EmeraldQuestEventHub.PlayerDamaged += CompleteHealthVisible;
        EmeraldQuestEventHub.PlayerHealed += CompleteHealthVisible;
        EmeraldQuestEventHub.CombatFeedbackProvided += CompleteFeedback;
    }

    private void UnbindEvents()
    {
        EmeraldQuestEventHub.AttackInputRead -= CompleteAttackInput;
        EmeraldQuestEventHub.AttackDamageApplied -= CompleteEnemyDamage;
        EmeraldQuestEventHub.AttackBlockedByCooldown -= CompleteCooldown;
        EmeraldQuestEventHub.HealingPickupCollected -= CompleteHealingPickup;
        EmeraldQuestEventHub.ZoneEffectApplied -= CompleteZoneEffect;
        EmeraldQuestEventHub.PlayerDamaged -= CompletePlayerDamage;
        EmeraldQuestEventHub.EnemyDefeated -= CompleteEnemyDeath;
        EmeraldQuestEventHub.EnemyHealthChanged -= CompleteHealthVisible;
        EmeraldQuestEventHub.PlayerDamaged -= CompleteHealthVisible;
        EmeraldQuestEventHub.PlayerHealed -= CompleteHealthVisible;
        EmeraldQuestEventHub.CombatFeedbackProvided -= CompleteFeedback;
    }

    private void CompleteAttackInput()
    {
        CompleteObjective(AttackInputIndex);
    }

    private void CompleteEnemyDamage()
    {
        CompleteObjective(EnemyDamageIndex);
    }

    private void CompleteCooldown()
    {
        CompleteObjective(CooldownIndex);
    }

    private void CompleteHealingPickup()
    {
        CompleteObjective(HealingPickupIndex);
    }

    private void CompleteZoneEffect()
    {
        CompleteObjective(ZoneEffectIndex);
    }

    private void CompletePlayerDamage()
    {
        CompleteObjective(PlayerDamageIndex);
    }

    private void CompleteEnemyDeath()
    {
        CompleteObjective(EnemyDeathIndex);
    }

    private void CompleteHealthVisible()
    {
        CompleteObjective(HealthVisibleIndex);
    }

    private void CompleteFeedback()
    {
        CompleteObjective(FeedbackIndex);
    }

    private void CompleteObjective(int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= _isObjectiveComplete.Length)
        {
            return;
        }

        if (_isObjectiveComplete[objectiveIndex] == true)
        {
            return;
        }

        _isObjectiveComplete[objectiveIndex] = true;
        RefreshRow(objectiveIndex);
    }

    private void RefreshAllRows()
    {
        for (int i = 0; i < ObjectiveCount; i++)
        {
            RefreshRow(i);
        }
    }

    private void RefreshRow(int objectiveIndex)
    {
        TMP_Text objectiveText = GetObjectiveText(objectiveIndex);
        if (objectiveText == null)
        {
            return;
        }

        bool isComplete = _isObjectiveComplete[objectiveIndex];
        objectiveText.text = GetPrefix(isComplete) + ObjectiveNames[objectiveIndex];
        objectiveText.color = isComplete ? _completeColor : _incompleteColor;
    }

    private TMP_Text GetObjectiveText(int objectiveIndex)
    {
        if (_objectiveTexts == null)
        {
            return null;
        }

        if (objectiveIndex < 0 || objectiveIndex >= _objectiveTexts.Length)
        {
            return null;
        }

        return _objectiveTexts[objectiveIndex];
    }

    private string GetPrefix(bool isComplete)
    {
        return isComplete ? "[OK] " : "[ ] ";
    }
}
