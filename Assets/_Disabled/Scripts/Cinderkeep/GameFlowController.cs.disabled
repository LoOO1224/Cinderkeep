using UnityEngine;

namespace OODong.Cinderkeep
{
    // 장면 순서 지휘 전용 Controller.
    // 강사님 규칙 기준: 이 클래스는 "언제 낮/밤/보스가 시작되는지"만 지휘한다.
    // 적 이동, 공격, UI 표시, 데이터 로드 같은 실제 배우 역할은 각 컴포넌트와 매니저에 위임한다.
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private GameManager GameManager_GameManager;
        [SerializeField] private CinderkeepEnemySpawner CinderkeepEnemySpawner_EnemySpawner;
        [SerializeField] private CinderkeepFlameHeart CinderkeepFlameHeart_FlameHeart;
        [SerializeField] private Light Light_Sun;
        [SerializeField] private float _dayDuration = 180f;
        [SerializeField] private float _nightDuration = 120f;
        [SerializeField] private float _morningRewardDuration = 15f;
        [SerializeField] private float _bossNightDuration = 180f;
        [SerializeField] private int _daySpawnMax = 0;
        [SerializeField] private int _nightSpawnMax = 8;
        [SerializeField] private int _bossSpawnMax = 12;

        private float _timer;
        private bool _bossSpawned;
        private CinderkeepDayRuleData _currentDayRuleData;

        private void Start()
        {
            // 현재 MVP 루프: Day 3분 -> Night 2분 -> MorningReward 15초 -> 3일차 BossNight 3분.
            // 발표 15분 루프를 목표로 하되, 수업 시연 편의를 위해 Inspector에서 시간을 줄여 테스트할 수 있다.
            ResolveReferences();
            GameManager_GameManager?.StartRun();
            StartDay();
        }

        private void Update()
        {
            if (GameManager_GameManager == null || !GameManager_GameManager.IsRunActive)
            {
                return;
            }

            _timer -= Time.deltaTime;
            GameManager_GameManager.SetPhaseRemaining(_timer);

            if (GameManager_GameManager.RunModel.Phase == CinderkeepGamePhase.BossNight && GameManager_GameManager.RunModel.IsBossDefeated)
            {
                GameManager_GameManager.WinRun("Boss defeated. Cinderkeep is safe.");
                return;
            }

            if (_timer > 0f)
            {
                return;
            }

            AdvancePhase();
        }

        public void SetReferences(GameManager gameManager, CinderkeepEnemySpawner enemySpawner, CinderkeepFlameHeart flameHeart, Light sun)
        {
            GameManager_GameManager = gameManager;
            CinderkeepEnemySpawner_EnemySpawner = enemySpawner;
            CinderkeepFlameHeart_FlameHeart = flameHeart;
            Light_Sun = sun;
        }

        private void StartDay()
        {
            // 낮: 수집/건설 시간. 일반 적 스폰은 기본적으로 끈다.
            // TODO(팀원 작업 요청): 낮 이벤트가 필요하면 "낮 전용 이벤트 컴포넌트"를 추가하고 여기서는 호출만 해 주세요.
            _currentDayRuleData = GetCurrentDayRuleData();
            _bossSpawned = false;
            SetSpawner(false, _daySpawnMax, GetSpawnInterval(), _currentDayRuleData);
            SetLighting(true);
            SetPhase(CinderkeepGamePhase.Day, GetDayDuration());
            GameManager_GameManager.ShowStatus($"Day {GameManager_GameManager.RunModel.CurrentDay}/{GameManager_GameManager.RunModel.MaxDay}: {GetDayObjective()}");
        }

        private void StartNight()
        {
            // 밤: FlameHeart 방어 시간. Spawner 설정만 바꾸고, 실제 생성은 CinderkeepEnemySpawner가 담당한다.
            _currentDayRuleData = GetCurrentDayRuleData();
            SetSpawner(true, GetNightSpawnMax(), GetSpawnInterval(), _currentDayRuleData);
            SetLighting(false);
            SetPhase(CinderkeepGamePhase.Night, GetNightDuration());
            GameManager_GameManager.ShowStatus($"Night {GameManager_GameManager.RunModel.CurrentDay}/{GameManager_GameManager.RunModel.MaxDay}: {GetNightObjective()}");
        }

        private void StartMorningReward()
        {
            // 아침 보상: GameManager가 Model/Inventory를 변경하고, Controller는 다음 단계로 넘기는 역할만 한다.
            SetSpawner(false, 0, GetSpawnInterval(), _currentDayRuleData);
            SetLighting(true);
            GameManager_GameManager.GrantMorningReward();
            SetPhase(CinderkeepGamePhase.MorningReward, GetMorningRewardDuration());
        }

        private void StartBossNight()
        {
            // 3일차 최종 밤. 보스 생성 요청은 Spawner에게 위임한다.
            // TODO(팀원 작업 요청): 보스 등장 연출/카메라 흔들림/사운드는 별도 BossIntroView 컴포넌트로 만들어 연결해 주세요.
            _currentDayRuleData = GetCurrentDayRuleData();
            SetSpawner(true, GetBossSpawnMax(), Mathf.Max(1.2f, GetSpawnInterval() * 0.75f), _currentDayRuleData);
            SetLighting(false);
            SetPhase(CinderkeepGamePhase.BossNight, GetBossNightDuration());
            CinderkeepEnemySpawner_EnemySpawner?.SpawnBoss();
            _bossSpawned = true;
            GameManager_GameManager.ShowStatus($"Final night Day {GameManager_GameManager.RunModel.CurrentDay}/{GameManager_GameManager.RunModel.MaxDay}: defeat the Cinder Warden.");
        }

        private void AdvancePhase()
        {
            CinderkeepRunModel runModel = GameManager_GameManager.RunModel;

            switch (runModel.Phase)
            {
                case CinderkeepGamePhase.Day:
                    if (ShouldStartBossNight(runModel))
                    {
                        StartBossNight();
                    }
                    else
                    {
                        StartNight();
                    }

                    break;
                case CinderkeepGamePhase.Night:
                    StartMorningReward();
                    break;
                case CinderkeepGamePhase.MorningReward:
                    GameManager_GameManager.AdvanceDay();
                    StartDay();
                    break;
                case CinderkeepGamePhase.BossNight:
                    if (!_bossSpawned || CinderkeepEnemySpawner_EnemySpawner == null || !CinderkeepEnemySpawner_EnemySpawner.HasLivingBoss)
                    {
                        GameManager_GameManager.WinRun("Cinderkeep survived the final night.");
                    }
                    else
                    {
                        GameManager_GameManager.LoseRun("The Cinder Warden overran the FlameHeart.");
                    }

                    break;
            }
        }

        private void SetPhase(CinderkeepGamePhase phase, float duration)
        {
            _timer = Mathf.Max(0f, duration);
            GameManager_GameManager.SetPhase(phase, _timer);
        }

        private void SetSpawner(bool isEnabled, int maxAliveCount, float spawnInterval, CinderkeepDayRuleData dayRuleData)
        {
            if (CinderkeepEnemySpawner_EnemySpawner == null)
            {
                return;
            }

            CinderkeepEnemySpawner_EnemySpawner.SetMaxAliveCount(maxAliveCount);
            CinderkeepEnemySpawner_EnemySpawner.SetSpawnInterval(spawnInterval);
            if (dayRuleData != null)
            {
                CinderkeepEnemySpawner_EnemySpawner.SetSpawnRadius(dayRuleData.SpawnRadius);
                CinderkeepEnemySpawner_EnemySpawner.SetEnemyDataId(dayRuleData.EnemyDataId);
                CinderkeepEnemySpawner_EnemySpawner.SetBossDataId(dayRuleData.BossDataId);
            }

            CinderkeepEnemySpawner_EnemySpawner.SetSpawningEnabled(isEnabled);
        }

        private void SetLighting(bool isDay)
        {
            if (Light_Sun == null)
            {
                return;
            }

            Light_Sun.intensity = isDay ? 0.78f : 0.24f;
            Light_Sun.color = isDay ? new Color(0.62f, 0.78f, 1f, 1f) : new Color(0.36f, 0.52f, 0.92f, 1f);
            RenderSettings.fogColor = isDay ? new Color(0.32f, 0.46f, 0.66f, 1f) : new Color(0.035f, 0.055f, 0.11f, 1f);
        }

        private bool ShouldStartBossNight(CinderkeepRunModel runModel)
        {
            return runModel != null && runModel.CurrentDay >= runModel.MaxDay;
        }

        private CinderkeepDayRuleData GetCurrentDayRuleData()
        {
            if (GameManager_GameManager == null)
            {
                return null;
            }

            return GameManager_GameManager.GetDayRuleData(GameManager_GameManager.RunModel.CurrentDay);
        }

        private float GetDayDuration()
        {
            return _currentDayRuleData != null ? _currentDayRuleData.DayDuration : _dayDuration;
        }

        private float GetNightDuration()
        {
            return _currentDayRuleData != null ? _currentDayRuleData.NightDuration : _nightDuration;
        }

        private float GetMorningRewardDuration()
        {
            return _currentDayRuleData != null ? _currentDayRuleData.MorningRewardDuration : _morningRewardDuration;
        }

        private float GetBossNightDuration()
        {
            return _currentDayRuleData != null ? _currentDayRuleData.BossNightDuration : _bossNightDuration;
        }

        private int GetNightSpawnMax()
        {
            if (_currentDayRuleData != null)
            {
                return _currentDayRuleData.NightSpawnMax;
            }

            return _nightSpawnMax + GameManager_GameManager.RunModel.CurrentDay;
        }

        private int GetBossSpawnMax()
        {
            return _currentDayRuleData != null ? _currentDayRuleData.BossSpawnMax : _bossSpawnMax;
        }

        private float GetSpawnInterval()
        {
            if (_currentDayRuleData != null)
            {
                return _currentDayRuleData.SpawnInterval;
            }

            return Mathf.Max(1.8f, 4.4f - GameManager_GameManager.RunModel.CurrentDay * 0.55f);
        }

        private string GetDayObjective()
        {
            return _currentDayRuleData != null ? _currentDayRuleData.DayObjective : "Gather stone, ore, chests, and build fixed sites.";
        }

        private string GetNightObjective()
        {
            return _currentDayRuleData != null ? _currentDayRuleData.NightObjective : "Defend the FlameHeart.";
        }

        private void ResolveReferences()
        {
            if (GameManager_GameManager == null)
            {
                GameManager_GameManager = FindFirstObjectByType<GameManager>();
            }

            if (CinderkeepEnemySpawner_EnemySpawner == null)
            {
                CinderkeepEnemySpawner_EnemySpawner = FindFirstObjectByType<CinderkeepEnemySpawner>();
            }

            if (CinderkeepFlameHeart_FlameHeart == null)
            {
                CinderkeepFlameHeart_FlameHeart = FindFirstObjectByType<CinderkeepFlameHeart>();
            }

            if (Light_Sun == null)
            {
                Light_Sun = FindFirstObjectByType<Light>();
            }
        }
    }
}
