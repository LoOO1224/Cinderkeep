using UnityEngine;
using UnityEngine.UI;

// 적의 감지, 타깃 판단, 이동, 공격, 보스 클리어 연결 중 한 역할을 담당합니다.
// AI 판단과 실제 행동 컴포넌트를 분리해 적 패턴이 늘어도 유지보수 가능하게 합니다.
// 몬스터 머리 위 체력 UI를 표시하는 컴포넌트입니다.
// 체력 계산과 사망 처리는 EnemyStatus가 담당하고, 이 스크립트는 화면 표시만 담당합니다.
public sealed class EnemyHud : MonoBehaviour
{
    [SerializeField] private GameObject _hudRoot;
    [SerializeField] private Image _hpFillImage;
    [SerializeField] private Text _hpText;
    [SerializeField] private Camera _targetCamera;

    private void Awake()
    {
        InitializeHudRoot();
    }

    private void LateUpdate()
    {
        RotateToCamera();
    }

    public void SetTargetCamera(Camera targetCamera)
    {
        _targetCamera = targetCamera;
    }

    public void RefreshHealth(float currentHealth, float maxHealth)
    {
        RefreshHpBar(currentHealth, maxHealth);
        RefreshHpText(currentHealth, maxHealth);
    }

    private void InitializeHudRoot()
    {
        if (_hudRoot == null)
        {
            _hudRoot = gameObject;
        }
    }

    private void RefreshHpBar(float currentHealth, float maxHealth)
    {
        if (_hpFillImage == null)
        {
            return;
        }

        if (maxHealth <= 0f)
        {
            _hpFillImage.fillAmount = 0f;
            return;
        }

        _hpFillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
    }

    private void RefreshHpText(float currentHealth, float maxHealth)
    {
        if (_hpText == null)
        {
            return;
        }

        int currentHealthText = Mathf.RoundToInt(currentHealth);
        int maxHealthText = Mathf.RoundToInt(maxHealth);
        _hpText.text = currentHealthText + " / " + maxHealthText;
    }

    private void RotateToCamera()
    {
        if (_targetCamera == null)
        {
            return;
        }

        transform.rotation = _targetCamera.transform.rotation;
    }
}
