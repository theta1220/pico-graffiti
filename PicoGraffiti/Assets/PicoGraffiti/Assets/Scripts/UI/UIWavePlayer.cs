using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using Tuna;
using UnityEngine;

namespace PicoGraffiti.UI
{
    public class UIWavePlayer : TunaBehaviour
    {
        private bool _start = false;
        private long _index = 0;
        private Note _touchNote = null;
        private bool _isCode = false;
        
        public bool IsPlaying => _start;
        public long Index => _index;
        public Tuna.Object<UIWaveMonitor> UIWaveMonitor { get; private set; }

        private Wave Wave => AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.Wave;

        public async UniTask InitializeAsync(Transform monitorParent)
        {
            _touchNote = new Note();
            UIWaveMonitor = await Tuna.Object<UIWaveMonitor>.Create(monitorParent);
            await UIWaveMonitor.Instance.InitializeAsync();
        }

        public void UpdateFrame()
        {
            UIWaveMonitor.Instance.UpdateFrame();
        }

        public void Play(long offset)
        {
            _index = offset;
            _start = true;
            Wave.ResetCount(AppGlobal.Instance.ScoreRepository.Instance.Score);
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

        public void OnWriteOn()
        {
            Wave.ResetCount(AppGlobal.Instance.ScoreRepository.Instance.Score);
        }

        public void OnWrite(double melo, WaveType waveType, bool isCode)
        {
            _touchNote.Melo = melo;
            _touchNote.WaveType = waveType;
            _isCode = isCode;

            if (IsPlaying)
            {
                Stop();
            }
        }

        public void OnWriteOff()
        {
            _touchNote.Melo = -1;
            Wave.ResetCount(AppGlobal.Instance.ScoreRepository.Instance.Score);
        }

        public void OnAudioFilterRead(float[] data, int channels)
        {
            if (_touchNote.Melo >= 0)
            {
                for (var i = 0; i < data.Length / channels; i++)
                {
                    for (var ch = 0; ch < channels; ch++)
                    {
                        data[i * channels + ch] = Wave.Calc(_touchNote, _isCode, 1.0f, AppGlobal.Instance.ScoreRepository.Instance.Score.Trans, ch == 0);
                    }
                }
            }
            else if (_start)
            {
                for (var i = 0; i < data.Length / channels; i++)
                {
                    for (var ch = 0; ch < channels; ch++)
                    {
                        data[i * channels + ch] = Wave.CreateWave(AppGlobal.Instance.ScoreRepository.Instance.Score, ch, _index);
                    }
                    _index++;
                }
            }
            UIWaveMonitor.Instance.Stack(data);
        }
    }
}