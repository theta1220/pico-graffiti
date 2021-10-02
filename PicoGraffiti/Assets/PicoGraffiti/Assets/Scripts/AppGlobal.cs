using System;
using Stocker.Framework;
using Tuna.Framework;

namespace PicoGraffiti
{
    public class AppGlobal : Singleton<AppGlobal>
    {
        public Version<ScoreRepository> ScoreRepository { get; private set; }

        public void Initialize()
        {
            ScoreRepository = new Version<ScoreRepository>();
        }
    }
}