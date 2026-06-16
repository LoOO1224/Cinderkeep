using System;
using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public T LoadSync<T>(string resourcePath)
            where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            return Resources.Load<T>(resourcePath);
        }

        public ResourceRequest LoadAsync<T>(string resourcePath, Action<T> completed)
            where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                completed?.Invoke(null);
                return null;
            }

            ResourceRequest request = Resources.LoadAsync<T>(resourcePath);
            request.completed += operation => completed?.Invoke(((ResourceRequest)operation).asset as T);
            return request;
        }
    }
}
