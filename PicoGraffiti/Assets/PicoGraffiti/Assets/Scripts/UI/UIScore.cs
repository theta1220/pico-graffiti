using System;
using System.Collections.Generic;
using System.Linq;
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

        public Dictionary<ulong, Tuna.Object<UITrack>> UITracks { get; private set; }
        public UnityEvent<Vector2> OnWriteEvent { get; private set; } = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnEraseEvent { get; private set; } = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnWriteOrEraseStartEvent { get; private set; } = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnWriteOrEraseEndEvent { get; private set; } = new UnityEvent<Vector2>();
        public UnityEvent<Vector2> OnMoveEvent { get; private set; } = new UnityEvent<Vector2>();
        public Vector2 PrevPos { get; private set; }
        public List<Color> NoteColors => _noteColors;

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();
        private State _state = State.Write;

        public async UniTask InitializeAsync()
        {
            UITracks = new Dictionary<ulong, Object<UITrack>>();
        }

        public async UniTask CreateTrackAsync(Track track)
        {
            var uiTrack = await Tuna.Object<UITrack>.Create(_tracks);
            UITracks[track.Id] = uiTrack;
            await uiTrack.Instance.InitializeAsync();
            uiTrack.Instance.OnPointerEvent.Subscribe(OnWriteEvent.Invoke).AddTo(_subscribers);
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
            if (_state == State.None)
            {
                if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) _state = State.Move;
                else if (Input.GetMouseButtonDown(0)) _state = State.Write;
                else if (Input.GetMouseButtonDown(1)) _state = State.Erase;
            }
            
            if(_state == State.Write || _state == State.Erase) OnWriteOrEraseStartEvent.Invoke(eventData.position);
            PrevPos = eventData.position;
            OnDrag(eventData);
            if (_state == State.Write) OnWriteEvent.Invoke(PrevPos);
            else if (_state == State.Erase) OnEraseEvent.Invoke(PrevPos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_state == State.Write || _state == State.Erase) OnWriteOrEraseEndEvent.Invoke(eventData.position);
            _state = State.None;
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