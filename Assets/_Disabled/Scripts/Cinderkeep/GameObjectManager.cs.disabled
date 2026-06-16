using System.Collections.Generic;
using UnityEngine;

namespace OODong.Cinderkeep
{
    // 씬에서 동적으로 생성되는 거의 모든 게임 오브젝트의 instance id를 관리한다.
    // Enemy/Projectile/Placed Item처럼 런타임에 생기는 객체는 생성 직후 RegisterGameObject를 호출해야 한다.
    public sealed class GameObjectManager : MonoBehaviour
    {
        private readonly Dictionary<int, CinderkeepGameObjectIdentity> _identityByInstanceId = new Dictionary<int, CinderkeepGameObjectIdentity>();

        [SerializeField] private int _nextInstanceId = 1;

        public static GameObjectManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public int GenerateInstanceId()
        {
            int instanceId = _nextInstanceId;
            _nextInstanceId++;
            return instanceId;
        }

        public CinderkeepGameObjectIdentity RegisterGameObject(GameObject target)
        {
            // TODO(팀원 작업 요청): 오브젝트 풀링을 붙일 때도 instance id는 여기에서 유지/재발급 정책을 정해 주세요.
            if (target == null)
            {
                return null;
            }

            CinderkeepGameObjectIdentity identity = target.GetComponent<CinderkeepGameObjectIdentity>();
            if (identity == null)
            {
                identity = target.AddComponent<CinderkeepGameObjectIdentity>();
            }

            RegisterIdentity(identity);
            return identity;
        }

        public void RegisterIdentity(CinderkeepGameObjectIdentity identity)
        {
            if (identity == null)
            {
                return;
            }

            if (identity.InstanceId <= 0)
            {
                identity.SetInstanceId(GenerateInstanceId());
            }

            _identityByInstanceId[identity.InstanceId] = identity;
        }

        public void UnregisterIdentity(CinderkeepGameObjectIdentity identity)
        {
            if (identity == null || identity.InstanceId <= 0)
            {
                return;
            }

            if (_identityByInstanceId.TryGetValue(identity.InstanceId, out CinderkeepGameObjectIdentity registered) && registered == identity)
            {
                _identityByInstanceId.Remove(identity.InstanceId);
            }
        }

        public GameObject GetGameObject(int instanceId)
        {
            if (!_identityByInstanceId.TryGetValue(instanceId, out CinderkeepGameObjectIdentity identity) || identity == null)
            {
                return null;
            }

            return identity.gameObject;
        }

        public bool DespawnInstance(int instanceId)
        {
            GameObject target = GetGameObject(instanceId);
            if (target == null)
            {
                return false;
            }

            Destroy(target);
            return true;
        }
    }
}
