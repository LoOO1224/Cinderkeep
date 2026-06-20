using Cinderkeep.Gameplay;
using UnityEngine;

// 플레이어가 E키로 제작 시설을 열 수 있게 하는 상호작용 컴포넌트입니다.
// 실제 제작법 필터링은 CraftingStation이 담당하고, 이 클래스는 입력 전달만 담당합니다.
public sealed class CraftingStationInteractable : MonoBehaviour, IInteractable
{
    [Header("Connected Station")]
    [Tooltip("이 오브젝트가 열어줄 제작 시설 컴포넌트입니다.")]
    [SerializeField] private CraftingStation _craftingStation;
    [Tooltip("현재 제작 시설 상호작용을 허용할지 결정합니다.")]
    [SerializeField] private bool _canInteract = true;

    private void Start()
    {
        ConnectCraftingStation();
    }

    public bool CanInteract(GameObject gameObjectInteractor)
    {
        ConnectCraftingStation();

        if (_canInteract == false)
        {
            return false;
        }

        if (_craftingStation == null)
        {
            return false;
        }

        return _craftingStation.CanOpen(gameObjectInteractor);
    }

    public void Interact(GameObject gameObjectInteractor)
    {
        if (CanInteract(gameObjectInteractor) == false)
        {
            return;
        }

        _craftingStation.OpenStation(gameObjectInteractor);
        OpenStationUI(gameObjectInteractor);
    }

    private void ConnectCraftingStation()
    {
        if (_craftingStation != null)
        {
            return;
        }

        _craftingStation = GetComponent<CraftingStation>();
    }

    private void OpenStationUI(GameObject gameObjectInteractor)
    {
        if (GameManager.Inst == null)
        {
            return;
        }

        UIManager uiManager = GameManager.Inst.GetUIManager();
        if (uiManager == null)
        {
            return;
        }

        FurnaceStation furnaceStation = GetComponent<FurnaceStation>();
        if (furnaceStation != null)
        {
            uiManager.OpenFurnaceUI(furnaceStation, gameObjectInteractor);
            return;
        }

        uiManager.OpenCraftingUI(_craftingStation, gameObjectInteractor);
    }
}
