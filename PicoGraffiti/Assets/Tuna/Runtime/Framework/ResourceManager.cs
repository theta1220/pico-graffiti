using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tuna.Framework
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        private class ResourceInfo
        {
            public Type Type { get; private set; }
            public GameObject LoadedResource { get; private set; }

            public ResourceInfo(Type type, GameObject loadedResource)
            {
                Type = type;
                LoadedResource = loadedResource;
            }
        }
        
        private List<Type> _loadedResourceTypes = null;
        private List<ResourceInfo> _resourcePool = null;

        public void Initialize()
        {
            _loadedResourceTypes = new List<Type>();
            _resourcePool = new List<ResourceInfo>();
        }
        
        public async UniTask<GameObject> LoadAsync<T>() where T : MonoBehaviour
        {
            _loadedResourceTypes.Add(typeof(T));

            // 初ロード
            if (_loadedResourceTypes.Count(t => t == typeof(T)) == 1)
            {
                var res = await LoadInternalAsync<T>();
                var info = new ResourceInfo(typeof(T), res);
                _resourcePool.Add(info);
                return info.LoadedResource;
            }
            // 既にロードされてる
            else
            {
                var info = _resourcePool.Find(o => o.Type == typeof(T));
                return info.LoadedResource;
            }
        }

        public void Unload<T>() where T : MonoBehaviour
        {
            _loadedResourceTypes.Remove(typeof(T));
            if (!_loadedResourceTypes.Contains(typeof(T)))
            {
                var res = _resourcePool.Find(info => info.Type == typeof(T));
                // UnloadInternal(res);
            }
        }

        private async UniTask<GameObject> LoadInternalAsync<T>() where T : MonoBehaviour
        {
            var resourcePath = $"{typeof(T).FullName}".Replace('.', '/');
            var res = await Resources.LoadAsync<GameObject>(resourcePath) as GameObject;
            if (res == null)
            {
                Debug.LogError($"{resourcePath}のロードに失敗しました");
                return null;
            }
            Debug.Log($"{resourcePath}をロードしました");
            return res;
        }

        private void UnloadInternal(ResourceInfo info)
        {
            var resourcePath = $"{info.Type.FullName}".Replace('.', '/');
            try
            {
                Resources.UnloadAsset(info.LoadedResource);
                Debug.Log($"{resourcePath}をアンロードしました");
            }
            catch
            {
                Debug.Log($"{resourcePath}をアンロードしようとしましたが失敗しました");
            }
        }
    }
}