using System.Xml.XPath;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Framework;
using Tuna;
using UnityEngine;
using UnityEngine.UI;

namespace PicoGraffiti.UI
{
    public class UILines : TunaBehaviour
    {
        private const int SCALE = 4;
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

        private float _offset = 0;
        private bool _moved = false;

        private int _num = 0;

        public async UniTask InitializeAsync(float height, int num)
        {
            _num = num;
            
            var rect = _image.rectTransform;
            Width = (int)rect.rect.width / SCALE;
            Height = (int)height / SCALE;
            _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, height);
            
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
            
            // よこせん
            var count = 0;
            var xSplit = 32.0f;
            var wSplit = Width / xSplit;
            var hSplit = Height/ _num;
            for (var y = 0; y < Height; y+=hSplit)
            {
                if (isSharp(count))
                {
                    for (var i = 0; i < Width; i++)
                    {
                        _textureBuffer[i + y * Width].r = _lineColorS.r;
                        _textureBuffer[i + y * Width].g = _lineColorS.g;
                        _textureBuffer[i + y * Width].b = _lineColorS.b;
                        _textureBuffer[i + y * Width].a = _lineColorS.a;
                    }
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
                }

                count++;
            }

            // たてせん
            count = 0;
            var offset = (int)_offset * (UIScore.SCALE / SCALE);
            for (var x = 0f; x < Width; x+=wSplit)
            {
                var pos = (int)x - offset;
                while (pos < 0)
                {
                    pos += Width;
                }
                for (var i = 0; i < Height; i++)
                {
                    _textureBuffer[pos + i * Width].r = _lineColorH.r;
                    _textureBuffer[pos + i * Width].g = _lineColorH.g;
                    _textureBuffer[pos + i * Width].b = _lineColorH.b;
                    _textureBuffer[pos + i * Width].a = _lineColorH.a;
                }
                if (x / wSplit % 4 == 0)
                {
                    pos = (int)x + 1 - offset;
                    while (pos < 0)
                    {
                        pos += Width;
                    }
                    for (var i = 0; i < Height; i++)
                    {
                        _textureBuffer[pos + i * Width].r = _lineColorH.r;
                        _textureBuffer[pos + i * Width].g = _lineColorH.g;
                        _textureBuffer[pos + i * Width].b = _lineColorH.b;
                        _textureBuffer[pos + i * Width].a = _lineColorH.a;
                    }
                }
                if (x / wSplit % 16 == 0)
                {
                    pos = (int)x - 1 - offset;
                    while (pos < 0)
                    {
                        pos += Width;
                    }
                    for (var i = 0; i < Height; i++)
                    {
                        _textureBuffer[pos + i * Width].r = _lineColorS.r;
                        _textureBuffer[pos + i * Width].g = _lineColorS.g;
                        _textureBuffer[pos + i * Width].b = _lineColorS.b;
                        _textureBuffer[pos + i * Width].a = _lineColorS.a;
                    }
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

        public void OnMove(float offset)
        {
            _moved = true;
            _offset = offset;
            Write();
        }
    }
}