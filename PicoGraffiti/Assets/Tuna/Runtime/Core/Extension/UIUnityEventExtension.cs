using System;
using UnityEngine.Events;

namespace Tuna
{
    public static class UIUnityEventExtension
    {
        public static IDisposable Subscribe(this UnityEvent e, UnityAction handler)
        {
            e.AddListener(handler);
            return TunaDisposable.Create(() => e.RemoveListener(handler));
        }
        
        public static IDisposable Subscribe<T0>(this UnityEvent<T0> e, UnityAction<T0> handler)
        {
            e.AddListener(handler);
            return TunaDisposable.Create(() => e.RemoveListener(handler));
        }
        
        public static IDisposable Subscribe<T0, T1>(this UnityEvent<T0, T1> e, UnityAction<T0, T1> handler)
        {
            e.AddListener(handler);
            return TunaDisposable.Create(() => e.RemoveListener(handler));
        }
        
        public static IDisposable Subscribe<T0, T1, T2>(this UnityEvent<T0, T1, T2> e, UnityAction<T0, T1, T2> handler)
        {
            e.AddListener(handler);
            return TunaDisposable.Create(() => e.RemoveListener(handler));
        }

        public static IDisposable Subscribe(this UnityEvent e, UnityEvent handler)
        {
            return e.Subscribe(handler.Invoke);
        }

        public static IDisposable Subscribe<T0>(this UnityEvent<T0> e, UnityEvent<T0> handler)
        {
            return e.Subscribe(handler.Invoke);
        }

        public static IDisposable Subscribe<T0, T1>(this UnityEvent<T0, T1> e, UnityEvent<T0, T1> handler)
        {
            return e.Subscribe(handler.Invoke);
        }

        public static IDisposable Subscribe<T0, T1, T2>(this UnityEvent<T0, T1, T2> e, UnityEvent<T0, T1, T2> handler)
        {
            return e.Subscribe(handler.Invoke);
        }
    }
}