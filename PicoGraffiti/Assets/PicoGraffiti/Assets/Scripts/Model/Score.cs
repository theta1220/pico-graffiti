using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Stocker.Framework;

namespace PicoGraffiti.Model
{
    [Serializable]
    public class Score : ICloneable<Score>
    {
        public List<Track> Tracks { get; private set; } = new List<Track>();
        public int BPM { get; private set; } = 144;

        public int GetSize()
        {
            return Tracks.Max(track => track.GetSize());
        }

        public Score DeepClone()
        {
            var obj = new Score();
            foreach (var track in Tracks)
            {
                obj.Tracks.Add(track.DeepClone());
            }

            obj.BPM = BPM;
            return obj;
        }
    }
}