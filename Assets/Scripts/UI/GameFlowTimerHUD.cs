using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;

// 현재 낮/밤/아침/보스 페이즈의 시간과 실행 모드를 표시하는 HUD 컴포넌트입니다.
// 시간 계산은 GameRunModel을 읽고, 실제 페이즈 전환은 GameFlowController가 담당합니다.
public sealed class GameFlowTimerHUD : MonoBehaviour
{
    [Header("Connected Model")]
    [Tooltip("GameRunModel을 가진 GameManager입니다. 비어 있으면 GameManager.Inst를 사용합니다.")]
    [SerializeField] private GameManager _gameManager;

    [Header("Text UI")]
    [Tooltip("현재 페이즈의 경과 시간과 총 시간을 표시하는 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text _timerText;
    [Tooltip("현재 페이즈 이름과 실행 모드를 표시하는 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text _phaseText;

    private GameRunModel _gameRunModel;

    private void Start()
    {
        ConnectGameRunModel();
    }

    private void Update()
    {
        RefreshHUD();
    }

    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
        ConnectGameRunModel();
    }

    private void ConnectGameRunModel()
    {
        if (_gameManager == null)
        {
            _gameManager = GameManager.Inst;
        }

        if (_gameManager == null)
        {
            return;
        }

        _gameRunModel = _gameManager.GameRunModel;
    }

    private void RefreshHUD()
    {
        if (_gameRunModel == null)
        {
            ConnectGameRunModel();
        }

        if (_gameRunModel == null)
        {
            return;
        }

        RefreshTimerText();
        RefreshPhaseText();
    }

    private void RefreshTimerText()
    {
        if (_timerText == null)
        {
            return;
        }

        float phaseDuration = Mathf.Max(0f, _gameRunModel.PhaseDuration);
        float elapsedTime = Mathf.Clamp(phaseDuration - _gameRunModel.RemainingTime, 0f, phaseDuration);
        _timerText.text = FormatTime(elapsedTime) + " / " + FormatTime(phaseDuration);
    }

    private void RefreshPhaseText()
    {
        if (_phaseText == null)
        {
            return;
        }

        _phaseText.text = GetPhaseDisplayText(_gameRunModel.Phase) + " / " + GameLaunchSettings.ModeDisplayName;
    }

    private string FormatTime(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private string GetPhaseDisplayText(GameRunPhase phase)
    {
        switch (phase)
        {
            case GameRunPhase.Day:
                return "낮";
            case GameRunPhase.Night:
                return "밤";
            case GameRunPhase.MorningReward:
                return "아침 정비";
            case GameRunPhase.BossApproach:
                return "보스 접근";
            case GameRunPhase.BossFight:
                return "보스 전투";
            case GameRunPhase.GameOver:
                return "게임 오버";
            case GameRunPhase.Clear:
                return "클리어";
        }

        return "대기";
    }
}
