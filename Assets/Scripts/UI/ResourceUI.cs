using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;

// 플레이어가 가진 자원 수량을 화면에 표시하는 UI 컴포넌트입니다.
// 자원 데이터는 PlayerModel이 가지고, 이 클래스는 표시와 이벤트 연결만 담당합니다.
public sealed class ResourceUI : MonoBehaviour
{
    [Header("Resource Text UI")]
    [SerializeField] private TMP_Text Text_Wood;
    [SerializeField] private TMP_Text Text_Stone;
    [SerializeField] private TMP_Text Text_Iron;
    [SerializeField] private TMP_Text Text_Gold;
    [SerializeField] private TMP_Text Text_Mithril;
    [SerializeField] private TMP_Text Text_Adamantium;

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

        RefreshText(Text_Wood, _playerModel.Wood);
        RefreshText(Text_Stone, _playerModel.Stone);
        RefreshText(Text_Iron, _playerModel.Iron);
        RefreshText(Text_Gold, _playerModel.Gold);
        RefreshText(Text_Mithril, _playerModel.Mithril);
        RefreshText(Text_Adamantium, _playerModel.Adamantium);
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
