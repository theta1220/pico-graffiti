using System;
using System.Collections.Generic;

namespace Tuna
{
    public static class TunaDisposableHelper
    {
        public static void AddTo(this IDisposable disposable, TunaCompositeDisposable container)
        {
            container?.Add(disposable);
        }
    }
    
    public sealed class TunaDisposable : ITunaCached<TunaDisposable>
    {
        public static readonly IDisposable Empty = new TunaDisposable();
        
        public int ReferenceCount { get; set; }
        public Action<TunaDisposable> ReturnToCache { get; set; }
        private Action _dispose;
        
        public static int CurrentTotalReference { get; private set; }

        public static TunaDisposable Create(Action dispose)
        {
            CurrentTotalReference++;
            var disposable = TunaInstanceCache<TunaDisposable>.Create();
            disposable._dispose = dispose;
            return disposable;
        }

        public void Dispose()
        {
            ReturnToCache(this);
            CurrentTotalReference--;
        }

        public void OnReturnCache()
        {
            if (_dispose != null)
            {
                _dispose();
                _dispose = null;
            }
        }
    }

    public sealed class TunaCompositeDisposable : IDisposable
    {
        private List<IDisposable> _subscriptions;
        public bool _isDisposed;
        
        public static int CurrentTotalReference { get; private set; }

        public static TunaCompositeDisposable Create()
        {
            CurrentTotalReference++;
            return new TunaCompositeDisposable();
        }

        private TunaCompositeDisposable()
        {
            _subscriptions = new List<IDisposable>();
        }

        public bool IsDisposed => _isDisposed;

        public void Add(IDisposable disposable)
        {
            if (_isDisposed)
            {
                disposable.Dispose();
            }
            else
            {
                _subscriptions.Add(disposable);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }
            _subscriptions.Clear();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                Clear();
                _subscriptions = null;
                CurrentTotalReference--;
            }
        }
    }
}