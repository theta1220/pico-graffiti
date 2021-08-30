using System;

namespace Tuna
{
    public interface ITunaCached<T> : IDisposable
    {
        int ReferenceCount { get; set; }
        Action<T> ReturnToCache { get; set; }
        void OnReturnCache();
    }
}