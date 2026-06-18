using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;

namespace Cinderkeep.Gameplay
{
    // 게임 중 동적으로 만들어지는 오브젝트를 등록하고 제거하는 매니저입니다.
    // 현재는 몬스터, 자원, 고정 건축 지점 같은 게임 루프 오브젝트를 관리할 기반입니다.
    // 생성된 오브젝트는 instanceId를 받아서, 이후 수정/제거 요청을 같은 id로 처리할 수 있습니다.
    public sealed class GameObjectManager : MonoBehaviour, IGameInitializable
    {
        [FormerlySerializedAs("_objectRoot")]
        [SerializeField] private Transform Transform_ObjectRoot;

        private readonly Dictionary<int, GameObjectIdentity> _createdObjectById = new Dictionary<int, GameObjectIdentity>();
        private int _objectInstanceKeyGenerator;
        private bool _isInitialized;

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _createdObjectById.Clear();
            _objectInstanceKeyGenerator = 0;
            _isInitialized = true;
        }

        public GameObject CreateGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            Initialize();

            if (prefab == null)
            {
                Debug.LogWarning("GameObjectManager: prefab is empty.");
                return null;
            }

            GameObject createdObject = Instantiate(prefab, position, rotation, Transform_ObjectRoot);
            RegisterGameObject(createdObject);
            return createdObject;
        }

        public void RegisterGameObject(GameObject targetObject)
        {
            Initialize();

            if (targetObject == null)
            {
                return;
            }

            GameObjectIdentity identity = targetObject.GetComponent<GameObjectIdentity>();
            if (identity == null)
            {
                identity = targetObject.AddComponent<GameObjectIdentity>();
            }

            int instanceId = GameUtil.GenerateNextInstanceId(_objectInstanceKeyGenerator);
            _objectInstanceKeyGenerator = instanceId;
            identity.Initialize(instanceId);
            _createdObjectById.Add(instanceId, identity);
        }

        public GameObject GetGameObjectCanBeNull(int instanceId)
        {
            if (!_createdObjectById.ContainsKey(instanceId))
            {
                return null;
            }

            GameObjectIdentity identity = _createdObjectById[instanceId];
            if (identity == null)
            {
                _createdObjectById.Remove(instanceId);
                return null;
            }

            return identity.gameObject;
        }

        public void RequestDestroyGameObject(int instanceId)
        {
            GameObject targetObject = GetGameObjectCanBeNull(instanceId);
            if (targetObject == null)
            {
                return;
            }

            _createdObjectById.Remove(instanceId);
            Destroy(targetObject);
        }
    }
}
