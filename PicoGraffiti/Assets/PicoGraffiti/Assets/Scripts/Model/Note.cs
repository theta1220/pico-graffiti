using System;
using PicoGraffiti.Framework;

namespace PicoGraffiti.Model
{
    [Serializable]
    public class Note
    {
        public ulong Id = 0;
        public double Melo = -1;
        public double Vol = 1.0;
        public WaveType WaveType = WaveType.Square;

        public Note(ulong id)
        {
            Id = id;
        }
    }
}