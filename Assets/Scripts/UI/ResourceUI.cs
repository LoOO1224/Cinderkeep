using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;

public sealed class ResourceUI : MonoBehaviour
{
    [Header("Resource Text UI")]
    [SerializeField] private TMP_Text Text_Wood;
    [SerializeField] private TMP_Text Text_Stone;
    [SerializeField] private TMP_Text Text_Iron;
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
