using System;
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
        private Color _noteColor;

        public int Width = 0;
        public int Height = 0;

        public UIScore.TextureBuffer TextureBuffer = null;
        private bool _isUpdateTexture = false;

        public UnityEvent<Vector2> OnPointerEvent { get; private set; } = new UnityEvent<Vector2>();

        public async UniTask InitializeAsync()
        {
        }

        public void InitializeTextureBuffer(UIScore.TextureBuffer textureBuffer, int width, int height)
        {
            TextureBuffer = textureBuffer;
            Width = width;
            Height = height;
            var fillLen = width * height;
            TextureBuffer.Buffer = new Color[fillLen];
            for (var i = 0; i < fillLen; i++)
            {
                TextureBuffer.Buffer[i] = Color.clear;
            }
        }

        public void SetNoteColor(Color color)
        {
            _noteColor = color;
        }

        public void Write(int index, double value)
        {
            var y = (int) (value * (Height - 1));
            WritePixel(index, y);
        }

        private void WritePixel(int x, int y)
        {
            WritePixelInternal(x, y);
            WritePixelInternal(x + 1, y);
            WritePixelInternal(x, y + 1);
            WritePixelInternal(x + 1, y + 1);
        }

        private void WritePixelInternal(int x, int y)
        {
            var index = x + y * Width;
            if (x == Width) return;
            if (index < 0 || index >= TextureBuffer.Buffer.Length) return;
            TextureBuffer.Buffer[index].r = _noteColor.r;
            TextureBuffer.Buffer[index].g = _noteColor.g;
            TextureBuffer.Buffer[index].b = _noteColor.b;
            TextureBuffer.Buffer[index].a = 1;
        }

        public void Erase(int index)
        {
            if (index < 0 || index + 1 > Width) return;
            for (var i = 0; i < Height; i++)
            {
                TextureBuffer.Buffer[index + i * Width].a = 0;
            }
        }

        public void UpdateFrame()
        {
        }
    }
}