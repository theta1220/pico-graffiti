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
                WaveType.None,0,
                WaveType.None, 0,
                false, false));
            
            // ベース
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square25,
                WaveType.Square125,10,
                WaveType.None, 0,
                false, false));
            
            // ハモリ
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square25,
                WaveType.Square,15,
                WaveType.None, 0,
                false, false));
            
            // ギター的なやつ
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square25,
                WaveType.Square125,15,
                WaveType.None, 0,
                false, false));
            
            // ギター的なやつ２
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square125,
                WaveType.Square,10,
                WaveType.Square25, 20,
                false, false));
            
            // アルペジオくん
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Square,
                WaveType.None,0,
                WaveType.None, 0,
                false, true));
            
            // キック
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Triangle,
                WaveType.None,0,
                WaveType.None, 0,
                true, false));
            
            // ノイズ
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Noise2,
                WaveType.None,0,
                WaveType.None, 0,
                false, false));
            
            // ノイズ
            Score.Tracks.Add(new Track(
                Identity.Get(), Score,
                WaveType.Noise,
                WaveType.None,0,
                WaveType.None, 0,
                false, false));

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