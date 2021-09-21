using System;
using System.Collections.Generic;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using Tuna;

namespace PicoGraffiti.UI
{
    public class UIWavePlayer : TunaBehaviour
    {
        private bool _start = false;
        private long _index = 0;
        private Stocker.Framework.Version<ScoreRepository> _scoreRepo = null;
        private Wave _wave = null;
        private Note _touchNote = null;
        public bool IsPlaying => _start;
        public long Index => _index;

        public void Initialize(Stocker.Framework.Version<ScoreRepository> scoreRepo)
        {
            _scoreRepo = scoreRepo;
            _wave = new Wave(null);
            _touchNote = new Note();
        }

        public void Play(long offset)
        {
            _index = offset;
            _start = true;
            Wave.ResetCount(_scoreRepo.Instance.Score);
        }

        public void Pause()
        {
            _start = false;
        }

        public void Resume()
        {
            _start = true;
        }

        public void Stop()
        {
            _start = false;
        }

        public void OnWrite(double melo, WaveType waveType)
        {
            _touchNote.Melo = melo;
            _touchNote.WaveType = waveType;

            if (IsPlaying)
            {
                Stop();
            }
        }

        public void OnWriteOff()
        {
            _touchNote.Melo = -1;
        }

        public void OnAudioFilterRead(float[] data, int channels)
        {
            if (_touchNote.Melo >= 0)
            {
                for (var i = 0; i < data.Length / channels; i++)
                {
                    for (var ch = 0; ch < channels; ch++)
                    {
                        data[i * channels + ch] = _wave.Calc(_touchNote, false, 1.0f, _scoreRepo.Instance.Score.Trans);
                    }
                }
            }
            
            if (!_start) return;

            for (var i = 0; i < data.Length / channels; i++)
            {
                for (var ch = 0; ch < channels; ch++)
                {
                    data[i * channels + ch] = Wave.CreateWave(_scoreRepo.Instance.Score, ch, _index);
                }
                _index++;
            }
        }
    }
}