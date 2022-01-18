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
        private Track _track;
        
        public Track Track => _track;

        public Wave(ulong trackId)
        {
            _trackId = trackId;
            _track = AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks.FirstOrDefault(_ => _.Id == _trackId);

            _rCounters = new[]
            {
                new RCounter(this, 0, 1, 1.0f, 0.1),
                new RCounter(this, 3, 2, 1.0f, -0.1),
                new RCounter(this, 2, 3/2.0f, 1.0f, 0.2),
                new RCounter(this, 1, 3, 1.0f, -0.2),
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
                if (count == 0) vol = 8.0f;
                if (count == 1) vol = 6.0f;
                if (count == 2) vol = 6.0f;
                if (count == 3) vol = 7.0f;
                if (count == 4) vol = 3.0f;
                if (count == 5) vol = 4.0f;
                if (count == 6) vol = 15.0f;
                if (count == 7) vol = 1.5f;
                if (count == 8) vol = 1.5f;
                
                wave += track.Wave.Calc(track.GetNote(index), track.IsCode, vol, track.ParentScore.Trans, ch == 0);
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
            private float _rate;
            private int _overrideCount = 0;
            private int _kickCount = 0;
            private int _chorusCount = 0;
            private int _attackCount = 0;
            private double _pan = 0;

            private Wave _wave = null;
            private Track _track = null;

            public RCounter(Wave wave, int index, float rate, float vol, double pan)
            {
                _wave = wave;
                _index = index;
                _rate = rate;
                _vol = vol;
                _pan = pan;

                _overrideCount = 0;
                _kickCount = 0;
                _attackCount = 0;

                _track = _wave.Track;
            }

            public float Calc(Wave wave, Note note, float vol, int trans, bool R, float freqRate)
            {
                // 音階周波数
                if (note.WaveType == WaveType.Noise2 || note.WaveType == WaveType.Noise || _track.IsKick)
                {
                    trans = 0;
                }

                var bpmRate = 60.0 / (_track.ParentScore.BPM * Model.Track.NOTE_GRID_SIZE);
                var freq = A0 * Math.Pow(SCALE_FREQ, note.Melo * 89 + trans);

                // 周波数
                var r = SAMPLE_RATE / freq * 4;
                
                // Chorus
                if (_track.IsChorus)
                {
                    _chorusCount++;
                }
                
                // キック
                if (_track.IsKick)
                {
                    _kickCount++;
                }

                // 波形オーバーライド
                var waveType = note.WaveType;
                if (_track.OverrideWaveType != WaveType.None && 
                    _overrideCount < SAMPLE_RATE * bpmRate * _track.OverrideWaveTime)
                {
                    waveType = _track.OverrideWaveType;
                    _overrideCount++;
                }
                // 波形オーバーライド２
                else if (_track.SecondOverrideWaveType != WaveType.None && 
                         _overrideCount < SAMPLE_RATE * bpmRate * _track.SecondOverrideWaveTime)
                {
                    waveType = _track.SecondOverrideWaveType;
                    _overrideCount++;
                }
                
                // アタック
                var attack = (float) (1 - _attackCount / ((float) SAMPLE_RATE * bpmRate * 80)) * 1.5f;
                if (waveType == WaveType.Noise)
                {
                    attack = (float) (1 - _attackCount / ((float) SAMPLE_RATE * bpmRate * 50)) * 1.2f;
                    attack *= 4;
                }
                else if (waveType == WaveType.Noise2)
                {
                    attack = (float) (1 - _attackCount / ((float) SAMPLE_RATE * bpmRate * 70)) * 1.2f;
                    attack *= 4;
                }
                if (attack < 0) attack = 0;
                vol *= 1.0f + attack;
                _attackCount++;

                // 周波数Update
                if (_count >= (long) _currentR)
                {
                    if (_wave.Track.IsChorus)
                    {
                        var len = bpmRate * SAMPLE_RATE;
                        var i = _chorusCount / len / 10;

                        if ((int) i % 2 == 0)
                        {
                            r = r * 2.01;
                            if (R) vol *= 1.4f;
                        }
                        else
                        {
                            if (!R) vol *= 1.4f;
                        }
                    }

                    if (_wave.Track.IsCode)
                    {
                        var len = bpmRate * SAMPLE_RATE * Model.Track.NOTE_GRID_SIZE;
                        var i = _chorusCount / len / 8;

                        if ((int) i % 2 == 0)
                        {
                            r = r * 2.01;
                            if (R) vol *= 1.4f;
                        }
                        else
                        {
                            if (!R) vol *= 1.4f;
                        }
                    }

                    if (wave.Track.IsKick)
                    {
                        var t = _kickCount / (double) Wave.SAMPLE_RATE;
                        var s = Mathf.Pow((float)t * 10, 2) * 500;
                        r += s;
                    }
                    _currentR = r / _rate / freqRate;
                    _count = 0;
                }
                else
                {
                    _count++;
                }
                
                // パン
                vol *= 1.0f + (float)_track.Pan * (R ? 1 : -1) * 3;
                vol *= 1.0f + (float)_pan * (R ? 1 : -1) * 3;

                if (_track.IsCode)
                {
                    vol *= 1.0f + (_index % 2 == 0 ? 1 : -1);
                }
                
                // master
                var masterVol = 0.13f;

                return wave.CalcWave(_currentR, _count, waveType, (float) note.Vol * 0.2f * _vol * vol * masterVol);
            }

            public void ResetCount()
            {
                _count = 0;
                _overrideCount = 0;
                _kickCount = 0;
                _currentR = 0;
                _chorusCount = 0;
                _attackCount = 0;
            }

            public void ResetOverrideCount()
            {
                _overrideCount = 0;
                _kickCount = 0;
                _attackCount = 0;
                if (_wave.Track.IsKick)
                {
                    _count = 0;
                    _currentR = 0;
                }
                _chorusCount = 0;
            }
        }

        public float Calc(Note note, bool isCode, float vol, int trans, bool R)
        {
            var curNote = note;
            if (note == null)
            {
                if (_prevNote != null)
                {
                    _vol = (float) _prevNote.Vol * 
                           (_prevNote.WaveType == WaveType.Noise2 ||
                            _prevNote.WaveType == WaveType.Noise ? 0.7f : 1.0f); 
                }

                curNote = _currentNote;
                _vol -= 0.00001f;
                if (_vol < 0)
                {
                    _vol = 0;
                }
            }
            else
            {
                if (_prevNote == null)
                {
                    foreach (var rCounter in _rCounters)
                    {
                        rCounter.ResetOverrideCount();
                    } 
                }
                _vol = 1.0f;
            }

            _prevNote = note;
            if (curNote == null) return 0;
            if (curNote.Melo < 0) return 0;

            var buf = 0.0f;
            var count = 0;
            foreach (var rCounter in _rCounters)
            {
                buf += rCounter.Calc(this, curNote, _vol * vol, trans, R, 1 + count * 0.0001f);

                count++;
                if (count >= _track.Harmony) break;
            }

            _currentNote = curNote;
            return buf;
        }

        float CalcWave(double r, long count, WaveType waveType, float vol)
        {
            float buff = 0;
            // 位相
            var diff = 2 * Mathf.PI / count;
            switch (waveType)
            {
                case WaveType.Square:
                {
                    // ハイパスフィルターをかけてる
                    buff = Hipath(0.5, count, r, vol);
                    break;
                }

                case WaveType.Square125:
                {
                    // ハイパスフィルターをかけてる
                    buff = Hipath(0.125, count, r, vol);
                    break;
                }

                case WaveType.Square25:
                {
                    // ハイパスフィルターをかけてる
                    buff = Hipath(0.25, count, r, vol);
                    break;
                }

                case WaveType.Square75:
                {
                    // ハイパスフィルターをかけてる
                    buff = Hipath(0.75, count, r, vol);
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
                    buff += GetWhiteNoise(t, vol * 0.2f);
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

        public float Hipath(double duty, long count, double r, float vol)
        {
            var R = -1.0;
            var t = count % r / r;
            var s = (t > duty ? 1 : -1);
            var d = (s == 1) ? 0.00001f : duty;
            var f = t / (t + (1 / R)) * s;
            var buff = (float) (s * vol * 0.4);
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

        public static bool isSharp(int melo)
        {
            var sharp = new[] {false, true, false, true, false, true, true, false, true, false, true, true};
            return sharp[melo % sharp.Length];
        }
    }
}