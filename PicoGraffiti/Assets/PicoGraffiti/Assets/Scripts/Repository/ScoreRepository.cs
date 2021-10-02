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
            var track = new Track(Identity.Get(), Score, WaveType.Square25, WaveType.Square);
            Score.Tracks.Add(track);
            CurrentTrack = track;

            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Square25, WaveType.Square));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Square25, WaveType.Square));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Square25, WaveType.Square));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Square25));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Square25, WaveType.Square, false, true));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Triangle, WaveType.None, true));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Noise2));
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
            return obj;
        }
    }
}