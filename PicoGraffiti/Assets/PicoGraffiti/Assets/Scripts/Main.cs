using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using PicoGraffiti.UI;
using Stocker.Framework;
using Tuna;
using Tuna.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PicoGraffiti.Assets.Scripts
{
    public class Main : MonoBehaviour
    {
        public Tuna.Object<UIMain> UIMain { get; private set; }
        public UIHandler UIScoreHandler { get; private set; }
        public UIHandler UIVolumeHandler { get; private set; }
        public ScoreHandler ScoreHandler { get; private set; }
        public ScoreHandler VolumeHandler { get; private set; }
        public Tuna.Object<UIWavePlayer> UIWavePlayer { get; private set; }
        public SaveDataManager SaveDataManager { get; private set; }
        public Tuna.Object<UIValue> BPM { get; private set; }
        public Tuna.Object<UIValue> Trans { get; private set; }

        private bool _isSetupEnd = false;
        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();

        public async void Start()
        {
            await UniTask.SwitchToMainThread();
            ResourceManager.Instance.Initialize();
            AppGlobal.Instance.Initialize();

            SaveDataManager = new SaveDataManager();

            UIMain = await Tuna.Object<UIMain>.Create();

            UIScoreHandler = new UIHandler(UIMain.Instance.Content);
            await UIScoreHandler.InitializeAsync(Wave.MELO_NUM, 2500/4);
            UIVolumeHandler = new UIHandler(UIMain.Instance.VolumeRoot);
            await UIVolumeHandler.InitializeAsync(5, UIMain.Instance.VolumeRoot.GetComponent<RectTransform>().sizeDelta.y);

            UIWavePlayer = await Tuna.Object<UIWavePlayer>.Create();
            await UIWavePlayer.Instance.InitializeAsync(UIMain.Instance.Monitor);

            ScoreHandler = new ScoreHandler(ScoreHandler.ScoreType.Melo, UIScoreHandler, UIWavePlayer);
            await ScoreHandler.InitializeAsync();

            VolumeHandler = new ScoreHandler(ScoreHandler.ScoreType.Volume, UIVolumeHandler,
                UIWavePlayer);
            await VolumeHandler.InitializeAsync();

            BPM = await Tuna.Object<UIValue>.Create(UIMain.Instance.ScoreValuesRoot);
            BPM.Instance.Initialize("BPM", AppGlobal.Instance.ScoreRepository.Instance.Score.BPM);
            BPM.Instance.OnEndEdit.Subscribe(
                    value => AppGlobal.Instance.ScoreRepository.Instance.Score.BPM = (int) value).AddTo(_subscribers);
            
            Trans = await Tuna.Object<UIValue>.Create(UIMain.Instance.ScoreValuesRoot);
            Trans.Instance.Initialize("KEY", AppGlobal.Instance.ScoreRepository.Instance.Score.Trans);
            Trans.Instance.OnEndEdit.Subscribe(
                value => AppGlobal.Instance.ScoreRepository.Instance.Score.Trans = (int) value).AddTo(_subscribers);

            ScoreHandler.OnWrite.Subscribe(OnWriteEvent).AddTo(_subscribers);
            VolumeHandler.OnWrite.Subscribe(OnWriteEvent).AddTo(_subscribers);
            ScoreHandler.OnErase.Subscribe(OnEraseEvent).AddTo(_subscribers);
            VolumeHandler.OnErase.Subscribe(OnEraseEvent).AddTo(_subscribers);
            ScoreHandler.OnMove.Subscribe(OnMoveEvent).AddTo(_subscribers);
            VolumeHandler.OnMove.Subscribe(OnMoveEvent).AddTo(_subscribers);
            ScoreHandler.OnSlide.Subscribe(OnSlideEvent).AddTo(_subscribers);
            VolumeHandler.OnSlide.Subscribe(OnSlideEvent).AddTo(_subscribers);

            _isSetupEnd = true;
        }

        public void OnDestroy()
        {
            _subscribers.Dispose();
            UIScoreHandler.Dispose();
            UIVolumeHandler.Dispose();
            UIWavePlayer.Dispose();
            ScoreHandler.Dispose();
            BPM.Dispose();
            Trans.Dispose();
        }

        public void OnApplicationQuit()
        {
            SaveDataManager.Save(AppGlobal.Instance.ScoreRepository.Instance, "temp.pg");
        }

        public void Update()
        {
            if (!_isSetupEnd)
            {
                return;
            }

            ScoreHandler.UpdateFrame();
            VolumeHandler.UpdateFrame();
            UIScoreHandler.UpdateFrame();
            UIVolumeHandler.UpdateFrame();
            UIWavePlayer.Instance.UpdateFrame();

            // ファイル
            if (ExclusiveInput.GetKeyDown(KeyCode.I))
            {
                AppGlobal.Instance.ScoreRepository.SetInstance(SaveDataManager.Load());
                ScoreHandler.Offset = 0;
                VolumeHandler.Offset = 0;
                ScoreHandler.ScoreApply();
                VolumeHandler.ScoreApply();
                BPM.Instance.SetValue(AppGlobal.Instance.ScoreRepository.Instance.Score.BPM);
                Trans.Instance.SetValue(AppGlobal.Instance.ScoreRepository.Instance.Score.Trans);
            }

            if (ExclusiveInput.GetKeyDown(KeyCode.O))
            {
                SaveDataManager.Save(AppGlobal.Instance.ScoreRepository.Instance);
            }

            // エクスポート
            if (ExclusiveInput.GetKeyDown(KeyCode.E))
            {
                UniTask.Run(async () =>
                {
                    await UIMain.Instance.Loading.ShowAsync();
                    await SaveDataManager.Export(AppGlobal.Instance.ScoreRepository.Instance);
                    await UIMain.Instance.Loading.Hide();
                }).Forget();
            }
            
            // Undo Redo
            if (ExclusiveInput.GetKeyDown(KeyCode.Z) &&
                (ExclusiveInput.GetKey(KeyCode.RightShift) || ExclusiveInput.GetKey(KeyCode.LeftShift)))
            {
                AppGlobal.Instance.ScoreRepository.Redo();
                ScoreHandler.ScoreApply();
                VolumeHandler.ScoreApply();
            }
            else if (ExclusiveInput.GetKeyDown(KeyCode.Z))
            {
                AppGlobal.Instance.ScoreRepository.Undo();
                ScoreHandler.ScoreApply();
                VolumeHandler.ScoreApply();
            }

            // 再生
            if (ExclusiveInput.GetKeyDown(KeyCode.Space))
            {
                if (UIWavePlayer.Instance.IsPlaying)
                {
                    UIWavePlayer.Instance.Stop();
                }
                else
                {
                    if (ExclusiveInput.GetKey(KeyCode.LeftShift) ||
                        ExclusiveInput.GetKey(KeyCode.RightShift))
                    {
                        UIWavePlayer.Instance.Play(0);
                    }
                    else
                    {
                        UIWavePlayer.Instance.Play(GetPlayOffset());
                    }
                }
            }

            if (ExclusiveInput.GetKeyDown(KeyCode.RightArrow))
            {
                ScoreHandler.Offset += UIScoreHandler.UIScore.Instance.Width;
                ScoreHandler.ScoreApply();
                VolumeHandler.ScoreApply();
                if (UIWavePlayer.Instance.IsPlaying)
                {
                    UIWavePlayer.Instance.Play(GetPlayOffset());
                }
            }
            if (ExclusiveInput.GetKeyDown(KeyCode.LeftArrow))
            {
                ScoreHandler.Offset -= UIScoreHandler.UIScore.Instance.Width;
                ScoreHandler.ScoreApply();
                VolumeHandler.ScoreApply();
                if (UIWavePlayer.Instance.IsPlaying)
                {
                    UIWavePlayer.Instance.Play(GetPlayOffset());
                }
            }

            if (ExclusiveInput.GetKeyDown(KeyCode.N))
            {
                SaveDataManager.Save(AppGlobal.Instance.ScoreRepository.Instance, "temp.pg");
                AppGlobal.Instance.Initialize();
                ScoreHandler.Offset = 0;
                VolumeHandler.Offset = 0;
                ScoreHandler.ScoreApply();
                VolumeHandler.ScoreApply();
            }

            if (ExclusiveInput.GetKey(KeyCode.RightShift) || ExclusiveInput.GetKey(KeyCode.LeftShift))
            {
                if (ExclusiveInput.GetKeyDown(KeyCode.DownArrow))
                {
                    foreach (var note in AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.Notes)
                    {
                        note.Value.Melo = (note.Value.Melo * 89 - 12) / 89;
                    }
                    ScoreHandler.ScoreApply();
                }
                if (ExclusiveInput.GetKeyDown(KeyCode.UpArrow))
                {
                    foreach (var note in AppGlobal.Instance.ScoreRepository.Instance.CurrentTrack.Notes)
                    {
                        note.Value.Melo = (note.Value.Melo * 89 + 12) / 89;
                    }
                    ScoreHandler.ScoreApply();
                }
            }
        }

        public long GetPlayOffset()
        {
            var bpmRate = 60.0 / (AppGlobal.Instance.ScoreRepository.Instance.Score.BPM * Track.NOTE_GRID_SIZE);
            var len = bpmRate * Wave.SAMPLE_RATE;
            return (long) (ScoreHandler.Offset * len);
        }

        public void OnWriteEvent()
        {
            ScoreHandler.ScoreApply();
            VolumeHandler.ScoreApply();
        }

        public void OnEraseEvent()
        {
            ScoreHandler.ScoreApply();
            VolumeHandler.ScoreApply();
        }

        public void OnMoveEvent(float offset)
        {
            ScoreHandler.Offset = offset;
            VolumeHandler.Offset = offset;
            
            ScoreHandler.ScoreApply();
            VolumeHandler.ScoreApply();
        }

        public void OnSlideEvent(float x, float move)
        {
            ScoreHandler.ScoreApply();
            VolumeHandler.ScoreApply();
        }
    }
}