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
        [SerializeField] private Color _noteColor;

        public static int Width = 0;
        public static int Height = 0;

        public static Color[] TextureBuffer = null;
        private bool _isUpdateTexture = false;

        public UnityEvent<Vector2> OnPointerEvent { get; private set; } = new UnityEvent<Vector2>();

        public async UniTask InitializeAsync()
        {
        }

        public static void InitializeTextureBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            var fillLen = width * height;
            TextureBuffer = new Color[fillLen];
            for (var i = 0; i < fillLen; i++)
            {
                TextureBuffer[i] = Color.clear;
            }
        }

        public void SetNoteColor(Color color)
        {
            _noteColor = color;
        }

        public void Write(int index, double melo, double vol)
        {
            var threshold = (int) (melo * Height);

            if (index < 0 || index + 1 > Width) return;
            TextureBuffer[index + threshold * Width].r = _noteColor.r;
            TextureBuffer[index + threshold * Width].g = _noteColor.g;
            TextureBuffer[index + threshold * Width].b = _noteColor.b;
            TextureBuffer[index + threshold * Width].a = 1;
        }

        public void Erase(int index)
        {
            if (index < 0 || index + 1 > Width) return;
            for (var i = 0; i < Height; i++)
            {
                TextureBuffer[index + i * Width].a = 0;
            }
        }

        public void UpdateFrame()
        {
        }

        public static void Clear()
        {
            var len = TextureBuffer.Length;
            for (var i = 0; i < len; i++)
            {
                TextureBuffer[i].a = 0;
            }
        }
    }
}