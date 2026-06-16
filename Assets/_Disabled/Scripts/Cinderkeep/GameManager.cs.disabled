using System;
using UnityEngine;

namespace OODong.Cinderkeep
{
    // Cinderkeep의 런타임 중앙 관리자.
    // 강사님 규칙 기준: 진행 중 저장해야 하는 Instance Data(Model)는 여기에서 소유한다.
    // 이 클래스는 "게임 결과/상태"를 결정하지만, 타이머 진행이나 스폰 세부 로직은 Controller/Spawner에게 맡긴다.
    public sealed class GameManager : MonoBehaviour
    {
        // Unity 참조 객체는 타입_역할 이름으로 둔다.
        // 팀원이 Inspector에서 직접 꽂을 수도 있지만, 가능하면 씬 빌더나 ResolveReferences로 자동 연결한다.
        [SerializeField] private GameDataManager GameDataManager_GameDataManager;
        [SerializeField] private CinderkeepHudView CinderkeepHudView_HudView;
        [SerializeField] private CinderkeepFlameHeart CinderkeepFlameHeart_FlameHeart;
        [SerializeField] private CinderkeepEnemySpawner CinderkeepEnemySpawner_EnemySpawner;
        [SerializeField] private CinderkeepInventory CinderkeepInventory_Inventory;
        // 기본 발표 루프는 3일차 보스전까지다.
        // 30분 이상 확장 테스트를 할 때는 씬 빌더나 별도 난이도 UI에서 SetMaxDay(7)을 호출해 주세요.
        [SerializeField] private int _maxDay = 3;

        private readonly CinderkeepRunModel _runModel = new CinderkeepRunModel();
        private readonly CinderkeepPlayerModel _playerModel = new CinderkeepPlayerModel();
        private readonly CinderkeepFlameHeartModel _flameHeartModel = new CinderkeepFlameHeartModel();

        public static GameManager Instance { get; private set; }
        public CinderkeepRunModel RunModel => _runModel;
        public CinderkeepPlayerModel PlayerModel => _playerModel;
        public CinderkeepFlameHeartModel FlameHeartModel => _flameHeartModel;
        public CinderkeepInventory Inventory => CinderkeepInventory_Inventory;
        public bool IsRunActive => _runModel.IsRunActive;

        public event Action<CinderkeepGamePhase> PhaseChanged;
        public event Action RunFinished;

        private void Awake()
        {
            Instance = this;
            ResolveReferences();
        }

        private void Update()
        {
            RefreshHud();
        }

        public void SetSceneReferences(
            GameDataManager gameDataManager,
            CinderkeepHudView hudView,
            CinderkeepFlameHeart flameHeart,
            CinderkeepEnemySpawner enemySpawner,
            CinderkeepInventory inventory)
        {
            GameDataManager_GameDataManager = gameDataManager;
            CinderkeepHudView_HudView = hudView;
            CinderkeepFlameHeart_FlameHeart = flameHeart;
            CinderkeepEnemySpawner_EnemySpawner = enemySpawner;
            CinderkeepInventory_Inventory = inventory;
            CinderkeepFlameHeart_FlameHeart?.SetModel(_flameHeartModel);
        }

        public void StartRun()
        {
            // 새 게임 시작 시 Model을 초기화한다.
            // TODO(팀원 작업 요청): 저장/로드를 붙일 때는 NetworkManager에서 불러온 값을 여기 Model에 주입해 주세요.
            ResolveReferences();
            GameDataManager_GameDataManager?.LoadStaticData();
            CinderkeepFlameHeart_FlameHeart?.SetModel(_flameHeartModel);
            CinderkeepFlameHeart_FlameHeart?.ResetHeart();
            _runModel.StartRun(_maxDay);
            RefreshHud();
        }

        public void SetMaxDay(int maxDay)
        {
            // day_rules.json은 7일차까지 준비되어 있다.
            // Controller 코드를 늘리지 않고 데이터만 바꿔 장기 루프를 테스트할 수 있게 제한한다.
            _maxDay = Mathf.Clamp(maxDay, 1, 7);
        }

        public CinderkeepDayRuleData GetDayRuleData(int dayIndex)
        {
            if (GameDataManager_GameDataManager == null)
            {
                GameDataManager_GameDataManager = GameDataManager.Instance;
            }

            return GameDataManager_GameDataManager != null ? GameDataManager_GameDataManager.GetDayRuleData(dayIndex) : null;
        }

        public void SetPhase(CinderkeepGamePhase phase, float duration)
        {
            _runModel.SetPhase(phase, duration);
            PhaseChanged?.Invoke(phase);
            RefreshHud();
        }

        public void SetPhaseRemaining(float remaining)
        {
            _runModel.SetPhaseRemaining(remaining);
        }

        public void AdvanceDay()
        {
            _runModel.AdvanceDay();
        }

        public void GrantMorningReward()
        {
            // MVP 발표용 보상: 밤을 버티면 화살/사과/FlameHeart 수리 제공.
            // TODO(팀원 작업 요청): 이후에는 relics.json 또는 reward table JSON으로 보상 후보를 데이터화해 주세요.
            if (CinderkeepInventory_Inventory == null)
            {
                return;
            }

            CinderkeepInventory_Inventory.AddItem(CinderkeepItemId.Arrow, 8 + (_runModel.CurrentDay * 2));
            CinderkeepInventory_Inventory.AddItem(CinderkeepItemId.Apple, 1);
            CinderkeepFlameHeart_FlameHeart?.Repair(25);
            ShowStatus($"Morning reward: arrows and repair before day {_runModel.CurrentDay + 1}");
        }

        public void ReportEnemyDefeated(CinderkeepEnemy enemy)
        {
            // Enemy가 직접 승리 판단을 하지 않고 GameManager에 보고한다.
            // 이렇게 해야 보스 처치, 퀘스트, 드랍, 점수 처리를 한 곳에서 확장할 수 있다.
            if (enemy != null && enemy.IsBoss)
            {
                ReportBossDefeated();
            }
        }

        public void ReportBossDefeated()
        {
            if (!_runModel.IsRunActive)
            {
                return;
            }

            _runModel.MarkBossDefeated();
            WinRun($"Boss defeated. Cinderkeep held for {_runModel.MaxDay} days.");
        }

        public void WinRun(string message)
        {
            if (!_runModel.IsRunActive)
            {
                return;
            }

            CinderkeepEnemySpawner_EnemySpawner?.SetSpawningEnabled(false);
            _runModel.Finish(CinderkeepGamePhase.Victory, message);
            ShowStatus(message);
            RefreshHud();
            RunFinished?.Invoke();
        }

        public void LoseRun(string message)
        {
            if (!_runModel.IsRunActive)
            {
                return;
            }

            CinderkeepEnemySpawner_EnemySpawner?.SetSpawningEnabled(false);
            _runModel.Finish(CinderkeepGamePhase.Defeat, message);
            ShowStatus(message);
            RefreshHud();
            RunFinished?.Invoke();
        }

        public void ShowStatus(string message)
        {
            CinderkeepHudView_HudView?.SetStatus(message);
        }

        private void ResolveReferences()
        {
            // 수동 참조가 누락되어도 게임 씬이 동작하도록 최소 자동 탐색을 제공한다.
            // Controller에 참조가 몰리지 않게, 각 객체 자체 참조는 자기 컴포넌트에서 해결하는 것이 원칙이다.
            if (GameDataManager_GameDataManager == null)
            {
                GameDataManager_GameDataManager = FindFirstObjectByType<GameDataManager>();
            }

            if (CinderkeepHudView_HudView == null)
            {
                CinderkeepHudView_HudView = FindFirstObjectByType<CinderkeepHudView>();
            }

            if (CinderkeepFlameHeart_FlameHeart == null)
            {
                CinderkeepFlameHeart_FlameHeart = FindFirstObjectByType<CinderkeepFlameHeart>();
            }

            CinderkeepFlameHeart_FlameHeart?.SetModel(_flameHeartModel);

            if (CinderkeepEnemySpawner_EnemySpawner == null)
            {
                CinderkeepEnemySpawner_EnemySpawner = FindFirstObjectByType<CinderkeepEnemySpawner>();
            }

            if (CinderkeepInventory_Inventory == null)
            {
                CinderkeepInventory_Inventory = FindFirstObjectByType<CinderkeepInventory>();
            }
        }

        private void RefreshHud()
        {
            CinderkeepHudView_HudView?.SetRunInfo(_runModel, FlameHeartModel);
        }
    }
}
