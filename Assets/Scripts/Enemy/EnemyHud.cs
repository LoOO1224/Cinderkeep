using UnityEngine;
using UnityEngine.UI;

public class EnemyHud : MonoBehaviour
{
    [Header("Enemy Hud")]
    [SerializeField] private GameObject Layout_HudRoot;
    [SerializeField] private Image Image_HpFill; // 체력바에서 채워지는 이미지
    [SerializeField] private Text Text_Hp;

    private Camera _mainCamera;

    private void Awake()
    {
        ResolveComponents();
    }

    private void LateUpdate()
    {
        LookAtCamera();
    }

    private void ResolveComponents()
    {
        _mainCamera = Camera.main;

        if (Layout_HudRoot == null)
        {
            Layout_HudRoot = gameObject;
        }
    }

    public void RefreshHp(int curHp, int maxHp)
    {
        RefreshHpBar(curHp, maxHp);
        RefreshHpText(curHp, maxHp);
    }

    private void RefreshHpBar(int curHp, int maxHp)
    {
        if(Image_HpFill == null)
        {
            return;
        }

        if(maxHp <= 0)
        {
            Image_HpFill.fillAmount = 0f;
            return;
        }

        float hpRatio = (float)curHp / maxHp;
        Image_HpFill.fillAmount = Mathf.Clamp01(hpRatio);

    }

    private void RefreshHpText(int curHp, int maxHp)
    {
        if (Text_Hp == null)
        {
            return;
        }

        Text_Hp.text = $"{curHp} / {maxHp}";
    }

    // World Space Canvas가 카메라 방향을 바라보게 한다.
    private void LookAtCamera()
    {
        if (_mainCamera == null)
        {
            return;
        }

        transform.rotation = _mainCamera.transform.rotation;
    }

}
