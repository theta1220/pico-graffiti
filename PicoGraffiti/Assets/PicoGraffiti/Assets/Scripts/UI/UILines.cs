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
        private Color[] _textureBuffer = null;

        public int Width = 0;
        public int Height = 0;

        private int _offset = 0;
        private bool _moved = false;

        public async UniTask InitializeAsync()
        {
            var rect = _image.rectTransform;
            Width = (int)rect.rect.width / UIScore.SCALE;
            Height = (int)rect.rect.height / UIScore.SCALE;
            
            _texture = new Texture2D(Width, Height);
            _texture.wrapMode = TextureWrapMode.Repeat;
            _texture.filterMode = FilterMode.Point;
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

            var len = Width * Height;
            _textureBuffer = new Color[len];
            for (var i = 0; i < len; i++)
            {
                _textureBuffer[i] = Color.clear;
            }

            Write();
            _moved = true;
        }

        public void Write()
        {
            var len = Width * Height;
            for (var i = 0; i < len; i++)
            {
                _textureBuffer[i].a = 0;
            }
            
            var count = 0;
            var wSplit = Width / 32;
            var hSplit = Height/ Wave.MELO_NUM;
            var offset = _offset % wSplit;
            for (var y = 0; y < Height; y+=hSplit)
            {
                if (isSharp(count))
                {
                    for (var i = 0; i < Width; i++)
                    {
                        _textureBuffer[i + (y + 1) * Width].r = _lineColorS.r;
                        _textureBuffer[i + (y + 1) * Width].g = _lineColorS.g;
                        _textureBuffer[i + (y + 1) * Width].b = _lineColorS.b;
                        _textureBuffer[i + (y + 1) * Width].a = _lineColorS.a;
                    }
                    // _texture.SetPixels(0, y + 1, Width, 1, _lineColorsS);
                }
                else
                {
                    for (var i = 0; i < Width; i++)
                    {
                        _textureBuffer[i + y * Width].r = _lineColorW.r;
                        _textureBuffer[i + y * Width].g = _lineColorW.g;
                        _textureBuffer[i + y * Width].b = _lineColorW.b;
                        _textureBuffer[i + y * Width].a = _lineColorW.a;
                    }
                    // _texture.SetPixels(0, y, Width, 1, _lineColorsW);
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
                for (var i = 0; i < Height; i++)
                {
                    _textureBuffer[pos - offset + i * Width].r = _lineColorH.r;
                    _textureBuffer[pos - offset + i * Width].g = _lineColorH.g;
                    _textureBuffer[pos - offset + i * Width].b = _lineColorH.b;
                    _textureBuffer[pos - offset + i * Width].a = _lineColorH.a;
                }
                // _texture.SetPixels(pos - offset, 0, 1, Height, _lineColorsH);
                if (count % 2 == 0 && pos - offset + 1 < Width)
                {
                    for (var i = 0; i < Height; i++)
                    {
                        _textureBuffer[(pos - offset + 1) + i * Width].r = _lineColorH.r;
                        _textureBuffer[(pos - offset + 1) + i * Width].g = _lineColorH.g;
                        _textureBuffer[(pos - offset + 1) + i * Width].b = _lineColorH.b;
                        _textureBuffer[(pos - offset + 1) + i * Width].a = _lineColorH.a;
                    }
                    // _texture.SetPixels(pos - offset + 1, 0, 1, Height, _lineColorsH);
                }

                count++;
            }
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
                _texture.SetPixels(_textureBuffer);
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