using System;
using UnityEngine;

namespace OODong.Cinderkeep
{
    // FlameHeart actor 컴포넌트.
    // 체력 Model은 GameManager가 소유하고, 이 컴포넌트는 피해/수리/시각 피드백만 담당한다.
    public sealed class CinderkeepFlameHeart : MonoBehaviour
    {
        [SerializeField] private int _maxHealth = 180;
        [SerializeField] private Renderer Renderer_Core;

        private CinderkeepFlameHeartModel _model = new CinderkeepFlameHeartModel();

        public CinderkeepFlameHeartModel Model => _model;
        public event Action Damaged;
        public event Action Depleted;

        private void Awake()
        {
            if (Renderer_Core == null)
            {
                Renderer_Core = GetComponentInChildren<Renderer>();
            }

            _model.SetMaxHealth(_maxHealth);
            _model.ResetHealth();
            RefreshView();
        }

        public void ResetHeart()
        {
            _model.SetMaxHealth(_maxHealth);
            _model.ResetHealth();
            RefreshView();
        }

        public void SetModel(CinderkeepFlameHeartModel model)
        {
            if (model == null)
            {
                return;
            }

            _model = model;
            _model.SetMaxHealth(_maxHealth);
            RefreshView();
        }

        public void TakeDamage(int damage)
        {
            // FlameHeart가 0이 되면 GameManager에 패배를 보고한다.
            // TODO(팀원 작업 요청): 피격 사운드/흔들림/파티클은 여기서 직접 만들지 말고 View 컴포넌트로 분리해 주세요.
            if (_model.IsDepleted)
            {
                return;
            }

            _model.TakeDamage(damage);
            Damaged?.Invoke();
            RefreshView();

            if (_model.IsDepleted)
            {
                Depleted?.Invoke();
                GameManager.Instance?.LoseRun("FlameHeart destroyed. Cinderkeep fell.");
            }
        }

        public void Repair(int amount)
        {
            _model.Repair(amount);
            RefreshView();
        }

        public void SetCoreRenderer(Renderer coreRenderer)
        {
            Renderer_Core = coreRenderer;
            RefreshView();
        }

        private void RefreshView()
        {
            if (Renderer_Core == null)
            {
                return;
            }

            Color healthy = new Color(1f, 0.42f, 0.12f, 1f);
            Color critical = new Color(0.2f, 0.03f, 0.02f, 1f);
            Material material = Application.isPlaying ? Renderer_Core.material : Renderer_Core.sharedMaterial;
            if (material != null)
            {
                material.color = Color.Lerp(critical, healthy, _model.HealthRate);
            }
        }
    }
}
