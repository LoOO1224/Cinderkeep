using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cinderkeep.Gameplay
{
    // 용광로 UI를 갱신하는 컴포넌트입니다.
    // 왼쪽 입력 칸, 진행 게이지, 오른쪽 결과 칸을 코드에서 새로 만들지 않고 준비된 UI에 표시합니다.
    public sealed class FurnaceUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _rootObject;

        [Header("Input Slot")]
        [SerializeField] private TMP_Text _inputResourceText;
        [SerializeField] private TMP_Text _inputAmountText;
        [SerializeField] private string _selectedInputResourceId = PlayerModel.ResourceIronOre;
        [SerializeField] private int _selectedInputAmount = 1;

        [Header("Progress UI")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TMP_Text _progressText;

        [Header("Output Slot")]
        [SerializeField] private TMP_Text _outputResourceText;
        [SerializeField] private TMP_Text _outputAmountText;

        [Header("Button UI")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _collectButton;

        [Header("Message UI")]
        [SerializeField] private TMP_Text _messageText;

        private FurnaceStation _currentFurnaceStation;
        private PlayerModel _playerModel;

        private void OnEnable()
        {
            ConnectButtons();
        }

        private void OnDisable()
        {
            DisconnectButtons();
            UnsubscribeFurnace();
        }

        private void Start()
        {
            Close();
        }

        private void Update()
        {
            RefreshUI();
        }

        public void OpenFurnace(FurnaceStation furnaceStation)
        {
            if (furnaceStation == null)
            {
                return;
            }

            SetFurnaceStation(furnaceStation);
            ConnectPlayerModel();
            SetVisible(true);
            RefreshUI();
        }

        public void Close()
        {
            UnsubscribeFurnace();
            SetVisible(false);
        }

        public void SetInputResource(string inputResourceId, int inputAmount)
        {
            _selectedInputResourceId = inputResourceId;
            _selectedInputAmount = inputAmount;
            RefreshUI();
        }

        public void TryStartSmelting()
        {
            ConnectPlayerModel();

            if (_currentFurnaceStation == null || _playerModel == null)
            {
                RefreshMessage("용광로 연결이 아직 준비되지 않았습니다.");
                return;
            }

            if (_currentFurnaceStation.TryStartSmeltingByInputResource(_selectedInputResourceId, _selectedInputAmount, _playerModel))
            {
                RefreshMessage("제련을 시작했습니다.");
                RefreshUI();
                return;
            }

            RefreshMessage("제련할 광석이 부족하거나 사용할 수 없는 재료입니다.");
            RefreshUI();
        }

        public void TryCollectOutput()
        {
            ConnectPlayerModel();

            if (_currentFurnaceStation == null || _playerModel == null)
            {
                RefreshMessage("용광로 연결이 아직 준비되지 않았습니다.");
                return;
            }

            if (_currentFurnaceStation.TryCollectOutput(_playerModel))
            {
                RefreshMessage("주괴를 회수했습니다.");
                RefreshUI();
                return;
            }

            RefreshMessage("회수할 결과물이 없습니다.");
            RefreshUI();
        }

        private void SetFurnaceStation(FurnaceStation furnaceStation)
        {
            if (_currentFurnaceStation == furnaceStation)
            {
                return;
            }

            UnsubscribeFurnace();
            _currentFurnaceStation = furnaceStation;
            SubscribeFurnace();
        }

        private void RefreshUI()
        {
            RefreshInputSlot();
            RefreshProgress();
            RefreshOutputSlot();
            RefreshButtons();
        }

        private void RefreshInputSlot()
        {
            RefreshText(_inputResourceText, _selectedInputResourceId);
            RefreshText(_inputAmountText, _selectedInputAmount.ToString());
        }

        private void RefreshProgress()
        {
            float progress = 0f;
            if (_currentFurnaceStation != null)
            {
                progress = _currentFurnaceStation.GetProgress01();
            }

            if (_progressSlider != null)
            {
                _progressSlider.minValue = 0f;
                _progressSlider.maxValue = 1f;
                _progressSlider.value = progress;
            }

            RefreshText(_progressText, Mathf.RoundToInt(progress * 100f).ToString() + "%");
        }

        private void RefreshOutputSlot()
        {
            if (_currentFurnaceStation == null)
            {
                RefreshText(_outputResourceText, "");
                RefreshText(_outputAmountText, "0");
                return;
            }

            RefreshText(_outputResourceText, _currentFurnaceStation.CurrentOutputResourceId);
            RefreshText(_outputAmountText, _currentFurnaceStation.ReadyOutputAmount.ToString());
        }

        private void RefreshButtons()
        {
            if (_startButton != null)
            {
                _startButton.interactable = _currentFurnaceStation != null && _currentFurnaceStation.IsSmelting == false;
            }

            if (_collectButton != null)
            {
                _collectButton.interactable = _currentFurnaceStation != null && _currentFurnaceStation.ReadyOutputAmount > 0;
            }
        }

        private void RefreshMessage(string message)
        {
            RefreshText(_messageText, message);
        }

        private void RefreshText(TMP_Text targetText, string text)
        {
            if (targetText == null)
            {
                return;
            }

            targetText.text = text;
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

        private void SubscribeFurnace()
        {
            if (_currentFurnaceStation == null)
            {
                return;
            }

            _currentFurnaceStation.OnFurnaceStateChanged += RefreshUI;
        }

        private void UnsubscribeFurnace()
        {
            if (_currentFurnaceStation == null)
            {
                return;
            }

            _currentFurnaceStation.OnFurnaceStateChanged -= RefreshUI;
        }

        private void ConnectButtons()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(TryStartSmelting);
                _startButton.onClick.AddListener(TryStartSmelting);
            }

            if (_collectButton != null)
            {
                _collectButton.onClick.RemoveListener(TryCollectOutput);
                _collectButton.onClick.AddListener(TryCollectOutput);
            }
        }

        private void DisconnectButtons()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(TryStartSmelting);
            }

            if (_collectButton != null)
            {
                _collectButton.onClick.RemoveListener(TryCollectOutput);
            }
        }

        private void SetVisible(bool isVisible)
        {
            if (_rootObject == null)
            {
                gameObject.SetActive(isVisible);
                return;
            }

            _rootObject.SetActive(isVisible);
        }
    }
}
