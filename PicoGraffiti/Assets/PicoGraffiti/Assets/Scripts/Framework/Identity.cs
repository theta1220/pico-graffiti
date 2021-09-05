using System;
using Stocker.Framework;

namespace PicoGraffiti.Framework
{
    [Serializable]
    public class Identity : ICloneable<Identity>
    {
        private ulong _count = 0;
        public ulong Get()
        {
            return _count;
        }

        public void Add()
        {
            _count++;
        }

        public Identity DeepClone()
        {
            var obj = new Identity();
            obj._count = _count;
            return obj;
        }
    }
}