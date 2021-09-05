using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NAudio.Wave;
using PicoGraffiti.Model;
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

        public static void Save(Score score, string path)
        {
            var wave = new Wave();
            wave.SaveInternal(score, path);
        }

        void SaveInternal(Score score, string path)
        {
            ResetCount(score);
            
            // 波形作成
            var wave = CreateAllWave(score);

            using (var fs = new FileStream(path, FileMode.Create))
            using (var wr = new WaveFileWriter(fs, WaveFormat.CreateIeeeFloatWaveFormat((int)SAMPLE_RATE, 2)))
            {
                wr.WriteSamples(wave.ToArray(), 0, wave.Count);
            }
            
            Debug.Log("Wavファイル生成完了");
        }

        private List<float> CreateAllWave(Score score)
        {
            var size = score.GetSize() * Track.NOTE_GRID_SIZE + SAMPLE_RATE * 3;
            var list = new List<float>();
            for (long i = 0; i < size; i++)
            {
                for (var ch = 0; ch < 2; ch++)
                {
                    list.Add(CreateWave(score, ch, i));
                }
            }

            return list;
        }
        
        public static float CreateWave(Score score, int ch, long index)
        {
            if (score == null) return 0;
            
            var wave = 0f;
            foreach (var track in score.Tracks)
            {
                wave += track.Wave.Calc(index, track.GetNote(index));
            }

            return wave;
        }

        public static void ResetCount(Score score)
        {
            foreach (var track in score.Tracks)
            {
                track.Wave._rCount = 0;
                track.Wave._currentNote = null;
                track.Wave._prevNote = null;
            }
        }

        private double _currentR = 0;
        public long _rCount = 0;
        private Note _prevNote = null;
        private Note _currentNote = null;
        private float _vol = 1.0f;
        
        public float Calc(long index, Note note)
        {
            var curNote = note;
            if (note == null)
            {
                if (_prevNote != null)
                {
                    _vol = (float)_prevNote.Vol * 0.4f;
                }
                curNote = _currentNote;
                _vol -= 0.00001f;
                if (_vol < 0) _vol = 0;
            }
            else
            {
                _vol = 1.0f;
            }
            _prevNote = note;
            if (curNote == null) return 0;
            
            // 音階周波数
            var freq = A0 * Math.Pow(SCALE_FREQ, curNote.Melo * MELO_NUM + 2);
            // 周波数
            var r = SAMPLE_RATE / freq * 4;
            if (_rCount >= (long)_currentR)
            {
                _currentR = r;
                _rCount = 0;
            }
            else
            {
                _rCount++;
            }

            _currentNote = curNote;

            return CalcWave(_currentR, _rCount, curNote.WaveType, (float)curNote.Vol * 0.2f * _vol);
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
                    buff = GetPinkNoise(t, vol);
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
        public float GetPinkNoise(double t, float vol)
        {
            var r = _currentR / 128;
            var rCount = _rCount;
            if ((long)r == 0 || rCount % (long)r == 0)
            {
                _pinkNoiseBuff = (float) _random.NextDouble() * 2 - 1;
            }

            return _pinkNoiseBuff * vol;
        }
    }
}