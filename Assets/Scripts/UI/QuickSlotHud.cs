using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cinderkeep.Gameplay
{
    // Always-visible 1~7 HUD bar for the current quick slots.
    public sealed class QuickSlotHud : MonoBehaviour
    {
        private const string CanvasName = "Canvas_GameHUD";
        private const string HudRootName = "Panel_HUDRoot";
        private const string HudObjectName = "Panel_QuickSlotHud_5_26";
        private const string TitleObjectName = "Text_QuickSlotHudTitle";
        private const int SlotCount = PlayerInventoryModel.QuickSlotCount;

        private readonly TMP_Text[] _slotTexts = new TMP_Text[SlotCount];
        private readonly Image[] _slotBackgrounds = new Image[SlotCount];
        private PlayerInventoryModel _inventoryModel;
        private float _nextRefreshTime;

        public static QuickSlotHud EnsureSceneHud()
        {
            QuickSlotHud existing = FindFirstObjectByType<QuickSlotHud>(FindObjectsInactive.Include);
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                existing.RefreshNow();
                return existing;
            }

            GameObject canvasObject = GameObject.Find(CanvasName);
            if (canvasObject == null)
            {
                return null;
            }

            Transform hudRoot = canvasObject.transform.Find(HudRootName);
            if (hudRoot == null)
            {
                GameObject hudRootObject = new GameObject(HudRootName, typeof(RectTransform));
                hudRootObject.transform.SetParent(canvasObject.transform, false);
                hudRoot = hudRootObject.transform;
            }

            GameObject rootObject = new GameObject(HudObjectName, typeof(RectTransform), typeof(Image));
            rootObject.transform.SetParent(hudRoot, false);
            ConfigureRoot(rootObject.GetComponent<RectTransform>(), rootObject.GetComponent<Image>());

            QuickSlotHud hud = rootObject.AddComponent<QuickSlotHud>();
            CreateTitle(rootObject.transform);
            hud.CreateSlots(rootObject.transform);
            hud.RefreshNow();
            return hud;
        }

        private static void ConfigureRoot(RectTransform rectTransform, Image backgroundImage)
        {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            rectTransform.anchoredPosition = new Vector2(34f, 112f);
            rectTransform.sizeDelta = new Vector2(526f, 76f);

            backgroundImage.color = new Color(0.02f, 0.035f, 0.05f, 0.86f);
            backgroundImage.raycastTarget = false;
        }

        private static void CreateTitle(Transform root)
        {
            GameObject titleObject = new GameObject(TitleObjectName, typeof(RectTransform));
            titleObject.transform.SetParent(root, false);

            RectTransform titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -4f);
            titleRect.sizeDelta = new Vector2(-12f, 20f);

            TMP_Text titleText = titleObject.AddComponent<TextMeshProUGUI>();
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.fontSize = 13f;
            titleText.color = new Color(0.92f, 0.94f, 0.98f, 1f);
            titleText.raycastTarget = false;
            titleText.text = "Quick Slot 1-7";
        }

        private void OnEnable()
        {
            ConnectInventoryModel();
            RefreshNow();
        }

        private void OnDisable()
        {
            DisconnectInventoryModel();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefreshTime)
            {
                return;
            }

            _nextRefreshTime = Time.unscaledTime + 0.15f;
            ConnectInventoryModel();
            RefreshNow();
        }

        public void RefreshNow()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                RefreshSlot(i);
            }
        }

        private void CreateSlots(Transform root)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                GameObject slotObject = new GameObject("HUD_QuickSlot_" + (i + 1), typeof(RectTransform), typeof(Image));
                slotObject.transform.SetParent(root, false);

                RectTransform slotRect = slotObject.GetComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0f, 0.5f);
                slotRect.anchorMax = new Vector2(0f, 0.5f);
                slotRect.pivot = new Vector2(0f, 0.5f);
                slotRect.anchoredPosition = new Vector2(8f + i * 72f, -12f);
                slotRect.sizeDelta = new Vector2(64f, 48f);

                Image slotImage = slotObject.GetComponent<Image>();
                slotImage.raycastTarget = false;
                _slotBackgrounds[i] = slotImage;

                GameObject textObject = new GameObject("Text_HUD_QuickSlot_" + (i + 1), typeof(RectTransform));
                textObject.transform.SetParent(slotObject.transform, false);

                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(3f, 2f);
                textRect.offsetMax = new Vector2(-3f, -2f);

                TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 13f;
                text.color = Color.white;
                text.raycastTarget = false;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                _slotTexts[i] = text;
            }
        }

        private void ConnectInventoryModel()
        {
            PlayerInventoryModel currentInventoryModel = GameManager.Inst == null
                ? null
                : GameManager.Inst.PlayerInventoryModel;

            if (_inventoryModel == currentInventoryModel)
            {
                return;
            }

            DisconnectInventoryModel();
            _inventoryModel = currentInventoryModel;
            if (_inventoryModel != null)
            {
                _inventoryModel.OnInventoryChanged += RefreshNow;
            }
        }

        private void DisconnectInventoryModel()
        {
            if (_inventoryModel != null)
            {
                _inventoryModel.OnInventoryChanged -= RefreshNow;
            }

            _inventoryModel = null;
        }

        private void RefreshSlot(int slotIndex)
        {
            InventoryItemModel itemModel = _inventoryModel == null ? null : _inventoryModel.GetQuickSlotItem(slotIndex);
            bool hasItem = itemModel != null && itemModel.IsEmpty == false;

            if (_slotBackgrounds[slotIndex] != null)
            {
                _slotBackgrounds[slotIndex].color = hasItem
                    ? ResolveItemColor(itemModel.ItemType)
                    : new Color(0.08f, 0.10f, 0.12f, 0.86f);
            }

            if (_slotTexts[slotIndex] == null)
            {
                return;
            }

            string number = (slotIndex + 1).ToString();
            _slotTexts[slotIndex].text = hasItem
                ? number + "\n" + GetShortItemName(itemModel)
                : number;
        }

        private static string GetShortItemName(InventoryItemModel itemModel)
        {
            string displayName = UiItemDisplayFormatter.GetItemName(itemModel);
            if (string.IsNullOrEmpty(displayName))
            {
                return string.Empty;
            }

            return displayName.Length > 6 ? displayName.Substring(0, 6) : displayName;
        }

        private static Color ResolveItemColor(InventoryItemType itemType)
        {
            if (itemType == InventoryItemType.Weapon)
            {
                return new Color(0.45f, 0.12f, 0.10f, 0.95f);
            }

            if (itemType == InventoryItemType.Tool)
            {
                return new Color(0.35f, 0.30f, 0.18f, 0.95f);
            }

            if (itemType == InventoryItemType.Food)
            {
                return new Color(0.10f, 0.34f, 0.18f, 0.95f);
            }

            if (itemType == InventoryItemType.Building)
            {
                return new Color(0.12f, 0.24f, 0.42f, 0.95f);
            }

            return new Color(0.16f, 0.18f, 0.22f, 0.95f);
        }
    }
}
