using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Model;
using Tuna;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        public class TextureBuffer
        {
            public Color[] Buffer;
        }

        public const int SCALE = 4;
        
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
        public int Width { get; private set; }
        public int Height { get; private set; }

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();
        private State _state = State.Write;
        private RectTransform _rectTransform;
        private RawImage _image;
        private Texture2D _texture;
        private bool _isUpdateTexture = false;
        private TextureBuffer _textureBuffer = null;

        public async UniTask InitializeAsync(float parentHeight)
        {
            UITracks = new Dictionary<ulong, Object<UITrack>>();
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<RawImage>();
            Width = (int) _rectTransform.rect.width / SCALE;
            Height = (int) parentHeight / SCALE;
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, Height * SCALE);
            _texture = new Texture2D(Width, Height);
            _texture.filterMode = FilterMode.Point;
            _image.texture = _texture;
            _textureBuffer = new TextureBuffer();
            _textureBuffer.Buffer = new Color[Width * Height];
            UITracks.Clear();
            UpdateTexture();
        }

        public async UniTask CreateTrackAsync(Track track)
        {
            var uiTrack = await Tuna.Object<UITrack>.Create(_tracks);
            UITracks[track.Id] = uiTrack;
            await uiTrack.Instance.InitializeAsync();
            uiTrack.Instance.InitializeTextureBuffer(_textureBuffer, Width, Height);
            uiTrack.Instance.OnPointerEvent.Subscribe(OnWriteEvent.Invoke).AddTo(_subscribers);
        }

        public void UpdateFrame()
        {
            foreach (var uiTrack in UITracks)
            {
                uiTrack.Value.Instance.UpdateFrame();
            }

            if (_isUpdateTexture)
            {
                _texture.SetPixels(_textureBuffer.Buffer);
                _texture.Apply();
                _isUpdateTexture = false;
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
            
            if(_state == State.Write || _state == State.Erase) OnWriteOrEraseStartEvent.Invoke(GetTouchPosition(eventData));
            PrevPos = GetTouchPosition(eventData);
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
            var touchPos = GetTouchPosition(eventData);
            switch (_state)
            {
                case State.Write:
                {
                    var move = touchPos - PrevPos;
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
                    var move = touchPos - PrevPos;
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
                    OnMoveEvent.Invoke(touchPos);
                    break;
                }
            }

            PrevPos = touchPos;
        }

        public Vector2 GetTouchPosition(PointerEventData eventData)
        {
            var touchPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, Camera.main, out touchPos);
            return touchPos / SCALE;
        }

        public void UpdateTexture()
        {
            _isUpdateTexture = true;
        }

        public void Clear()
        {
            var len = _textureBuffer.Buffer.Length;
            for (var i = 0; i < len; i++)
            {
                _textureBuffer.Buffer[i].a = 0;
            }
        }
    }
}