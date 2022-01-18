using System.Xml.XPath;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using Tuna;
using UnityEngine;
using UnityEngine.UI;

namespace PicoGraffiti.UI
{
    public class UILines : TunaBehaviour
    {
        private const int SCALE = 1;
        [SerializeField] private RawImage _image;
        [SerializeField] private Color _lineColorW;
        [SerializeField] private Color _lineColorH;
        [SerializeField] private Color _lineColorS;
        [SerializeField] private Color _lineColor32;
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
            await UniTask.SwitchToMainThread();
            await UniTask.Yield();
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
            var width = Track.NOTE_GRID_SIZE * 32;
            var wSplit = Track.NOTE_GRID_SIZE / 4;
            var hSplit = Height/ _num;
            for (var y = 0; y < Height; y+=hSplit)
            {
                if (isSharp(count))
                {
                    for (var i = 0; i < Width; i++)
                    {
                        _textureBuffer[i + y * Width].r = _lineColor32.r;
                        _textureBuffer[i + y * Width].g = _lineColor32.g;
                        _textureBuffer[i + y * Width].b = _lineColor32.b;
                        _textureBuffer[i + y * Width].a = _lineColor32.a;
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

            var offset = _offset * (UIScore.SCALE / (float)SCALE);

            // たてせん
            count = 0;
            for (var x = 0.0f; x < width; x+=wSplit)
            {
                var pos = x - offset - wSplit * 16;
                while (pos < 0)
                {
                    pos += width;
                }
                if (count % 32 == 0)
                {
                    for (var i = 0; i < Height; i++)
                    {
                        Write((int)pos - 1, i, _lineColor32);
                        Write((int)pos + 0, i, _lineColor32);
                        Write((int)pos + 1, i, _lineColor32);
                    }
                }
                else if (count % 4 == 0)
                {
                    for (var i = 0; i < Height; i++)
                    {
                        Write((int)pos + 1, i, _lineColorS);
                        Write((int)pos + 0, i, _lineColorS);
                    }
                }
                else
                {
                    for (var i = 0; i < Height; i++)
                    {
                        if(i % 2 == 0) continue;
                        Write((int)pos + 0, i, _lineColorW);
                    }
                }

                count++;
            }
        }

        private void Write(int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            
            _textureBuffer[x + y * Width].r = color.r;
            _textureBuffer[x + y * Width].g = color.g;
            _textureBuffer[x + y * Width].b = color.b;
            _textureBuffer[x + y * Width].a = color.a;
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
                Write();
                _texture.SetPixels(_textureBuffer);
                _texture.Apply();
                _moved = false;
            }
        }

        public void OnMove(float offset)
        {
            _moved = true;
            _offset = offset;
        }
    }
}