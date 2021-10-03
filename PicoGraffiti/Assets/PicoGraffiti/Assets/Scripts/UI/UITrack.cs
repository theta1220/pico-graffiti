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

        public TextureBuffer TextureBuffer = null;
        private bool _isUpdateTexture = false;
        private UITrackParticle _particle = null;

        public UnityEvent<Vector2> OnPointerEvent { get; private set; } = new UnityEvent<Vector2>();

        public async UniTask InitializeAsync()
        {
        }

        public void InitializeTextureBuffer(TextureBuffer textureBuffer, int width, int height)
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

            _particle = new UITrackParticle(TextureBuffer);
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
            WritePixelInternal(x, y + 1);
            WritePixelInternal(x, y - 1);
        }

        private void WritePixelInternal(int x, int y)
        {
            TextureBuffer.Draw(x, y, ref _noteColor);
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
            _particle.Update(ref _noteColor);
        }

        public void CreateParticle(int x, int y, bool isLow)
        {
            _particle.Create(x, y, isLow);
        }
    }
}