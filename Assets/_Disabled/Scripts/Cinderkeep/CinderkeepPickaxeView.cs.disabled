using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepPickaxeView : MonoBehaviour
    {
        [SerializeField] private Transform Transform_ViewRoot;
        [SerializeField] private float _swingDuration = 0.22f;
        [SerializeField] private Vector3 _swingEulerOffset = new Vector3(38f, -18f, -14f);
        [SerializeField] private Vector3 _swingPositionOffset = new Vector3(0.08f, -0.07f, 0.06f);

        private Vector3 _defaultLocalPosition;
        private Quaternion _defaultLocalRotation;
        private float _swingTimer;

        private void Awake()
        {
            if (Transform_ViewRoot == null)
            {
                Transform_ViewRoot = transform;
            }

            _defaultLocalPosition = Transform_ViewRoot.localPosition;
            _defaultLocalRotation = Transform_ViewRoot.localRotation;
        }

        private void Update()
        {
            if (_swingTimer <= 0f)
            {
                return;
            }

            _swingTimer -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(_swingTimer / _swingDuration);
            float arc = Mathf.Sin(progress * Mathf.PI);
            Transform_ViewRoot.localPosition = _defaultLocalPosition + (_swingPositionOffset * arc);
            Transform_ViewRoot.localRotation = _defaultLocalRotation * Quaternion.Euler(_swingEulerOffset * arc);

            if (_swingTimer <= 0f)
            {
                Transform_ViewRoot.localPosition = _defaultLocalPosition;
                Transform_ViewRoot.localRotation = _defaultLocalRotation;
            }
        }

        public void PlaySwing()
        {
            _swingTimer = _swingDuration;
        }
    }
}
