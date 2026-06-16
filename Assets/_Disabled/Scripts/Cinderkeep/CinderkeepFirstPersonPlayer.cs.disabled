using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OODong.Cinderkeep
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CinderkeepInventory))]
    // 1인칭 플레이어 입력 허브.
    // 현재 MVP에서는 이동/시야/상호작용/퀵슬롯 사용을 한 파일에 모아두었다.
    // TODO(팀원 작업 요청): 작업량이 늘어나면 Movement, Interaction, QuickSlotUse 컴포넌트로 분리해 주세요.
    public sealed class CinderkeepFirstPersonPlayer : MonoBehaviour
    {
        [SerializeField] private CharacterController CharacterController_Character;
        [SerializeField] private Camera Camera_Player;
        [SerializeField] private CinderkeepInventory CinderkeepInventory_Inventory;
        [SerializeField] private CinderkeepHudView CinderkeepHudView_HudView;
        [SerializeField] private CinderkeepPickaxeView CinderkeepPickaxeView_PickaxeView;
        [SerializeField] private CinderkeepAutoShooter CinderkeepAutoShooter_ManualShooter;
        [SerializeField] private CinderkeepPlaceableItemFactory CinderkeepPlaceableItemFactory_PlaceableItemFactory;
        [SerializeField] private LayerMask _interactionMask = ~0;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _lookSensitivity = 0.12f;
        [SerializeField] private float _gravity = -18f;
        [SerializeField] private float _interactionDistance = 4f;
        [SerializeField] private int _pickaxeDamage = 1;
        [SerializeField] private bool _lockCursorOnStart = true;

        private Vector3 _verticalVelocity;
        private ICinderkeepInteractable _currentInteractable;
        private int _selectedQuickSlotIndex;
        private float _yaw;
        private float _pitch;
        private bool _isCursorLocked;

        public CinderkeepInventory Inventory => CinderkeepInventory_Inventory;
        public int SelectedQuickSlotIndex => _selectedQuickSlotIndex;

        private void Awake()
        {
            ResolveReferences();
            _yaw = transform.eulerAngles.y;
            _pitch = Camera_Player != null ? Camera_Player.transform.localEulerAngles.x : 0f;
        }

        private void Start()
        {
            SetCursorLock(_lockCursorOnStart);
            CinderkeepHudView_HudView?.SetInventory(CinderkeepInventory_Inventory);
            CinderkeepHudView_HudView?.SetInventoryOpen(false);
            CinderkeepHudView_HudView?.SetSelectedQuickSlot(_selectedQuickSlotIndex);
            CinderkeepHudView_HudView?.SetStatus("Tab/I: Inventory, E: Interact, Left Click: Use quick slot, 1-7: Select");
        }

        private void Update()
        {
            // 입력은 매 프레임 읽지만, 실제 행동은 Inventory/Interactable/Shooter/Factory로 위임한다.
            ToggleCursorLockFromInput();
            LookFromInput();
            MoveFromInput();
            SelectQuickSlotFromInput();
            FindInteractable();

            if (WasInteractPressed() && _currentInteractable != null)
            {
                _currentInteractable.Interact(this);
            }

            if (_isCursorLocked && WasPrimaryUsePressed())
            {
                UseSelectedQuickSlot(false);
            }

            if (_isCursorLocked && WasSecondaryUsePressed())
            {
                UseSelectedQuickSlot(true);
            }
        }

        public bool HasItem(CinderkeepItemId itemId)
        {
            return CinderkeepInventory_Inventory != null && CinderkeepInventory_Inventory.HasItem(itemId);
        }

        public CinderkeepItemId GetSelectedQuickSlotItem()
        {
            return CinderkeepInventory_Inventory != null ? CinderkeepInventory_Inventory.GetQuickSlotItem(_selectedQuickSlotIndex) : CinderkeepItemId.None;
        }

        public void AddItem(CinderkeepItemId itemId, int count)
        {
            CinderkeepInventory_Inventory?.AddItem(itemId, count);
        }

        public void PlayPickaxeSwing()
        {
            CinderkeepPickaxeView_PickaxeView?.PlaySwing();
        }

        public void ShowStatus(string message)
        {
            CinderkeepHudView_HudView?.SetStatus(message);
        }

        public void ShowMiningProgress(float progress)
        {
            CinderkeepHudView_HudView?.SetMiningProgress(progress);
        }

        public void SetReferences(
            CharacterController characterController,
            Camera playerCamera,
            CinderkeepInventory inventory,
            CinderkeepHudView hudView,
            CinderkeepPickaxeView pickaxeView)
        {
            CharacterController_Character = characterController;
            Camera_Player = playerCamera;
            CinderkeepInventory_Inventory = inventory;
            CinderkeepHudView_HudView = hudView;
            CinderkeepPickaxeView_PickaxeView = pickaxeView;
            CinderkeepAutoShooter_ManualShooter = GetComponent<CinderkeepAutoShooter>();
            CinderkeepPlaceableItemFactory_PlaceableItemFactory = GetComponent<CinderkeepPlaceableItemFactory>();
        }

        private void ResolveReferences()
        {
            if (CharacterController_Character == null)
            {
                CharacterController_Character = GetComponent<CharacterController>();
            }

            if (CinderkeepInventory_Inventory == null)
            {
                CinderkeepInventory_Inventory = GetComponent<CinderkeepInventory>();
            }

            if (Camera_Player == null)
            {
                Camera_Player = GetComponentInChildren<Camera>();
            }

            if (CinderkeepAutoShooter_ManualShooter == null)
            {
                CinderkeepAutoShooter_ManualShooter = GetComponent<CinderkeepAutoShooter>();
            }

            if (CinderkeepPlaceableItemFactory_PlaceableItemFactory == null)
            {
                CinderkeepPlaceableItemFactory_PlaceableItemFactory = GetComponent<CinderkeepPlaceableItemFactory>();
            }
        }

        private void MoveFromInput()
        {
            Vector2 movementInput = ReadMovementInput();
            Vector3 movement = (transform.right * movementInput.x) + (transform.forward * movementInput.y);
            movement.y = 0f;

            if (CharacterController_Character.isGrounded && _verticalVelocity.y < 0f)
            {
                _verticalVelocity.y = -2f;
            }

            _verticalVelocity.y += _gravity * Time.deltaTime;
            Vector3 velocity = (movement.normalized * _moveSpeed) + _verticalVelocity;
            CharacterController_Character.Move(velocity * Time.deltaTime);
        }

        private void LookFromInput()
        {
            if (!_isCursorLocked || Camera_Player == null)
            {
                return;
            }

            Vector2 lookDelta = ReadLookDelta();
            if (lookDelta.sqrMagnitude <= 0f)
            {
                return;
            }

            _yaw += lookDelta.x * _lookSensitivity;
            _pitch = Mathf.Clamp(_pitch - lookDelta.y * _lookSensitivity, -70f, 72f);
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            Camera_Player.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void FindInteractable()
        {
            // 직접 참조 대신 Raycast + 인터페이스(ICinderkeepInteractable)로 느슨하게 연결한다.
            // 새로운 상호작용 오브젝트는 이 인터페이스만 구현하면 플레이어 코드를 수정하지 않아도 된다.
            _currentInteractable = null;

            if (Camera_Player == null)
            {
                CinderkeepHudView_HudView?.SetPrompt(string.Empty);
                return;
            }

            Ray ray = new Ray(Camera_Player.transform.position, Camera_Player.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionMask, QueryTriggerInteraction.Collide))
            {
                CinderkeepHudView_HudView?.SetPrompt(string.Empty);
                return;
            }

            MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ICinderkeepInteractable interactable && interactable.CanInteract(this))
                {
                    _currentInteractable = interactable;
                    CinderkeepHudView_HudView?.SetPrompt(interactable.GetPrompt());
                    return;
                }
            }

            CinderkeepHudView_HudView?.SetPrompt(string.Empty);
        }

        private void SelectQuickSlotFromInput()
        {
            for (int i = 0; i < CinderkeepInventoryModel.QuickSlotCount; i++)
            {
                if (WasQuickSlotPressed(i))
                {
                    _selectedQuickSlotIndex = i;
                    CinderkeepHudView_HudView?.SetSelectedQuickSlot(_selectedQuickSlotIndex);
                    CinderkeepItemId itemId = GetSelectedQuickSlotItem();
                    ShowStatus($"Selected {i + 1}: {CinderkeepItemCatalog.GetDisplayName(itemId)}");
                }
            }
        }

        private void ToggleCursorLockFromInput()
        {
            if (WasInventoryTogglePressed())
            {
                bool nextOpen = CinderkeepHudView_HudView == null || !CinderkeepHudView_HudView.IsInventoryOpen;
                CinderkeepHudView_HudView?.SetInventoryOpen(nextOpen);
                SetCursorLock(!nextOpen);
            }

            if (WasCancelPressed())
            {
                CinderkeepHudView_HudView?.SetInventoryOpen(false);
                SetCursorLock(false);
            }
        }

        private void UseSelectedQuickSlot(bool isSecondaryUse)
        {
            // 퀵슬롯 사용 분기.
            // TODO(팀원 작업 요청): 아이템 종류가 늘어나면 switch를 데이터 기반 ItemUseHandler로 분리해 주세요.
            CinderkeepItemId itemId = GetSelectedQuickSlotItem();
            switch (itemId)
            {
                case CinderkeepItemId.Pickaxe:
                    UsePickaxe();
                    break;
                case CinderkeepItemId.Arrow:
                    UseArrow();
                    break;
                case CinderkeepItemId.Stone:
                case CinderkeepItemId.Ore:
                    CinderkeepPlaceableItemFactory_PlaceableItemFactory?.TryPlaceItem(this, Camera_Player, itemId);
                    break;
                case CinderkeepItemId.Apple:
                    UseApple();
                    break;
                default:
                    ShowStatus(isSecondaryUse ? "Right click: no quick slot item" : "Left click: no quick slot item");
                    break;
            }
        }

        private void UsePickaxe()
        {
            PlayPickaxeSwing();

            if (_currentInteractable is CinderkeepMineableNode)
            {
                _currentInteractable.Interact(this);
                return;
            }

            if (TryHitEnemy())
            {
                ShowStatus("Pickaxe hit enemy");
                return;
            }

            ShowStatus("Pickaxe swing");
        }

        private void UseArrow()
        {
            if (CinderkeepInventory_Inventory == null || !CinderkeepInventory_Inventory.HasItem(CinderkeepItemId.Arrow))
            {
                ShowStatus("Arrow is empty");
                return;
            }

            if (CinderkeepAutoShooter_ManualShooter == null || !CinderkeepAutoShooter_ManualShooter.TryFireNearestEnemy())
            {
                ShowStatus("No enemy in arrow range");
                return;
            }

            CinderkeepInventory_Inventory.TryRemoveItem(CinderkeepItemId.Arrow, 1);
            ShowStatus("Arrow fired");
        }

        private bool TryHitEnemy()
        {
            if (Camera_Player == null)
            {
                return false;
            }

            Ray ray = new Ray(Camera_Player.transform.position, Camera_Player.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionMask, QueryTriggerInteraction.Collide))
            {
                return false;
            }

            CinderkeepEnemy enemy = hit.collider.GetComponentInParent<CinderkeepEnemy>();
            if (enemy == null)
            {
                return false;
            }

            enemy.TakeDamage(_pickaxeDamage);
            return true;
        }

        private void UseApple()
        {
            if (CinderkeepInventory_Inventory == null || !CinderkeepInventory_Inventory.TryRemoveItem(CinderkeepItemId.Apple, 1))
            {
                ShowStatus("Apple is empty");
                return;
            }

            ShowStatus("Apple used");
        }

        private void SetCursorLock(bool isLocked)
        {
            _isCursorLocked = isLocked;
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        private Vector2 ReadMovementInput()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                horizontal += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                vertical -= 1f;
            }

            if (keyboard.wKey.isPressed)
            {
                vertical += 1f;
            }

            return new Vector2(horizontal, vertical);
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private Vector2 ReadLookDelta()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 12f;
#endif
        }

        private bool WasInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        private bool WasPrimaryUsePressed()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private bool WasSecondaryUsePressed()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            return mouse != null && mouse.rightButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(1);
#endif
        }

        private bool WasInventoryTogglePressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && (keyboard.tabKey.wasPressedThisFrame || keyboard.iKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I);
#endif
        }

        private bool WasCancelPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private bool WasQuickSlotPressed(int zeroBasedIndex)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            Key key = GetQuickSlotKey(zeroBasedIndex);
            return key != Key.None && keyboard[key].wasPressedThisFrame;
#else
            KeyCode keyCode = GetQuickSlotKeyCode(zeroBasedIndex);
            return keyCode != KeyCode.None && Input.GetKeyDown(keyCode);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private Key GetQuickSlotKey(int zeroBasedIndex)
        {
            switch (zeroBasedIndex)
            {
                case 0:
                    return Key.Digit1;
                case 1:
                    return Key.Digit2;
                case 2:
                    return Key.Digit3;
                case 3:
                    return Key.Digit4;
                case 4:
                    return Key.Digit5;
                case 5:
                    return Key.Digit6;
                case 6:
                    return Key.Digit7;
                default:
                    return Key.None;
            }
        }
#else
        private KeyCode GetQuickSlotKeyCode(int zeroBasedIndex)
        {
            switch (zeroBasedIndex)
            {
                case 0:
                    return KeyCode.Alpha1;
                case 1:
                    return KeyCode.Alpha2;
                case 2:
                    return KeyCode.Alpha3;
                case 3:
                    return KeyCode.Alpha4;
                case 4:
                    return KeyCode.Alpha5;
                case 5:
                    return KeyCode.Alpha6;
                case 6:
                    return KeyCode.Alpha7;
                default:
                    return KeyCode.None;
            }
        }
#endif
    }
}
