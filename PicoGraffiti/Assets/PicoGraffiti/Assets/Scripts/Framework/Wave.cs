using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        public const int MELO_NUM = 50;

        private int _noiseBuff;

        System.Random _random = new System.Random();

        private List<float> _whiteNoisePattern = new List<float>();
        private List<float> _pinkNoisePattern = new List<float>();

        public void Initialize()
        {
            for (var i = 0; i < 32768; i++)
            {
                _whiteNoisePattern.Add(Random.Range(-1.0f, 1.0f));
            }

            for (var i = 0; i < 128; i++)
            {
                _pinkNoisePattern.Add(Random.Range(-1.0f, 1.0f));
            }
        }

        public async UniTask Save(Score score, string path)
        {
            await SaveInternal(score, path);
        }

        async UniTask SaveInternal(Score score, string path)
        {
            // 波形作成
            var wave = CreateAllWave(score);
            uint wavelen = (uint) wave.Count;

            // WAVEファイルヘッダ
            var header = new WaveFileHeader();

            header.nChannels = 2;
            header.wBitsPerSample = 32;
            header.nSamplesPerSec = SAMPLE_RATE;
            header.nBlockAlign = (ushort) (header.wBitsPerSample / 8 * header.nChannels);
            header.nAvgBytesPerSec = (uint) SAMPLE_RATE * header.nBlockAlign;

            header.riff_cksize = 36 + wavelen * 2;
            header.data_cksize = wavelen * 2;

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                // ヘッダーの書き込み
                FieldInfo[] infos = typeof(WaveFileHeader).GetFields();
                foreach (FieldInfo info in infos)
                {
                    byte[] ba = BitConverter.GetBytes(Convert.ToUInt32(info.GetValue(header)));
                    stream.Write(ba, 0, Marshal.SizeOf(info.FieldType));
                }

                // 波形の書き込み
                List<byte> bin = new List<byte>();
                foreach (var w in wave)
                {
                    var buf = new List<byte>();
                    foreach (var b in BitConverter.GetBytes(Convert.ToSingle(w)))
                    {
                        buf.Add(b);
                    }

                    buf.Reverse();
                    foreach (var b in buf)
                    {
                        bin.Add(b);
                    }
                }

                stream.Write(bin.ToArray(), 0, bin.Count);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        private List<float> CreateAllWave(Score score)
        {
            var size = score.GetSize();
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
                    _vol = 0.5f;
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
            var freq = A0 * Math.Pow(SCALE_FREQ, curNote.Melo * MELO_NUM + 12 * 1 + 2);
            // 周波数
            var r = SAMPLE_RATE / freq * 4;
            if (_rCount == (long)_currentR)
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
            var index = Mathf.FloorToInt(_whiteNoisePattern.Count * (float) t);
            if (index >= _whiteNoisePattern.Count)
            {
                index = 0;
            }

            return _whiteNoisePattern[index] * vol;
        }

        public float GetPinkNoise(double t, float vol)
        {
            var index = Mathf.FloorToInt(_pinkNoisePattern.Count * (float) t);
            if (index >= _pinkNoisePattern.Count)
            {
                index = 0;
            }

            return _pinkNoisePattern[index] * vol;
        }
    }
}