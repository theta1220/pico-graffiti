using System;
using System.Linq;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using Stocker.Framework;
using UnityEngine;

namespace PicoGraffiti
{
    [Serializable]
    public class ScoreRepository : ICloneable<ScoreRepository>
    {
        public string Version { get; private set; }
        public Identity Identity { get; private set; }
        public Score Score { get; private set; }
        public Track CurrentTrack { get; private set; } = null;

        public ScoreRepository()
        {
            Version = Application.version;
            Score = new Score();
            Identity = new Identity();

            // 主旋律
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square25,
                WaveType.Square,10,
                WaveType.None, 0,
                false, false, false, 2, 0));
            
            // ベース（コード）
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square125,
                WaveType.Square25,10,
                WaveType.None, 0,
                false, false, true, 4, 0));
            
            // ハモリ
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square,
                WaveType.Square,15,
                WaveType.None, 0,
                false, false, false, 2, 0.2));
            
            // コード
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square125,
                WaveType.Square25,15,
                WaveType.None, 0,
                false, false, true, 2, 0));
            
            // ギター的なやつ２
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square125,
                WaveType.Square,10,
                WaveType.Square25, 20,
                false, false, false, 3, -0.3));
            
            // アルペジオくん
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square,
                WaveType.None,0,
                WaveType.None, 0,
                false, true, false, 1,-0.4));
            
            // キック
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Triangle,
                WaveType.None,0,
                WaveType.None, 0,
                true, false, false, 1, 0));
            
            // ノイズ
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Noise2,
                WaveType.None,0,
                WaveType.None, 0,
                false, false, false, 1,0.1));
            
            // ノイズ
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Noise,
                WaveType.None,0,
                WaveType.None, 0,
                false, false, false, 1,-0.1));

            CurrentTrack = Score.Tracks.First();
        }

        public void Initialize()
        {
            Score.Initialize();
        }

        public void SetNextTrack()
        {
            CurrentTrack = Score.Tracks[(Score.Tracks.IndexOf(CurrentTrack) + 1) % Score.Tracks.Count];
        }

        public void SetCurrentTrack(int index)
        {
            CurrentTrack = Score.Tracks[index];
        }

        public ScoreRepository DeepClone()
        {
            var obj = new ScoreRepository();
            obj.Identity = Identity.DeepClone();
            obj.Score = Score.DeepClone();
            obj.CurrentTrack = obj.Score.Tracks.First(_ => CurrentTrack.Id == _.Id);
            obj.Initialize();
            return obj;
        }
    }
}