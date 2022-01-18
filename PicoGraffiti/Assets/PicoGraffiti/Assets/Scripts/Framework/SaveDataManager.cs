using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Model;
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

                // 主旋律
                if (count == 0)
                {
                    track.WaveType = WaveType.Square25;
                    track.OverrideWaveType = WaveType.Square;
                    track.OverrideWaveTime = 30;
                    foreach (var note in track.Notes)
                    {
                        note.Value.WaveType = WaveType.Square25;
                        note.Value.Vol = 0.75;
                        note.Value.Melo = (note.Value.Melo * 89 + 12) / 89;
                    }

                    track.Harmony = 1;
                }
                // ベース（コード）
                if (count == 1)
                {
                    track.IsCode = true;
                    track.Pan = 0;
                    track.WaveType = WaveType.Square125;
                    track.OverrideWaveType = WaveType.Square25;
                    foreach (var note in track.Notes)
                    {
                        note.Value.WaveType = WaveType.Square125;
                    }

                    track.Harmony = 4;
                }
                // ハモリ
                if (count == 2)
                {
                    track.WaveType = WaveType.Square;
                    track.OverrideWaveType = WaveType.Square;
                    track.OverrideWaveTime = 20;
                    foreach (var note in track.Notes)
                    {
                        note.Value.WaveType = WaveType.Square;
                    }

                    track.Harmony = 2;
                    track.Pan = 0.2;
                }
                // コード
                if (count == 3)
                {
                    track.WaveType = WaveType.Square125;
                    track.OverrideWaveType = WaveType.Square25;
                    
                    foreach (var note in track.Notes)
                    {
                        note.Value.WaveType = WaveType.Square125;
                        note.Value.Vol = 0.75;
                    }

                    track.Harmony = 4;
                    track.Pan = 0;
                }
                // ギター的なやつ２
                if (count == 4)
                {
                    track.Harmony = 3;
                    track.Pan = -0.3;
                }
                // アルペジオくん
                if (count == 5)
                {
                    track.Pan = -0.1;
                    foreach (var note in track.Notes)
                    {
                        note.Value.Vol = 0.75;
                    }
                    track.Harmony = 1;
                }
                // キック
                if (count == 6)
                {
                    track.WaveType = WaveType.Triangle;
                    track.OverrideWaveType = WaveType.Triangle;
                    foreach (var note in track.Notes)
                    {
                        note.Value.WaveType = WaveType.Triangle;
                    }
                    track.Harmony = 1;
                }
                // ノイズ
                if (count == 7)
                {
                    track.Harmony = 1;
                    track.Pan = 0.05;
                }
                //　ノイズ
                if (count == 8)
                {
                    track.Harmony = 1;
                    track.Pan = -0.1;
                }
                count++;
            }
        }
    }
}