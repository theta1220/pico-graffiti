using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace PixelScreen
{
    // [ExecuteInEditMode()]
    public class PixelOutline : MonoBehaviour
    {
        [SerializeField] private RenderTexture _targetRenderTex = null;
        [SerializeField] private Color _outlineColor = new Color();
        [SerializeField] private RawImage _rawImage = null;

        [SerializeField] float _bler = 0.1f;

        private Texture2D _drawTex;
        private Color[] _cachedPixels;
        private Color[] _drawBuffer;
        private int _drawBufferLength;

        private Task _drawTask;

        int _height = 0;
        int _width = 0;
        static readonly Color ALPHA = new Color(0, 0, 0, 0);

        void Start()
        {
            _width = _targetRenderTex.width;
            _height = _targetRenderTex.height;

            _drawTex = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            _drawTex.anisoLevel = 9;
            _drawTex.wrapMode = TextureWrapMode.Clamp;
            _drawTex.filterMode = FilterMode.Point;
            _drawTex.name = "PixelScreenRawTexture";

            _rawImage.texture = _drawTex;

            _drawBuffer = new Color[_width * _height];
            _drawBufferLength = _drawBuffer.Length;
            Debug.LogFormat("{0}/{1}", _width * _height, _drawBuffer.Length);
        }

        void LateUpdate()
        {
            RenderTexture.active = _targetRenderTex;
            _drawTex.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
            _cachedPixels = _drawTex.GetPixels();
            for (var y = 1; y < _height - 1; y++)
            {
                for (var x = 1; x < _width - 1; x++)
                {
                    var index = x + y * _width;
                    var sum =
                        _cachedPixels[x + (y + 1) * _width].a +
                        _cachedPixels[x + (y - 1) * _width].a +
                        _cachedPixels[(x + 1) + y * _width].a +
                        _cachedPixels[(x - 1) + y * _width].a;

                    var blend = 1 - _bler;
                    if (_cachedPixels[index].a < 0.3f && sum > 0)
                    {
                        _drawBuffer[index].r = (_drawBuffer[index].r * (1 - blend) + _outlineColor.r * blend);
                        _drawBuffer[index].g = (_drawBuffer[index].g * (1 - blend) + _outlineColor.g * blend);
                        _drawBuffer[index].b = (_drawBuffer[index].b * (1 - blend) + _outlineColor.b * blend);
                        _drawBuffer[index].a = (_drawBuffer[index].a * (1 - blend) + _outlineColor.a * blend);
                        _drawTex.SetPixel(x, y, _drawBuffer[index]);
                    }
                }
            }
            _drawTex.Apply();
        }
    }
}