using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepGameObjectIdentity : MonoBehaviour
    {
        [SerializeField] private int _instanceId;

        public int InstanceId => _instanceId;

        private void Awake()
        {
            Register();
        }

        private void OnDestroy()
        {
            GameObjectManager.Instance?.UnregisterIdentity(this);
        }

        public void SetInstanceId(int instanceId)
        {
            if (_instanceId > 0)
            {
                return;
            }

            _instanceId = instanceId;
        }

        public void Register()
        {
            GameObjectManager.Instance?.RegisterIdentity(this);
        }
    }
}
