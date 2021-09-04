﻿using System;
using UnityEditorInternal.Profiling.Memory.Experimental;

namespace PicoGraffiti.Framework
{
    [Serializable]
    public class Identity
    {
        private ulong _count = 0;

        public ulong Get()
        {
            _count++;
            return _count;
        }
    }
}