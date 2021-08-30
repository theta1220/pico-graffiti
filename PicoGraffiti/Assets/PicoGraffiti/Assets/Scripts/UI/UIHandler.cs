using System;
using Cysharp.Threading.Tasks;
using Tuna;
using UnityEngine;
using UnityEngine.Events;

namespace PicoGraffiti.UI
{
    public class UIHandler : IDisposable
    {
        public Tuna.Object<UIMain> UIMain { get; private set; }
        public Tuna.Object<UIScore> UIScore { get; private set; }

        public UnityEvent<Vector2> OnPointerEvent = new UnityEvent<Vector2>();
        
        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();

        public async UniTask InitializeAsync()
        {
            UIMain = await Tuna.Object<UIMain>.Create();
            UIScore = await Tuna.Object<UIScore>.Create(UIMain.Instance.Root);
            await UIScore.Instance.InitializeAsync();

            UIScore.Instance.OnPointerEvent.Subscribe(OnPointerEvent.Invoke).AddTo(_subscribers);
        }

        public void UpdateFrame()
        {
            UIScore.Instance.UpdateFrame();
        }

        public void Dispose()
        {
            _subscribers.Dispose();
            UIMain.Dispose();
            UIScore.Dispose();
        }
    }
}