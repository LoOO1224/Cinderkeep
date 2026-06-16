using UnityEngine;
using UnityEngine.UI;

namespace OODong.Cinderkeep
{
    // Cinderkeep 게임 화면 전용 HUD View.
    // UIManager는 공통 Open/Close만 담당하고, 실제 텍스트 갱신은 이 View가 맡는다.
    public sealed class CinderkeepHudView : MonoBehaviour
    {
        [SerializeField] private CinderkeepInventory CinderkeepInventory_Inventory;
        [SerializeField] private Text Text_Prompt;
        [SerializeField] private Text Text_Status;
        [SerializeField] private Text Text_Mining;
        [SerializeField] private Text Text_Phase;
        [SerializeField] private Text Text_FlameHeart;
        [SerializeField] private Text Text_Objective;
        [SerializeField] private Image Image_PhaseFill;
        [SerializeField] private Image Image_MiningFill;
        [SerializeField] private GameObject GameObject_InventoryPanel;
        [SerializeField] private CinderkeepInventoryItemDragView[] CinderkeepInventoryItemDragView_InventoryItemViews;
        [SerializeField] private CinderkeepQuickSlotDropTarget[] CinderkeepQuickSlotDropTarget_QuickSlotViews;

        private readonly CinderkeepItemId[] _inventoryOrder =
        {
            CinderkeepItemId.Arrow,
            CinderkeepItemId.Pickaxe,
            CinderkeepItemId.Stone,
            CinderkeepItemId.Ore,
            CinderkeepItemId.Apple
        };

        private int _selectedQuickSlotIndex;
        private bool _isInventoryOpen;

        public bool IsInventoryOpen => _isInventoryOpen;

        private void Awake()
        {
            if (CinderkeepInventory_Inventory == null)
            {
                CinderkeepInventory_Inventory = FindFirstObjectByType<CinderkeepInventory>();
            }

            SetInventory(CinderkeepInventory_Inventory);
            SetMiningProgress(0f);
            SetInventoryOpen(false);
        }

        private void OnDestroy()
        {
            if (CinderkeepInventory_Inventory != null)
            {
                CinderkeepInventory_Inventory.Changed -= Refresh;
            }
        }

        public void SetInventory(CinderkeepInventory inventory)
        {
            if (CinderkeepInventory_Inventory != null)
            {
                CinderkeepInventory_Inventory.Changed -= Refresh;
            }

            CinderkeepInventory_Inventory = inventory;

            if (CinderkeepInventory_Inventory != null)
            {
                CinderkeepInventory_Inventory.Changed += Refresh;
            }

            BindChildViews();
            Refresh();
        }

        public void SetViewReferences(
            Text promptText,
            Text statusText,
            Text miningText,
            Text phaseText,
            Text flameHeartText,
            Text objectiveText,
            Image phaseFillImage,
            Image miningFillImage,
            GameObject inventoryPanel,
            CinderkeepInventoryItemDragView[] inventoryItemViews,
            CinderkeepQuickSlotDropTarget[] quickSlotViews)
        {
            Text_Prompt = promptText;
            Text_Status = statusText;
            Text_Mining = miningText;
            Text_Phase = phaseText;
            Text_FlameHeart = flameHeartText;
            Text_Objective = objectiveText;
            Image_PhaseFill = phaseFillImage;
            Image_MiningFill = miningFillImage;
            GameObject_InventoryPanel = inventoryPanel;
            CinderkeepInventoryItemDragView_InventoryItemViews = inventoryItemViews;
            CinderkeepQuickSlotDropTarget_QuickSlotViews = quickSlotViews;
            BindChildViews();
        }

        public void ToggleInventory()
        {
            SetInventoryOpen(!_isInventoryOpen);
        }

        public void SetInventoryOpen(bool isOpen)
        {
            _isInventoryOpen = isOpen;
            if (GameObject_InventoryPanel != null)
            {
                GameObject_InventoryPanel.SetActive(isOpen);
            }
        }

        public void SetPrompt(string prompt)
        {
            if (Text_Prompt != null)
            {
                Text_Prompt.text = prompt;
            }
        }

        public void SetStatus(string status)
        {
            if (Text_Status != null)
            {
                Text_Status.text = status;
            }
        }

        public void SetSelectedQuickSlot(int slotIndex)
        {
            _selectedQuickSlotIndex = Mathf.Clamp(slotIndex, 0, CinderkeepInventoryModel.QuickSlotCount - 1);
            RefreshQuickSlots();
        }

        public void SetMiningProgress(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            if (Image_MiningFill != null)
            {
                Image_MiningFill.fillAmount = clampedProgress;
            }

            if (Text_Mining != null)
            {
                Text_Mining.text = clampedProgress > 0f && clampedProgress < 1f
                    ? $"Mining {Mathf.RoundToInt(clampedProgress * 100f)}%"
                    : string.Empty;
            }
        }

        public void SetRunInfo(CinderkeepRunModel runModel, CinderkeepFlameHeartModel flameHeartModel)
        {
            // GameManager가 들고 있는 Model을 읽어서 표시만 한다.
            // HUD에서 게임 상태를 직접 바꾸지 않는 것이 핵심이다.
            if (runModel == null)
            {
                return;
            }

            if (Text_Phase != null)
            {
                Text_Phase.text = $"Cinderkeep {runModel.Phase} | Day {runModel.CurrentDay}/{runModel.MaxDay} | {GameUtil.FormatClock(runModel.PhaseRemaining)}";
            }

            if (Image_PhaseFill != null)
            {
                float phaseProgress = runModel.PhaseDuration <= 0f ? 0f : 1f - (runModel.PhaseRemaining / runModel.PhaseDuration);
                Image_PhaseFill.fillAmount = Mathf.Clamp01(phaseProgress);
            }

            if (Text_FlameHeart != null && flameHeartModel != null)
            {
                Text_FlameHeart.text = $"FlameHeart {flameHeartModel.CurrentHealth}/{flameHeartModel.MaxHealth}";
            }

            if (Text_Objective != null)
            {
                Text_Objective.text = GetObjectiveText(runModel);
            }
        }

        private void Refresh()
        {
            RefreshInventoryRows();
            RefreshQuickSlots();
        }

        private void RefreshInventoryRows()
        {
            if (CinderkeepInventoryItemDragView_InventoryItemViews == null || CinderkeepInventory_Inventory == null)
            {
                return;
            }

            for (int i = 0; i < CinderkeepInventoryItemDragView_InventoryItemViews.Length && i < _inventoryOrder.Length; i++)
            {
                CinderkeepItemId itemId = _inventoryOrder[i];
                CinderkeepInventoryItemDragView_InventoryItemViews[i].SetInventory(CinderkeepInventory_Inventory);
                CinderkeepInventoryItemDragView_InventoryItemViews[i].SetItem(itemId);
                CinderkeepInventoryItemDragView_InventoryItemViews[i].Refresh(CinderkeepInventory_Inventory.GetItemCount(itemId));
            }
        }

        private void RefreshQuickSlots()
        {
            if (CinderkeepQuickSlotDropTarget_QuickSlotViews == null || CinderkeepInventory_Inventory == null)
            {
                return;
            }

            for (int i = 0; i < CinderkeepQuickSlotDropTarget_QuickSlotViews.Length; i++)
            {
                CinderkeepQuickSlotDropTarget_QuickSlotViews[i].SetInventory(CinderkeepInventory_Inventory);
                CinderkeepQuickSlotDropTarget_QuickSlotViews[i].SetHudView(this);
                CinderkeepQuickSlotDropTarget_QuickSlotViews[i].SetSlotIndex(i);
                CinderkeepQuickSlotDropTarget_QuickSlotViews[i].Refresh(CinderkeepInventory_Inventory.GetQuickSlotItem(i));
                CinderkeepQuickSlotDropTarget_QuickSlotViews[i].RefreshSelected(i == _selectedQuickSlotIndex);
            }
        }

        private void BindChildViews()
        {
            if (CinderkeepInventoryItemDragView_InventoryItemViews != null)
            {
                for (int i = 0; i < CinderkeepInventoryItemDragView_InventoryItemViews.Length; i++)
                {
                    if (CinderkeepInventoryItemDragView_InventoryItemViews[i] != null)
                    {
                        CinderkeepInventoryItemDragView_InventoryItemViews[i].SetInventory(CinderkeepInventory_Inventory);
                    }
                }
            }

            if (CinderkeepQuickSlotDropTarget_QuickSlotViews != null)
            {
                for (int i = 0; i < CinderkeepQuickSlotDropTarget_QuickSlotViews.Length; i++)
                {
                    if (CinderkeepQuickSlotDropTarget_QuickSlotViews[i] != null)
                    {
                        CinderkeepQuickSlotDropTarget_QuickSlotViews[i].SetInventory(CinderkeepInventory_Inventory);
                        CinderkeepQuickSlotDropTarget_QuickSlotViews[i].SetHudView(this);
                        CinderkeepQuickSlotDropTarget_QuickSlotViews[i].SetSlotIndex(i);
                    }
                }
            }
        }

        private string GetObjectiveText(CinderkeepRunModel runModel)
        {
            switch (runModel.Phase)
            {
                case CinderkeepGamePhase.Day:
                    return "Gather resources and build fixed defense sites.";
                case CinderkeepGamePhase.Night:
                    return "Defend the FlameHeart until morning.";
                case CinderkeepGamePhase.MorningReward:
                    return "Take rewards and prepare for the next day.";
                case CinderkeepGamePhase.BossNight:
                    return "Defeat the Cinder Warden.";
                case CinderkeepGamePhase.Victory:
                case CinderkeepGamePhase.Defeat:
                    return runModel.ResultMessage;
                default:
                    return "Prepare the run.";
            }
        }
    }
}
