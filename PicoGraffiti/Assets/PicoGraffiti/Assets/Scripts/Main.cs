using System;
using System.Runtime.InteropServices;
using PicoGraffiti.Model;
using PicoGraffiti.UI;
using Tuna;
using Tuna.Framework;
using UnityEngine;

namespace PicoGraffiti.Assets.Scripts
{
    public class Main : MonoBehaviour
    {
        public ScoreRepository ScoreRepository { get; private set; }
        public UIHandler UIHandler { get; private set; }
        public Tuna.Object<UIWavePlayer> UIWavePlayer { get; private set; }

        private UITrack _CurrentTrack => UIHandler.UIScore.Instance.UITracks[ScoreRepository.CurrentTrack]?.Instance;

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();

        private bool _isSetupEnd = false;
        private int _offset = 0;
        private double _ink = 1.0;
        private double _inkDecay = 0;

        public async void Start()
        {
            ResourceManager.Instance.Initialize();

            ScoreRepository = new ScoreRepository();
            ScoreRepository.Initialize();

            UIHandler = new UIHandler();
            await UIHandler.InitializeAsync();

            UIWavePlayer = await Tuna.Object<UIWavePlayer>.Create();
            UIWavePlayer.Instance.Initialize();

            for (var i = 0; i < ScoreRepository.Score.Tracks.Count; i++)
            {
                await UIHandler.UIScore.Instance.CreateTrackAsync(ScoreRepository.Score.Tracks[i]);
                UIHandler.UIScore.Instance.UITracks[ScoreRepository.Score.Tracks[i]].Instance
                    .SetNoteColor(UIHandler.UIScore.Instance.NoteColors[i]);
            }
            
            UIHandler.UIScore.Instance.OnWriteEvent.Subscribe(OnWriteEvent).AddTo(_subscribers);
            UIHandler.UIScore.Instance.OnWriteEndEvent.Subscribe(OnWriteEndEvent).AddTo(_subscribers);
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
                ScoreRepository.SetNextTrack();
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
                        UIWavePlayer.Instance.Play(ScoreRepository.Score, 0);
                    }
                    else
                    {
                        UIWavePlayer.Instance.Play(ScoreRepository.Score, _offset * Track.NoteGridSize);
                    }
                }
            }
        }

        public void OnWriteEvent(Vector2 pos)
        {
            var index = (int) (pos.x + _offset);
            var melo = (double) pos.y / _CurrentTrack.Height;
            var vol = _ink;
            ScoreRepository.CurrentTrack.SetNote(index, melo, vol);
            _ink -= _inkDecay;
            if (_ink < 0) _ink = 0;
            _CurrentTrack.Erase(index - _offset);
            _CurrentTrack.Write(index - _offset, melo, vol);

            UIWavePlayer.Instance.OnWrite(melo, ScoreRepository.CurrentTrack.WaveType);
        }

        public void OnWriteEndEvent(Vector2 pos)
        {
            _ink = 1.0;
            UIWavePlayer.Instance.OnWriteOff();
        }

        public void OnEraseEvent(Vector2 pos)
        {
            var index = (int) (pos.x + _offset);
            ScoreRepository.CurrentTrack.RemoveNote(index);
            _CurrentTrack.Erase(index - _offset);
        }

        public void OnMoveEvent(Vector2 dir)
        {
            _offset -= (int) (dir.x - UIHandler.UIScore.Instance.PrevPos.x);
            if (_offset < 0) _offset = 0;

            foreach (var track in ScoreRepository.Score.Tracks)
            {
                var uiTrack = UIHandler.UIScore.Instance.UITracks[track].Instance;
                uiTrack.Clear();
                for (var i = _offset; i < _CurrentTrack.Width + _offset; i++)
                {
                    if (track.Notes.ContainsKey(i))
                    {
                        var note = track.Notes[i];
                        uiTrack.Write(i - _offset, note.Melo, note.Vol);
                    }
                }
            }

            UIHandler.UILines.Instance.OnMove(_offset);
        }
    }
}