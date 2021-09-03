using Cysharp.Threading.Tasks;
using PicoGraffiti.Framework;
using Tuna;
using UnityEngine;
using UnityEngine.UI;

namespace PicoGraffiti.UI
{
    public class UILines : TunaBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private Color _lineColorW;
        [SerializeField] private Color _lineColorH;
        [SerializeField] private Color _lineColorS;
        private Texture2D _texture = null;
        private Color[] _lineColorsW = null;
        private Color[] _lineColorsH = null;
        private Color[] _lineColorsS = null;
        
        public int Width { get; private set; }
        public int Height { get; private set; }

        private int _offset = 0;
        private bool _moved = false;

        public async UniTask InitializeAsync()
        {
            var rect = _image.rectTransform;
            Width = (int)rect.rect.width;
            Height = (int)rect.rect.height;
            
            _texture = new Texture2D(Width, Height);
            _texture.wrapMode = TextureWrapMode.Repeat;
            _image.texture = _texture;

            _lineColorsW = new Color[Width];
            _lineColorsH = new Color[Height];
            _lineColorsS = new Color[Width];
            for (var i = 0; i < Width; i++)
            {
                _lineColorsW[i] = _lineColorW;
                _lineColorsS[i] = _lineColorS;
            }
            for (var i = 0; i < Height; i++)
            {
                _lineColorsH[i] = _lineColorH;
            }

            Write();
        }

        public void Write()
        {
            var all = new Color[Width * Height];
            for (var i = 0; i < Width * Height; i++)
            {
                all[i] = Color.clear;
            }
            _texture.SetPixels(0, 0, Width, Height, all);
            
            var count = 0;
            var wSplit = Width / 32;
            var hSplit = Height/ Wave.MELO_NUM;
            var offset = _offset % wSplit;
            for (var y = 0; y < Height; y+=hSplit)
            {
                if (isSharp(count))
                {
                    _texture.SetPixels(0, y + 1, Width, 1, _lineColorsS);
                }
                else
                {
                    _texture.SetPixels(0, y, Width, 1, _lineColorsW);
                }

                count++;
            }

            count = 0;
            for (var x = 0; x < Width; x+=wSplit)
            {
                var pos = x;
                if (x - offset < 0)
                {
                    pos += Width;
                }
                _texture.SetPixels(pos - offset, 0, 1, Height, _lineColorsH);
                if (count % 2 == 0)
                {
                    _texture.SetPixels(pos - offset + 1, 0, 1, Height, _lineColorsH);
                }

                count++;
            }
            _texture.Apply();
        }

        private bool isSharp(int melo)
        {
            var sharp = new[] {false, true, false, true, false, false, true, false, true, false, true, false};
            return sharp[melo % sharp.Length];
        }

        public void UpdateFrame()
        {
            if (_moved)
            {
                _texture.Apply();
                _moved = false;
            }
        }

        public void OnMove(int offset)
        {
            _moved = true;
            _offset = offset;
            Write();
        }
    }
}