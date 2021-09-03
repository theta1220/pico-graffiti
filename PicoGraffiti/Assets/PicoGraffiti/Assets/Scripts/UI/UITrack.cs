using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tuna;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PicoGraffiti.UI
{
    public class UITrack : TunaBehaviour
    {
        [SerializeField] private RawImage _image = null;
        [SerializeField] private Color _noteColor;
        
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        private RectTransform _rectTransform = null;
        private Texture2D _texture = null;
        private Color[] _clearColors = null;
        private Color[] _fillClearColors = null;

        public UnityEvent<Vector2> OnPointerEvent { get; private set; } = new UnityEvent<Vector2>();

        public async UniTask InitializeAsync()
        {
            _rectTransform = _image.GetComponent<RectTransform>();
            Width = (int)_rectTransform.rect.width;
            Height = (int) _rectTransform.rect.height;
            _texture = new Texture2D(Width, Height);
            _image.texture = _texture;

            _clearColors = new Color[Height];
            for (var i = 0; i < Height; i++)
            {
                _clearColors[i] = Color.clear;
            }

            var fillLen = _texture.GetPixels().Length;
            _fillClearColors = new Color[fillLen];
            for(var i=0; i<fillLen; i++)
            {
                _fillClearColors[i] = Color.clear;
            }

            Clear();
        }

        public void SetNoteColor(Color color)
        {
            _noteColor = color;
            _image.GetComponent<Outline>().effectColor = color;
        }

        public void Write(int index, double melo, double vol)
        {
            var threshold = (int)(melo * Height);

            if (index < 0 || index + 1 > Width) return;
            var color = _noteColor;
            color.a = (float)vol;
            _texture.SetPixel(index, threshold, color);
        }

        public void Erase(int index)
        {
            if (index < 0 || index + 1 > Width) return;
            _texture.SetPixels(index, 0, 1, Height, _clearColors);
        }

        public void UpdateFrame()
        {
            _texture.Apply();
        }

        public void Clear()
        {
            _texture.SetPixels(_fillClearColors);
        }
    }
}
