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
        private Score _score = null;
        private Wave _wave = null;
        private Note _touchNote = null;

        public bool IsPlaying => _start;

        public void Initialize()
        {
            _wave = new Wave();
            _wave.Initialize();;
            _touchNote = new Note(0);
        }

        public void Play(Score score, long offset)
        {
            _index = offset;
            _score = score;
            _start = true;
            Wave.ResetCount(_score);
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
            _index = 0;
            _score = null;
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
                        data[i * channels + ch] = _wave.Calc(i, _touchNote);
                    }
                    _index++;
                }
            }
            
            if (!_start) return;

            for (var i = 0; i < data.Length / channels; i++)
            {
                for (var ch = 0; ch < channels; ch++)
                {
                    data[i * channels + ch] = Wave.CreateWave(_score, ch, _index);
                }
                _index++;
            }
        }
    }
}