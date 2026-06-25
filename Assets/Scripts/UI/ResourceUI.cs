using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;

// 플레이 상태를 화면에 표시하거나 사용자의 UI 요청을 전달합니다.
// UI는 규칙을 소유하지 않고 모델을 읽고 시스템에 요청을 보내는 계층으로 유지합니다.
// 플레이어가 가진 자원 수량을 화면에 표시하는 UI 컴포넌트입니다.
// 자원 데이터는 PlayerModel이 가지고, 이 클래스는 표시와 이벤트 연결만 담당합니다.
public sealed class ResourceUI : MonoBehaviour
{
    [Header("Resource Text UI")]
    [SerializeField] private TMP_Text _woodText;
    [SerializeField] private TMP_Text _stoneText;
    [SerializeField] private TMP_Text _ironText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _mithrilText;
    [SerializeField] private TMP_Text _adamantiumText;

    private PlayerModel _playerModel;

    private void Start()
    {
        ConnectPlayerModel();
        SubscribeResourceChanged();
        RefreshResourceUI();
    }

    private void OnDestroy()
    {
        UnsubscribeResourceChanged();
    }

    public void SetPlayerModel(PlayerModel playerModel)
    {
        UnsubscribeResourceChanged();
        _playerModel = playerModel;
        SubscribeResourceChanged();
        RefreshResourceUI();
    }

    private void ConnectPlayerModel()
    {
        if (_playerModel != null)
        {
            return;
        }

        if (GameManager.Inst == null)
        {
            return;
        }

        _playerModel = GameManager.Inst.PlayerModel;
    }

    private void SubscribeResourceChanged()
    {
        if (_playerModel == null)
        {
            return;
        }

        _playerModel.OnResourceChanged += RefreshResourceUI;
    }

    private void UnsubscribeResourceChanged()
    {
        if (_playerModel == null)
        {
            return;
        }

        _playerModel.OnResourceChanged -= RefreshResourceUI;
    }

    private void RefreshResourceUI()
    {
        if (_playerModel == null)
        {
            return;
        }

        RefreshText(_woodText, _playerModel.Wood);
        RefreshText(_stoneText, _playerModel.Stone);
        RefreshText(_ironText, _playerModel.Iron);
        RefreshText(_goldText, _playerModel.Gold);
        RefreshText(_mithrilText, _playerModel.Mithril);
        RefreshText(_adamantiumText, _playerModel.Adamantium);
    }

    private void RefreshText(TMP_Text textResource, int amount)
    {
        if (textResource == null)
        {
            return;
        }

        textResource.text = amount.ToString();
    }
}
