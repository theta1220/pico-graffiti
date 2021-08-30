using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Model;
using Tuna;
using UnityEngine;
using UnityEngine.Events;

namespace PicoGraffiti.UI
{
    public class UIScore : TunaBehaviour
    {
        [SerializeField] private Transform _tracks = null;

        public Dictionary<Track, Tuna.Object<UITrack>> UITracks { get; private set; }
        public UnityEvent<Vector2> OnPointerEvent { get; private set; } = new UnityEvent<Vector2>();

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();

        public async UniTask InitializeAsync()
        {
            UITracks = new Dictionary<Track, Object<UITrack>>();
        }

        public async UniTask CreateTrackAsync(Track track)
        {
            var uiTrack = await Tuna.Object<UITrack>.Create(_tracks);
            UITracks[track] = uiTrack;
            await uiTrack.Instance.InitializeAsync();
            uiTrack.Instance.OnPointerEvent.Subscribe(OnPointerEvent.Invoke).AddTo(_subscribers);
        }

        public async void UpdateFrame()
        {
            foreach (var uiTrack in UITracks)
            {
                uiTrack.Value.Instance.UpdateFrame();
            }
        }

        public void OnDestroy()
        {
            _subscribers.Dispose();
            foreach (var uiTrack in UITracks)
            {
                uiTrack.Value.Dispose();
            }
            UITracks.Clear();
        }
    }
}