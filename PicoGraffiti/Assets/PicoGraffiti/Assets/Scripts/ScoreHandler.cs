﻿using Cysharp.Threading.Tasks;
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

        public UIHandler UIHandler { get; private set; }
        public Tuna.Object<UIWavePlayer> UIWavePlayer { get; private set; }

        public float Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public UnityEvent OnWrite = new UnityEvent();
        public UnityEvent OnErase = new UnityEvent();
        public UnityEvent<float> OnMove = new UnityEvent<float>();

        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();
        private float _offset = 0;
        private bool _scoreApplyFlag = false;
        private ScoreType _scoreType = ScoreType.None;

        private UITrack _CurrentTrack =>
            UIHandler.UIScore.Instance.UITracks[AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.Id]?.Instance;

        public ScoreHandler(ScoreType scoreType, UIHandler uiHandler,
            Tuna.Object<UIWavePlayer> uiWavePlayer)
        {
            _scoreType = scoreType;
            UIHandler = uiHandler;
            UIWavePlayer = uiWavePlayer;
        }

        public async UniTask InitializeAsync()
        {
            for (var i = 0; i < AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks.Count; i++)
            {
                await UIHandler.UIScore.Instance.CreateTrackAsync(AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks[i]);
                UIHandler.UIScore.Instance.UITracks[AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks[i].Id].Instance
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
            UIHandler.UIScore.Instance.Clear();
            UIHandler.UpdateFrame();
            ScoreApplyInternal();

            // UITrack切り替え
            if (ExclusiveInput.GetKeyDown(KeyCode.Tab))
            {
                AppGlobal.Instance.ScoreRepository.Instance.SetNextTrack();
            }

            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha1)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(0);
            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha2)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(1);
            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha3)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(2);
            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha4)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(3);
            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha5)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(4);
            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha6)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(5);
            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha7)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(6);
            if (ExclusiveInput.GetKeyDown(KeyCode.Alpha8)) AppGlobal.Instance.ScoreRepository.Instance.SetCurrentTrack(7);
            
            UpdateOffset();
        }

        public void OnWriteEvent(Vector2 pos)
        {
            var index = (int) (pos.x + _offset);
            var value = (double) pos.y / UIHandler.UIScore.Instance.Height;
            if (value < 0) value = 0;
            if (value > 1) value = 1;

            // グリッドに沿う
            if (ExclusiveInput.GetKey(KeyCode.G) || ExclusiveInput.GetKey(KeyCode.F))
            {
                value = Mathf.Round((float)value * 89.0f) / 89.0;
            }
            // ノイズ用
            if (ExclusiveInput.GetKey(KeyCode.D))
            {
                if (Mathf.FloorToInt(index / (Track.NOTE_GRID_SIZE / 16.0f)) % 8 == 0)
                {
                    value = Mathf.Round((float)value * 89.0f) / 89.0;
                }
                else
                {
                    return;
                }
            }
            // コードアルペジオ
            if (ExclusiveInput.GetKey(KeyCode.F))
            {
                // var codes = new [] {"add9", "", "m", "", "m", "", "", "add9","", "m", "", "m-5"};
                // var pattern = new int[] {0, 1, 2, 1, 3, 1, 2, 1};
                // var patternIndex = 
                //     Mathf.FloorToInt(index / (UIHandler.UIScore.Instance.Width / 16.0f)) % pattern.Length;
                // var codeIndex = Mathf.RoundToInt((float)value * 89.0f) % codes.Length;
                // var code = CodeGetter.Get(codes[codeIndex]);
                // var add = code[pattern[patternIndex]];
                // if (add == -1) add = 12;
                // value = ((float) value * 89.0f + add) / 89;
            } 
            // 高速アルペジオ
            if (ExclusiveInput.GetKey(KeyCode.F))
            {
                if (Mathf.FloorToInt(index / (Track.NOTE_GRID_SIZE / 16.0f)) % 2 == 1)
                {
                    return;
                }
            }
            if (_scoreType == ScoreType.Melo)
            {
                AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.SetNote(index, value);

                UIWavePlayer.Instance.OnWrite(value, AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.WaveType);
            }
            else if (_scoreType == ScoreType.Volume)
            {
                AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.SetNoteVolume(index, value);
            }

            // パーティクルをたまに生成する
            if (Random.Range(0, 100) < 5)
            {
                UIHandler.UIScore.Instance.UITracks[AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.Id].Instance.CreateParticle((int)pos.x, (int)pos.y, true);
            }
            
            OnWrite.Invoke();
            ScoreApply();
        }

        public void OnWriteOrEraseStartEvent(Vector2 pos)
        {
            AppGlobal.Instance.ScoreRepository.Commit();

            if (_scoreType == ScoreType.Melo)
            {
                UIWavePlayer.Instance.OnWriteOn();
            }
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
                AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.RemoveNote(index);
            }

            _CurrentTrack.Erase(index - (int)_offset);
            OnErase.Invoke();
        }

        public void OnMoveEvent(Vector2 dir)
        {
            _offset -= dir.x - UIHandler.UIScore.Instance.PrevPos.x;
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
            foreach (var track in AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks)
            {
                var uiTrack = UIHandler.UIScore.Instance.UITracks[track.Id].Instance;
                for (var i = (int)_offset; i < UIHandler.UIScore.Instance.Width + (int)_offset; i++)
                {
                    if (track.Notes.ContainsKey(i))
                    {
                        var note = track.Notes[i];
                        if (_scoreType == ScoreType.Melo) uiTrack.Write(i - (int)_offset, note.Melo);
                        else if (_scoreType == ScoreType.Volume) uiTrack.Write(i - (int)_offset, note.Vol);

                        if (_scoreType == ScoreType.Melo && UIWavePlayer.Instance.IsPlaying && i - (int) _offset == 0)
                        {
                            for (var j = 0; j < 3; j++)
                            {
                                uiTrack.CreateParticle(0, (int)(uiTrack.Height * note.Melo), false);
                            }
                        }
                    }
                }
            }

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
            var bpmRate = 60.0 / (AppGlobal.Instance.ScoreRepository.Instance.Score.BPM * Track.NOTE_GRID_SIZE);
            var len = bpmRate * Wave.SAMPLE_RATE;
            return (int) (UIWavePlayer.Instance.Index / len);
        }
    }
}