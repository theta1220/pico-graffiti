using System;
using PicoGraffiti.Framework;
using Stocker.Framework;

namespace PicoGraffiti.Model
{
    [Serializable]
    public class Note : ICloneable<Note>
    {
        public double Melo = -1;
        public double Vol = 0.75;
        public WaveType WaveType = WaveType.Square;

        public Note DeepClone()
        {
            var obj = new Note();
            obj.Melo = Melo;
            obj.Vol = Vol;
            obj.WaveType = WaveType;
            return obj;
        }
    }
}