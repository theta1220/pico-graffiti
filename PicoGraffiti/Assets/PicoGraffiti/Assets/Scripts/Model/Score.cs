using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace PicoGraffiti.Model
{
    public class Score
    {
        public List<Track> Tracks { get; private set; } = new List<Track>();
        public int BPM { get; private set; } = 144;

        public int GetSize()
        {
            return Tracks.Max(track => track.GetSize());
        }
    }
}