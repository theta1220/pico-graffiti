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
        private enum State
        {
            None,
            Write,
            Erase,
            Move,
        }
        
        [SerializeField] private Transform _tracks = null;
        [SerializeField] private List<Color> _noteColors = null;

        public Dictionary<Track, Tuna.Object<UITrack>> UITracks { get; private set; }
        public UnityEvent<Vector2> OnWriteEvent { get; private set; } = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnEraseEvent { get; private set; } = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnWriteEndEvent { get; private set; } = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnMoveEvent { get; private set; } = new UnityEvent<Vector2>();
        public Vector2 PrevPos { get; private set; }
        public List<Color> NoteColors => _noteColors;

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();
        private State _state = State.Write;

        public async UniTask InitializeAsync()
        {
            UITracks = new Dictionary<Track, Object<UITrack>>();
        }

        public async UniTask CreateTrackAsync(Track track)
        {
            var uiTrack = await Tuna.Object<UITrack>.Create(_tracks);
            UITracks[track] = uiTrack;
            await uiTrack.Instance.InitializeAsync();
            uiTrack.Instance.OnPointerEvent.Subscribe(OnWriteEvent.Invoke).AddTo(_subscribers);
        }

        public void UpdateFrame()
        {
            if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) _state = State.Move;
            else if (Input.GetMouseButton(0)) _state = State.Write;
            else if (Input.GetMouseButton(1)) _state = State.Erase;
            else _state = State.None;
            
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
            PrevPos = eventData.position;
            OnDrag(eventData);
            if (_state == State.Write) OnWriteEvent.Invoke(PrevPos);
            else if (_state == State.Erase) OnEraseEvent.Invoke(PrevPos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnWriteEndEvent.Invoke(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            switch (_state)
            {
                case State.Write:
                {
                    var move = eventData.position - PrevPos;
                    var dir = move.normalized;
                    var pos = PrevPos;
                    for (var i = 0; i < Mathf.CeilToInt(move.magnitude) * 10; i++)
                    {
                        OnWriteEvent.Invoke(pos);
                        pos += dir / 10;
                    }
                    Debug.Log("write");
                    break;
                }
                case State.Erase:
                {
                    var move = eventData.position - PrevPos;
                    var dir = move.normalized;
                    var pos = PrevPos;
                    for (var i = 0; i < Mathf.CeilToInt(move.magnitude) * 10; i++)
                    {
                        OnEraseEvent.Invoke(pos);
                        pos += dir / 10;
                    }
                    Debug.Log("erase");
                    break;
                }
                case State.Move:
                {
                    OnMoveEvent.Invoke(eventData.position);
                    break;
                }
            }

            PrevPos = eventData.position;
        }
    }
}