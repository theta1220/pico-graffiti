using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tuna;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PicoGraffiti.UI
{
    public class UITrack : TunaBehaviour ,IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RawImage _image = null;
        [SerializeField] private Color _noteColor;
        
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        private RectTransform _rectTransform = null;
        private Texture2D _texture = null;
        private int _offsetX = 0;
        private int _offsetY = 0;
        private Vector2 _prevPos;

        public UnityEvent<Vector2> OnPointerEvent { get; private set; } = new UnityEvent<Vector2>();

        public async UniTask InitializeAsync()
        {
            _rectTransform = _image.GetComponent<RectTransform>();
            Width = (int)_rectTransform.rect.width;
            Height = (int) _rectTransform.rect.height;
            _texture = new Texture2D(Width, Height);
            _image.texture = _texture;
            
            Clear();
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

        public void Write(int index, double melo)
        {
            var threshold = (int)(melo * Height);
            
            for (var y = 0; y < Height; y++)
            {
                if (y > threshold)
                {
                    _texture.SetPixel(index, y, Color.clear);
                }
                else
                {
                    _texture.SetPixel(index, y, _noteColor);
                }
            }
        }

        public void UpdateFrame()
        {
            _texture.Apply();
        }

        public void Clear()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _texture.SetPixel(x, y, Color.clear);
                }
            }
            _texture.Apply();
        }
    }
}
