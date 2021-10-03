using System.Collections.Generic;
using UnityEngine;

namespace PicoGraffiti.UI
{
    public class UITrackParticle
    {
        private class Particle
        {
            private Vector2 _pos = Vector2.zero;
            private float _movePow = 0;
            private float _jumpPow = 0;
            private const float FALL_SPEED = 3f;
            private TextureBuffer _textureBuffer = null;
            private bool _isBig = false;
            private int _life = 10;
            
            public bool DisposeFlag { get; private set; } = false;

            public Particle(int x, int y, TextureBuffer textureBuffer)
            {
                _pos = new Vector2(x, y);
                _textureBuffer = textureBuffer;
                _movePow = Random.Range(0f, 1f);
                _jumpPow = Random.Range(5f, 6f);
                _life = Random.Range(30, 60);
                _isBig = Random.Range(0, 2) == 0;
            }

            public void Low()
            {
                // _movePow *= 0.5f;
                // _jumpPow *= 0.5f;
            }

            public void Update(ref Color color)
            {
                _movePow *= 0.99f;
                _jumpPow *= 0.99f;

                _pos.x += _movePow;
                _pos.y += _jumpPow;
                _pos.y -= FALL_SPEED + Random.Range(0, FALL_SPEED);

                _life--;

                Draw(ref color);
            }

            private void Draw(ref Color color)
            {
                if (_isBig)
                {
                    _textureBuffer.Draw((int) _pos.x + 1, (int) _pos.y, ref color);
                    _textureBuffer.Draw((int) _pos.x - 1, (int) _pos.y, ref color);
                    _textureBuffer.Draw((int) _pos.x, (int) _pos.y + 1, ref color);
                    _textureBuffer.Draw((int) _pos.x, (int) _pos.y - 1, ref color);
                    if (!_textureBuffer.Draw((int)_pos.x, (int)_pos.y, ref color))
                    {
                        DisposeFlag = true;
                    }
                }
                else
                {
                    if (!_textureBuffer.Draw((int) _pos.x, (int) _pos.y, ref color))
                    {
                        DisposeFlag = true;
                    }
                }

                if (_life == 0)
                {
                    DisposeFlag = true;
                }
            }
        }

        private List<Particle> _particles = null;
        private TextureBuffer _textureBuffer = null;
        
        public UITrackParticle(TextureBuffer textureBuffer)
        {
            _particles = new List<Particle>();
            _textureBuffer = textureBuffer;
        }

        public void Create(int x, int y, bool isLow)
        {
            var par = new Particle(x, y, _textureBuffer);
            if(isLow) par.Low();
            _particles.Add(par);
        }

        public void Update(ref Color color)
        {
            foreach (var particle in _particles.ToArray())
            {
                particle.Update(ref color);
                if (particle.DisposeFlag)
                {
                    _particles.Remove(particle);
                }
            }
        }
    }
}