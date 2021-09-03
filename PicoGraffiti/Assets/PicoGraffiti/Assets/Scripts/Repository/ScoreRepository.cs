using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using Tuna.Framework;

namespace PicoGraffiti
{
    public class ScoreRepository
    {
        public Score Score { get; private set; } = new Score();
        public Track CurrentTrack { get; private set; } = null;

        public void Initialize()
        {
            var track = new Track(Score, WaveType.Square25);
            Score.Tracks.Add(track);
            CurrentTrack = track;
            
            Score.Tracks.Add(new Track(Score, WaveType.Square25));
            Score.Tracks.Add(new Track(Score, WaveType.Square));
            Score.Tracks.Add(new Track(Score, WaveType.Noise));
            Score.Tracks.Add(new Track(Score, WaveType.Noise2));
        }

        public void SetNextTrack()
        {
            CurrentTrack = Score.Tracks[(Score.Tracks.IndexOf(CurrentTrack) + 1) % Score.Tracks.Count];
        }
    }
}