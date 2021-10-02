
using System.Collections.Generic;
using UnityEngine;

namespace Tuna
{
    public static class TunaInstanceCache<T> where T : class, ITunaCached<T>, new()
    {
        public static int CacheCount
        {
            get { return Cache.StackCount; }
        }

        private static class Cache
        {
            private static readonly Stack<T> _stack = new Stack<T>();

            public static int StackCount => _stack.Count;

            public static T Issue()
            {
                if (_stack.Count <= 0)
                {
                    return null;
                }

                var instance = _stack.Pop();
                instance.ReferenceCount = 1;
                return instance;
            }

            public static void Return(T instance)
            {
                if (instance.ReferenceCount <= 0)
                {
                    // error
                    Debug.LogError("Reference Counter Error");
                    return;
                }

                if (--instance.ReferenceCount <= 0)
                {
                    instance.OnReturnCache();

                    if (128 <= _stack.Count)
                    {
                        return;
                    }
                    
                    _stack.Push(instance);
                }
            }
        }

        /// <summary>
        /// 生成
        /// </summary>
        public static T Create()
        {
            var instance = Cache.Issue();
            if (instance != null)
            {
                return instance;
            }

            instance = new T {ReferenceCount = 1, ReturnToCache = Cache.Return};
            return instance;
        }
    }
}