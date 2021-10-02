using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NAudio.Wave;
using PicoGraffiti.Model;
using Stocker.Framework;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PicoGraffiti.Framework
{
    [Serializable]
    public class Wave
    {
        public const uint SAMPLE_RATE = 44100;
        public const double SCALE_FREQ = 1.059463094;
        public const double A0 = 32.703;
        public const int MELO_NUM = 88;

        System.Random _random = new System.Random();

        private ulong _trackId;

        Track Track
        {
            get
            {
                return AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks.FirstOrDefault(_ => _.Id == _trackId);
            }
        }

        public Wave(ulong trackId)
        {
            _trackId = trackId;
            
            _rCounters = new[]
            {
                new RCounter(this, 0, 1, 1),
                new RCounter(this, 1, 1, 1),
                new RCounter(this, 2, 1, 1),
                new RCounter(this, 3, 1, 1),
            };
        }

        public static async UniTask SaveAsync(Score score, string path)
        {
            await SaveInternalAsync(score, path);
        }

        private static async UniTask SaveInternalAsync(Score score, string path)
        {
            ResetCount(score);

            // 波形作成
            var wave = await CreateAllWave(score);

            using (var fs = new FileStream(path, FileMode.Create))
            using (var wr = new WaveFileWriter(fs, WaveFormat.CreateIeeeFloatWaveFormat((int) SAMPLE_RATE, 2)))
            {
                wr.WriteSamples(wave.ToArray(), 0, wave.Count);
            }

            Debug.Log("Wavファイル生成完了");
        }

        private static async UniTask<List<float>> CreateAllWave(Score score)
        {
            var bpmRate = 60.0 / (score.BPM * Track.NOTE_GRID_SIZE);
            var size = bpmRate * Wave.SAMPLE_RATE * score.GetSize() + SAMPLE_RATE * 3;
            var list = new List<float>();
            for (long i = 0; i < size; i++)
            {
                for (var ch = 0; ch < 2; ch++)
                {
                    list.Add(CreateWave(score, ch, i));
                }

                if (i % 5000 == 0)
                {
                    await UniTask.Yield();
                }
            }

            return list;
        }

        public static float CreateWave(Score score, int ch, long index)
        {
            if (score == null) return 0;

            var wave = 0f;
            var count = 0;
            foreach (var track in score.Tracks)
            {
                var vol = 1.0f;
                if (count == 0 || count == 1 || count == 3) vol = 1.3f;
                if (count == 6) vol = 3.5f;
                if (count == 7) vol = 1.0f;
                wave += track.Wave.Calc(track.GetNote(index), false, vol, track.ParentScore.Trans, ch == 0);
                count++;
            }

            return wave;
        }


        private Note _prevNote = null;
        private Note _currentNote = null;
        private float _vol;
        private RCounter[] _rCounters;

        public static void ResetCount(Score score)
        {
            foreach (var track in score.Tracks)
            {
                track.Wave.ResetCount();
            }
        }

        public void ResetCount()
        {
            foreach (var rCounter in _rCounters)
            {
                rCounter.ResetCount();
            }
            _currentNote = null;
            _prevNote = null;
        }

        [Serializable]
        private class RCounter
        {
            private int _index = 0;
            private double _currentR = 0;
            private long _count = 0;
            private float _vol;
            private int _rate;
            private int _overrideCount = 0;
            private int _kickCount = 0;
            private int _chorusCount = 0;

            private Wave _wave = null;

            public RCounter(Wave wave, int index, int rate, float vol)
            {
                _wave = wave;
                _index = index;
                _rate = rate;
                _vol = vol;

                _overrideCount = 0;
                _kickCount = 0;
            }

            public float Calc(Wave wave, Note note, float vol, int trans, bool R)
            {
                // 音階周波数
                if (note.WaveType == WaveType.Noise2 || note.WaveType == WaveType.Noise || wave.Track.IsKick)
                {
                    trans = 0;
                }

                var freq = A0 * Math.Pow(SCALE_FREQ, note.Melo * 89 + trans);

                // 周波数
                var r = SAMPLE_RATE / freq * 4 + (R ? _currentR * 0.01 : 0);
                
                // Chorus
                if (_wave.Track.IsChorus)
                {
                    var bpmRate = 60.0 / (_wave.Track.ParentScore.BPM * Model.Track.NOTE_GRID_SIZE);
                    var len = bpmRate * SAMPLE_RATE;
                    var i = _chorusCount / len / 10;

                    if ((int)i % 2 == 0)
                    {
                        r *= 2;
                    }
                    _chorusCount++;
                }
                
                // キック
                if (wave.Track.IsKick)
                {
                    var t = _kickCount / (double) Wave.SAMPLE_RATE;
                    var s = Math.Pow(t * 10, 2) * 500;
                    r += s;
                    _kickCount++;
                }
                
                if (_count >= (long) _currentR)
                {
                    _currentR = r * _rate;
                    _count = 0;
                }
                else
                {
                    _count++;
                }

                // 波形オーバーライド
                var waveType = note.WaveType;
                if (_overrideCount < SAMPLE_RATE * 0.125 && waveType == WaveType.Square25)
                {
                    var track = _wave.Track;
                    if (track == null)
                    {
                        waveType = WaveType.Square;
                    }
                    else if (track.OverrideWaveType != WaveType.None)
                    {
                        waveType = track.OverrideWaveType;
                    }

                    vol *= 1.8f;
                    _overrideCount++;
                }

                return wave.CalcWave(_currentR, _count, waveType, (float) note.Vol * 0.2f * _vol * vol * 0.8f);
            }

            public void ResetCount()
            {
                _count = 0;
                _overrideCount = 0;
                _kickCount = 0;
                _currentR = 0;
                _chorusCount = 0;
            }

            public void ResetOverrideCount()
            {
                _overrideCount = 0;
                _kickCount = 0;
                _count = 0;
                _currentR = 0;
                _chorusCount = 0;
            }
        }

        public float Calc(Note note, bool isCode = false, float vol = 1.0f, int trans = 0, bool R = false)
        {
            var curNote = note;
            if (note == null)
            {
                if (_prevNote != null)
                {
                    _vol = (float) _prevNote.Vol * (_prevNote.WaveType == WaveType.Noise2 ? 0.3f : 1.0f);
                }

                curNote = _currentNote;
                _vol -= 0.000007f;
                if (_vol < 0)
                {
                    _vol = 0;
                }
            }
            else
            {
                _vol = 1.0f;
                if (_prevNote == null)
                {
                    foreach (var rCounter in _rCounters)
                    {
                        rCounter.ResetOverrideCount();
                    } 
                }
            }

            _prevNote = note;
            if (curNote == null) return 0;
            if (curNote.Melo < 0) return 0;

            var buf = 0.0f;
            var count = 0;
            foreach (var rCounter in _rCounters)
            {
                if (count == 0 || isCode)
                {
                    buf += rCounter.Calc(this, curNote, _vol * vol, trans, R);
                }

                count++;
            }

            _currentNote = curNote;
            return buf;
        }

        float CalcWave(double r, long count, WaveType waveType, float vol)
        {
            float buff = 0;
            var R = 300000;
            var C = 0.00001;
            // 位相
            var diff = 2 * Mathf.PI / count;
            switch (waveType)
            {
                case WaveType.Square:
                {
                    // ハイパスフィルターをかけてる
                    var duty = 0.5;
                    var t = count % r / r;
                    var s = (t > duty ? 1 : -1);
                    var d = (s == 1) ? 0 : duty;
                    var f = 1 / (1 + R * C * (t + d)) * s;
                    buff = (float) (f * vol * 2);
                    break;
                }

                case WaveType.Square125:
                {
                    // ハイパスフィルターをかけてる
                    var duty = 0.125;
                    var t = count % r / r;
                    var s = (t > duty ? 1 : -1);
                    var d = (s == 1) ? 0 : duty;
                    var f = 1 / (1 + R * C * (t + d)) * s;
                    buff = (float) (f * vol * 2);
                    break;
                }

                case WaveType.Square25:
                {
                    // ハイパスフィルターをかけてる
                    var duty = 0.25;
                    var t = count % r / r;
                    var s = (t > duty ? 1 : -1);
                    var d = (s == 1) ? 0 : duty;
                    var f = 1 / (1 + R * C * (t + d)) * s;
                    buff = (float) (f * vol * 2);
                    break;
                }

                case WaveType.Square75:
                {
                    // ハイパスフィルターをかけてる
                    var duty = 0.75;
                    var t = count % r / r;
                    var s = (t > duty ? 1 : -1);
                    var d = (s == 1) ? 0 : duty;
                    var f = 1 / (1 + R * C * (t + d)) * s;
                    buff = (float) (f * vol * 2);
                    break;
                }

                case WaveType.Triangle:
                {
                    var a = count % r / r;
                    buff = (float) (((a > 0.5 ? a : 1.0f - a) - 0.75) * 4 * vol);
                    break;
                }

                case WaveType.Saw:
                    buff = (int) ((count % r / r - 0.5) * 2 * vol);
                    break;

                case WaveType.Noise:
                {
                    var t = count % r / r;
                    buff = GetWhiteNoise(t, vol);
                    break;
                }

                case WaveType.Noise2:
                {
                    var t = count % r / r;
                    buff = GetPinkNoise(r, count, vol);
                    break;
                }

                case WaveType.Sin:
                {
                    var t = count % r / r;
                    buff = (int) (Math.Sin(t * 2 * Math.PI) * vol);
                    break;
                }
            }

            return buff;
        }

        public float GetWhiteNoise(double t, float vol)
        {
            return (float) (_random.NextDouble() * 2 - 1) * vol;
        }

        private float _pinkNoiseBuff = 0;

        public float GetPinkNoise(double currentR, long count, float vol)
        {
            var r = currentR / 128;
            var rCount = count;
            if ((long) r == 0 || rCount % (long) r == 0)
            {
                _pinkNoiseBuff = (float) _random.NextDouble() * 2 - 1;
            }

            return _pinkNoiseBuff * vol;
        }

        private bool isSharp(int melo)
        {
            var sharp = new[] {false, true, false, true, false, false, true, false, true, false, true, false};
            return sharp[melo % sharp.Length];
        }
    }
}