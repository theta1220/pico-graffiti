using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Model;
using Tuna;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PicoGraffiti.UI
{
    public class UIScore : TunaBehaviour ,IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private Transform _tracks = null;

        public Dictionary<Track, Tuna.Object<UITrack>> UITracks { get; private set; }
        public UnityEvent<Vector2> OnPointerEvent { get; private set; } = new UnityEvent<Vector2>();

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();
        private Vector2 _prevPos;

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

        public void UpdateFrame()
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
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerEvent.Invoke(eventData.position);
            _prevPos = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            var move = eventData.position - _prevPos;
            var dir = move.normalized;
            var pos = _prevPos;
            for (var i = 0; i < Mathf.CeilToInt(move.magnitude); i++)
            {
                OnPointerEvent.Invoke(pos);
                pos += dir;
            }

            _prevPos = eventData.position;
        }
    }
}