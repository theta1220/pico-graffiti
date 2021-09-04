using System;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using Tuna.Framework;
using UnityEngine;

namespace PicoGraffiti
{
    [Serializable]
    public class ScoreRepository
    {
        public string Version { get; private set; }
        public Identity Identity { get; private set; }
        public Score Score { get; private set; } = new Score();
        public Track CurrentTrack { get; private set; } = null;

        public ScoreRepository()
        {
            Version = Application.version;
            Identity = new Identity();
            var track = new Track(Identity.Get(), Score, WaveType.Square25);
            Score.Tracks.Add(track);
            CurrentTrack = track;
            
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Square25));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Square));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Noise));
            Score.Tracks.Add(new Track(Identity.Get(), Score, WaveType.Noise2));
        }

        public void SetNextTrack()
        {
            CurrentTrack = Score.Tracks[(Score.Tracks.IndexOf(CurrentTrack) + 1) % Score.Tracks.Count];
        }
    }
}