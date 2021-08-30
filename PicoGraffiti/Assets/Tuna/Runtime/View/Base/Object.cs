using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tuna
{
    public class Object<T> : IDisposable where T : TunaBehaviour
    {
        public T Instance { get; private set; }

        public static async UniTask<Tuna.Object<T>> Create(Transform parent = null)
        {
            var tuna = new Tuna.Object<T>();
            var res = await Tuna.Framework.ResourceManager.Instance.LoadAsync<T>();
            tuna.Instance = GameObject.Instantiate(res, parent).GetComponent<T>();
            return tuna;
        }

        public void Dispose()
        {
            Tuna.Framework.ResourceManager.Instance.Unload<T>();
            GameObject.Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}