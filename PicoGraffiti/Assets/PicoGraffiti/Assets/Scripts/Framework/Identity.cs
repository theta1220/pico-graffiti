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
            _count++;
            return _count;
        }

        public Identity DeepClone()
        {
            var obj = new Identity();
            obj._count = _count;
            return obj;
        }
    }
}