using Cysharp.Threading.Tasks;
using Stocker.Framework;
using Tuna;
using UnityEngine;
using UnityEngine.UI;

namespace PicoGraffiti.UI
{
    public class UIWaveMonitor : TunaBehaviour
    {
        [SerializeField] private RawImage _image = default;
        [SerializeField] private Color _graphColor = Color.white;
        
        private Texture2D _texture = null;
        private int _width = 0;
        private int _height = 0;
        private QueueStack<float> _valueQueue = null;
        private TextureBuffer _textureBuffer = null;

        public async UniTask InitializeAsync()
        {
            await UniTask.SwitchToMainThread();
            await UniTask.Yield();
            _width = (int) _image.rectTransform.rect.width;
            _height = (int)_image.rectTransform.rect.height;
            _texture = new Texture2D(_width, _height);
            _texture.filterMode = FilterMode.Point;
            _valueQueue = new QueueStack<float>();
            _textureBuffer = new TextureBuffer(_width, _height);
            _image.texture = _texture;
        }

        public void Stack(float[] value)
        {
            lock (_valueQueue)
            {
                _valueQueue.Clear();
                for (var i = 0; i < _width; i++)
                {
                    if(i * 4 >= value.Length) break;
                    _valueQueue.PushBack(value[value.Length - 1 - i * 4]);
                }
            }
        }

        public void UpdateFrame()
        {
            Draw();
        }

        private void Draw()
        {
            _textureBuffer.Clear();

            var last = _height / 2;
            var count = _width;

            lock (_valueQueue)
            {
                var arr = _valueQueue.ToArray();
                foreach (var value in arr)
                {
                    DrawLine(new Vector2(count - 1, last), new Vector2(count, (int)(value * _height / 2 + _height / 2f)), ref _graphColor);
                    last = (int)(value * _height / 2 + _height / 2f);
                    count--;
                }
            }
            
            _texture.SetPixels(_textureBuffer.Buffer);
            _texture.Apply();
        }

        private void DrawLine(Vector2 start, Vector2 end, ref Color color)
        {
            var distance = Vector2.Distance(start, end);
            for (var i = 0; i < distance; i++)
            {
                var pos = start + (end - start) * (i / distance);
                _textureBuffer.Draw((int) pos.x, (int) pos.y, ref color);
            }
        }
    }
}