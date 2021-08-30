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
            var track = new Track();
            Score.Tracks.Add(track);
            CurrentTrack = track;
        }
    }
}