using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using PicoGraffiti.UI;
using Stocker.Framework;
using Tuna;
using Tuna.Framework;
using UnityEngine;

namespace PicoGraffiti.Assets.Scripts
{
    public class Main : MonoBehaviour
    {
        public Stocker.Framework.Version<ScoreRepository> ScoreRepository { get; private set; }
        public UIHandler UIHandler { get; private set; }
        public Tuna.Object<UIWavePlayer> UIWavePlayer { get; private set; }
        public SaveDataManager SaveDataManager { get; private set; }

        private UITrack _CurrentTrack =>
            UIHandler.UIScore.Instance.UITracks[ScoreRepository.Instance.CurrentTrack.Id]?.Instance;

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();

        private bool _isSetupEnd = false;
        private int _offset = 0;
        private double _ink = 1.0;
        private double _inkDecay = 0;

        public async void Start()
        {
            ResourceManager.Instance.Initialize();

            ScoreRepository = new Version<ScoreRepository>();
            SaveDataManager = new SaveDataManager();

            UIHandler = new UIHandler();
            await UIHandler.InitializeAsync();

            UIWavePlayer = await Tuna.Object<UIWavePlayer>.Create();
            UIWavePlayer.Instance.Initialize();

            for (var i = 0; i < ScoreRepository.Instance.Score.Tracks.Count; i++)
            {
                await UIHandler.UIScore.Instance.CreateTrackAsync(ScoreRepository.Instance.Score.Tracks[i]);
                UIHandler.UIScore.Instance.UITracks[ScoreRepository.Instance.Score.Tracks[i].Id].Instance
                    .SetNoteColor(UIHandler.UIScore.Instance.NoteColors[i]);
            }

            UIHandler.UIScore.Instance.OnWriteEvent.Subscribe(OnWriteEvent).AddTo(_subscribers);
            UIHandler.UIScore.Instance.OnWriteOrEraseStartEvent.Subscribe(OnWriteOrEraseStartEvent).AddTo(_subscribers);
            UIHandler.UIScore.Instance.OnWriteOrEraseEndEvent.Subscribe(OnWriteOrEraseEndEvent).AddTo(_subscribers);
            UIHandler.UIScore.Instance.OnEraseEvent.Subscribe(OnEraseEvent).AddTo(_subscribers);
            UIHandler.UIScore.Instance.OnMoveEvent.Subscribe(OnMoveEvent).AddTo(_subscribers);

            _isSetupEnd = true;
        }

        public void OnDestroy()
        {
            _subscribers.Dispose();
            UIHandler.Dispose();
        }

        public void Update()
        {
            if (!_isSetupEnd)
            {
                return;
            }

            UIHandler.UpdateFrame();

            // UITrack切り替え
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ScoreRepository.Instance.SetNextTrack();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) ScoreRepository.Instance.SetCurrentTrack(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ScoreRepository.Instance.SetCurrentTrack(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ScoreRepository.Instance.SetCurrentTrack(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ScoreRepository.Instance.SetCurrentTrack(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ScoreRepository.Instance.SetCurrentTrack(4);

            // Undo Redo
            if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
            {
                ScoreRepository.Redo();
                ScoreApply();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                ScoreRepository.Undo();
                ScoreApply();
            }

            // ファイル
            if (Input.GetKeyDown(KeyCode.I))
            {
                ScoreRepository.SetInstance(SaveDataManager.Load());
                ScoreApply();
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                SaveDataManager.Save(ScoreRepository.Instance);
            }

            // エクスポート
            if (Input.GetKeyDown(KeyCode.E))
            {
                SaveDataManager.Export(ScoreRepository.Instance);
            }

            // 再生
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (UIWavePlayer.Instance.IsPlaying)
                {
                    UIWavePlayer.Instance.Stop();
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        UIWavePlayer.Instance.Play(ScoreRepository.Instance.Score, 0);
                    }
                    else
                    {
                        UIWavePlayer.Instance.Play(ScoreRepository.Instance.Score, GetPlayOffset());
                    }
                }
            }
            
            UpdateOffset();
        }

        public void OnWriteEvent(Vector2 pos)
        {
            var index = (int) (pos.x + _offset);
            var melo = (double) pos.y / UITrack.Height;
            var vol = _ink;
            ScoreRepository.Instance.CurrentTrack.SetNote(ScoreRepository.Instance.Identity.Get(), index, melo, vol);
            _ink -= _inkDecay;
            if (_ink < 0) _ink = 0;
            _CurrentTrack.Erase(index - _offset);
            _CurrentTrack.Write(index - _offset, melo, vol);
            UIHandler.UIScore.Instance.UpdateTexture();

            UIWavePlayer.Instance.OnWrite(melo, ScoreRepository.Instance.CurrentTrack.WaveType);
        }

        public void OnWriteOrEraseStartEvent(Vector2 pos)
        {
            ScoreRepository.Commit();
        }

        public void OnWriteOrEraseEndEvent(Vector2 pos)
        {
            _ink = 1.0;
            UIWavePlayer.Instance.OnWriteOff();
        }

        public void OnEraseEvent(Vector2 pos)
        {
            var index = (int) (pos.x + _offset);
            ScoreRepository.Instance.CurrentTrack.RemoveNote(index);
            _CurrentTrack.Erase(index - _offset);
        }

        public void OnMoveEvent(Vector2 dir)
        {
            _offset -= (int) (dir.x - UIHandler.UIScore.Instance.PrevPos.x);
            if (_offset < 0) _offset = 0;
            ScoreApply();
            UIHandler.UILines.Instance.OnMove(_offset);
        }

        public void  UpdateOffset()
        {
            if (!UIWavePlayer.Instance.IsPlaying) return;

            _offset = GetPlayingOffset();
            ScoreApply();
            UIHandler.UILines.Instance.OnMove(_offset);
        }

        public int GetPlayingOffset()
        {
            var bpmRate = 60.0 / (ScoreRepository.Instance.Score.BPM * Track.NOTE_GRID_SIZE);
            var len = bpmRate * Wave.SAMPLE_RATE;
            return (int)(UIWavePlayer.Instance.Index / len);
        }

        public long GetPlayOffset()
        {
            var bpmRate = 60.0 / (ScoreRepository.Instance.Score.BPM * Track.NOTE_GRID_SIZE);
            var len = bpmRate * Wave.SAMPLE_RATE;
            return (long)(_offset * len);
        }

        public void ScoreApply()
        {
            UITrack.Clear();
            foreach (var track in ScoreRepository.Instance.Score.Tracks)
            {
                var uiTrack = UIHandler.UIScore.Instance.UITracks[track.Id].Instance;
                for (var i = _offset; i < UITrack.Width + _offset; i++)
                {
                    if (track.Notes.ContainsKey(i))
                    {
                        var note = track.Notes[i];
                        uiTrack.Write(i - _offset, note.Melo, note.Vol);
                    }
                }
            }

            UIHandler.UIScore.Instance.UpdateTexture();
        }
    }
}