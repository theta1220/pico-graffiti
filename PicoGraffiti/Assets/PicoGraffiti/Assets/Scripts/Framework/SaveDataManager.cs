using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;
using SFB;
using UnityEngine;

namespace PicoGraffiti.Framework
{
    public class SaveDataManager
    {
        public ScoreRepository Load()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Open", Application.persistentDataPath, "pg", false);
            if (paths.Length == 0) return null;
            var path = paths[0];

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var f = new BinaryFormatter();
                var scoreRepository = (ScoreRepository) f.Deserialize(fs);
                Migration(scoreRepository);
                fs.Close();
                return scoreRepository;
            }
        }

        public void Save(ScoreRepository scoreRepository, string defaultFileName =  "")
        {
            var path = "";
            if (string.IsNullOrEmpty(defaultFileName))
            {
                path = StandaloneFileBrowser.SaveFilePanel("Save", Application.persistentDataPath, defaultFileName, "pg");
                if (string.IsNullOrEmpty(path)) return;
            }
            else
            {
                path = $"{Application.persistentDataPath}/{defaultFileName}";
            }

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var f = new BinaryFormatter();
                f.Serialize(fs, scoreRepository);
                fs.Close();
            }
        }

        public async UniTask Export(ScoreRepository scoreRepository)
        {
            await UniTask.SwitchToMainThread();
            var path = StandaloneFileBrowser.SaveFilePanel("Save", Application.persistentDataPath, "", "wav");
            if (string.IsNullOrEmpty(path)) return;
            await UniTask.SwitchToThreadPool();
            
            await Wave.SaveAsync(scoreRepository.Score, path);
        }

        public void Migration(ScoreRepository scoreRepository)
        {
            int count = 0;
            foreach (var track in scoreRepository.Score.Tracks)
            {
                track.Wave = new Wave(track.Id);
                if (count == 0 || count == 1 || count == 2)
                {
                    track.OverrideWaveType = WaveType.Square;
                }

                if (count == 6)
                {
                    track.WaveType = WaveType.Triangle;
                    track.IsKick = true;
                }

                if (count == 5)
                {
                    track.IsChorus = true;
                }

                count++;
            }
        }
    }
}