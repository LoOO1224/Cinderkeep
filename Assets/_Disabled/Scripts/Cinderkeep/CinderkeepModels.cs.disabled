using System;
using UnityEngine;

namespace OODong.Cinderkeep
{
    // Instance Data: 플레이 중 변하고 저장 대상이 될 수 있는 플레이어 상태.
    // 저장/로드는 NetworkManager가 처리하고, 실제 소유는 GameManager가 한다.
    [Serializable]
    public sealed class CinderkeepPlayerModel
    {
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth = 100;

        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;

        public void SetMaxHealth(int maxHealth)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        }

        public void SetCurrentHealth(int currentHealth)
        {
            _currentHealth = Mathf.Clamp(currentHealth, 0, _maxHealth);
        }
    }

    [Serializable]
    // Instance Data: FlameHeart의 체력 상태.
    // CinderkeepFlameHeart 오브젝트는 표시/피해 전달만 하고, 이 Model은 GameManager가 소유한다.
    public sealed class CinderkeepFlameHeartModel
    {
        [SerializeField] private int _maxHealth = 180;
        [SerializeField] private int _currentHealth = 180;

        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;
        public float HealthRate => _maxHealth <= 0 ? 0f : (float)_currentHealth / _maxHealth;
        public bool IsDepleted => _currentHealth <= 0;

        public void SetMaxHealth(int maxHealth)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        }

        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
        }

        public void Repair(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }
    }

    [Serializable]
    // Instance Data: 한 번의 15분 발표 루프 진행 상태.
    // 날짜, 현재 Phase, 남은 시간, 승패 결과를 담는다.
    public sealed class CinderkeepRunModel
    {
        [SerializeField] private int _currentDay = 1;
        [SerializeField] private int _maxDay = 3;
        [SerializeField] private CinderkeepGamePhase _phase = CinderkeepGamePhase.None;
        [SerializeField] private float _phaseDuration;
        [SerializeField] private float _phaseRemaining;
        [SerializeField] private bool _isRunActive;
        [SerializeField] private bool _isBossDefeated;
        [SerializeField] private string _resultMessage = string.Empty;

        public int CurrentDay => _currentDay;
        public int MaxDay => _maxDay;
        public CinderkeepGamePhase Phase => _phase;
        public float PhaseDuration => _phaseDuration;
        public float PhaseRemaining => _phaseRemaining;
        public bool IsRunActive => _isRunActive;
        public bool IsBossDefeated => _isBossDefeated;
        public string ResultMessage => _resultMessage;

        public void StartRun(int maxDay)
        {
            _currentDay = 1;
            _maxDay = Mathf.Max(1, maxDay);
            _phase = CinderkeepGamePhase.None;
            _phaseDuration = 0f;
            _phaseRemaining = 0f;
            _isRunActive = true;
            _isBossDefeated = false;
            _resultMessage = string.Empty;
        }

        public void SetDay(int day)
        {
            _currentDay = Mathf.Clamp(day, 1, _maxDay);
        }

        public void AdvanceDay()
        {
            _currentDay = Mathf.Min(_maxDay, _currentDay + 1);
        }

        public void SetPhase(CinderkeepGamePhase phase, float duration)
        {
            _phase = phase;
            _phaseDuration = Mathf.Max(0f, duration);
            _phaseRemaining = _phaseDuration;
        }

        public void SetPhaseRemaining(float remaining)
        {
            _phaseRemaining = Mathf.Clamp(remaining, 0f, _phaseDuration);
        }

        public void MarkBossDefeated()
        {
            _isBossDefeated = true;
        }

        public void Finish(CinderkeepGamePhase resultPhase, string resultMessage)
        {
            _phase = resultPhase;
            _phaseRemaining = 0f;
            _phaseDuration = 0f;
            _isRunActive = false;
            _resultMessage = resultMessage ?? string.Empty;
        }
    }
}
