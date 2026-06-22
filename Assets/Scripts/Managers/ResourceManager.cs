using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Cinderkeep.Gameplay
{
    // Resources 폴더 안의 Unity 에셋을 불러오는 매니저입니다.
    // 주의: Wood, Stone 같은 게임 플레이 자원 수량은 PlayerModel과 ResourceNode가 담당합니다.
    // 이 클래스는 프리팹, 머티리얼, 사운드, UI 프리팹 같은 에셋 로딩만 담당합니다.
    public sealed class ResourceManager : MonoBehaviour, IGameInitializable
    {
        [Header("Resources Root")]
        [SerializeField] private string _resourceRootPath = "";

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

            _isInitialized = true;
        }

        public TAsset LoadAsset<TAsset>(string resourcePath)
            where TAsset : UnityEngine.Object
        {
            Initialize();

            string fullPath = GetFullResourcePath(resourcePath);
            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarning("ResourceManager: resource path is empty.");
                return null;
            }

            TAsset asset = Resources.Load<TAsset>(fullPath);
            if (asset == null)
            {
                Debug.LogWarning("ResourceManager: asset was not found at Resources/" + fullPath);
            }

            return asset;
        }

        public GameObject LoadPrefab(string resourcePath)
        {
            return LoadAsset<GameObject>(resourcePath);
        }

        public AsyncOperationHandle<TAsset> LoadAddressableAssetAsync<TAsset>(string address, Action<TAsset> onLoaded = null)
            where TAsset : UnityEngine.Object
        {
            Initialize();

            AsyncOperationHandle<TAsset> handle = Addressables.LoadAssetAsync<TAsset>(address);
            handle.Completed += completedHandle =>
            {
                if (completedHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogWarning("ResourceManager: addressable asset was not found: " + address);
                    return;
                }

                if (onLoaded == null)
                {
                    return;
                }

                onLoaded.Invoke(completedHandle.Result);
            };

            return handle;
        }

        public AsyncOperationHandle<GameObject> InstantiateAddressablePrefabAsync(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null,
            Action<GameObject> onCreated = null)
        {
            Initialize();

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, position, rotation, parent);
            handle.Completed += completedHandle =>
            {
                if (completedHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogWarning("ResourceManager: addressable prefab was not instantiated: " + address);
                    return;
                }

                if (onCreated == null)
                {
                    return;
                }

                onCreated.Invoke(completedHandle.Result);
            };

            return handle;
        }

        public bool HasAsset<TAsset>(string resourcePath)
            where TAsset : UnityEngine.Object
        {
            Initialize();

            string fullPath = GetFullResourcePath(resourcePath);
            if (string.IsNullOrEmpty(fullPath))
            {
                return false;
            }

            TAsset asset = Resources.Load<TAsset>(fullPath);
            return asset != null;
        }

        public string GetFullResourcePath(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return "";
            }

            if (string.IsNullOrEmpty(_resourceRootPath))
            {
                return resourcePath;
            }

            return _resourceRootPath + "/" + resourcePath;
        }
    }
}
