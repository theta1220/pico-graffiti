using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
                fs.Close();
                return scoreRepository;
            }
        }

        public void Save(ScoreRepository scoreRepository)
        {
            var path = StandaloneFileBrowser.SaveFilePanel("Save", Application.persistentDataPath, "", "pg");
            if (string.IsNullOrEmpty(path)) return;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var f = new BinaryFormatter();
                f.Serialize(fs, scoreRepository);
                fs.Close();
            }
        }

        public void Export(ScoreRepository scoreRepository)
        {
            var path = StandaloneFileBrowser.SaveFilePanel("Save", Application.persistentDataPath, "", "wav");
            if (string.IsNullOrEmpty(path)) return;
            
            Wave.Save(scoreRepository.Score, path);
        }
    }
}