using Cysharp.Threading.Tasks;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using PicoGraffiti.UI;
using Stocker.Framework;
using Tuna;
using UnityEngine;
using UnityEngine.Events;

namespace PicoGraffiti.Assets.Scripts
{
    public class ScoreHandler
    {
        public enum ScoreType
        {
            None,
            Melo,
            Volume,
        }

        public Version<ScoreRepository> ScoreRepository { get; private set; }
        public UIHandler UIHandler { get; private set; }
        public Tuna.Object<UIWavePlayer> UIWavePlayer { get; private set; }

        public int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public UnityEvent OnWrite = new UnityEvent();
        public UnityEvent OnErase = new UnityEvent();
        public UnityEvent<int> OnMove = new UnityEvent<int>();

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();
        private int _offset = 0;
        private bool _scoreApplyFlag = false;
        private ScoreType _scoreType = ScoreType.None;

        private UITrack _CurrentTrack =>
            UIHandler.UIScore.Instance.UITracks[ScoreRepository.Instance.CurrentTrack.Id]?.Instance;

        public ScoreHandler(ScoreType scoreType, Version<ScoreRepository> scoreRepository, UIHandler uiHandler,
            Tuna.Object<UIWavePlayer> uiWavePlayer)
        {
            _scoreType = scoreType;
            ScoreRepository = scoreRepository;
            UIHandler = uiHandler;
            UIWavePlayer = uiWavePlayer;
        }

        public async UniTask InitializeAsync()
        {
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
        }

        public void Dispose()
        {
            _subscribers.Dispose();
        }

        public void UpdateFrame()
        {
            UIHandler.UpdateFrame();

            if (_scoreApplyFlag)
            {
                ScoreApplyInternal();
                _scoreApplyFlag = false;
            }

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

            UpdateOffset();
        }

        public void OnWriteEvent(Vector2 pos)
        {
            var index = (int) (pos.x + _offset);
            var value = (double) pos.y / UIHandler.UIScore.Instance.Height;
            if (value < 0) value = 0;
            if (value > 1) value = 1;
            if (_scoreType == ScoreType.Melo)
            {
                ScoreRepository.Instance.CurrentTrack.SetNote(index, value);

                UIWavePlayer.Instance.OnWrite(value, ScoreRepository.Instance.CurrentTrack.WaveType);
            }
            else if (_scoreType == ScoreType.Volume)
            {
                ScoreRepository.Instance.CurrentTrack.SetNoteVolume(index, value);
            }

            OnWrite.Invoke();
            ScoreApply();
        }

        public void OnWriteOrEraseStartEvent(Vector2 pos)
        {
            ScoreRepository.Commit();
        }

        public void OnWriteOrEraseEndEvent(Vector2 pos)
        {
            if (_scoreType == ScoreType.Melo)
            {
                UIWavePlayer.Instance.OnWriteOff();
            }
        }

        public void OnEraseEvent(Vector2 pos)
        {
            var index = (int) (pos.x + _offset);
            if (_scoreType == ScoreType.Melo)
            {
                ScoreRepository.Instance.CurrentTrack.RemoveNote(index);
            }

            _CurrentTrack.Erase(index - _offset);
            OnErase.Invoke();
        }

        public void OnMoveEvent(Vector2 dir)
        {
            _offset -= (int) (dir.x - UIHandler.UIScore.Instance.PrevPos.x);
            if (_offset < 0) _offset = 0;
            OnMove.Invoke(_offset);
        }

        public void ScoreApply()
        {
            _scoreApplyFlag = true;
        }

        private void ScoreApplyInternal()
        {
            UIHandler.UIScore.Instance.Clear();
            foreach (var track in ScoreRepository.Instance.Score.Tracks)
            {
                var uiTrack = UIHandler.UIScore.Instance.UITracks[track.Id].Instance;
                for (var i = _offset; i < UIHandler.UIScore.Instance.Width + _offset; i++)
                {
                    if (track.Notes.ContainsKey(i))
                    {
                        var note = track.Notes[i];
                        if (_scoreType == ScoreType.Melo) uiTrack.Write(i - _offset, note.Melo);
                        else if (_scoreType == ScoreType.Volume) uiTrack.Write(i - _offset, note.Vol);
                    }
                }
            }

            UIHandler.UIScore.Instance.UpdateTexture();
            UIHandler.UILines.Instance.OnMove(_offset);
        }

        public void UpdateOffset()
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
            return (int) (UIWavePlayer.Instance.Index / len);
        }
    }
}