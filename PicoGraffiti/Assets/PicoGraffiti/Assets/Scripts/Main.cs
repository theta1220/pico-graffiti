using System;
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
            ResourceManager.Instance.Initialize();

            ScoreRepository = new Version<ScoreRepository>();
            SaveDataManager = new SaveDataManager();

            UIMain = await Tuna.Object<UIMain>.Create();

            UIScoreHandler = new UIHandler(UIMain.Instance.Content);
            await UIScoreHandler.InitializeAsync(Wave.MELO_NUM, 2500);
            UIVolumeHandler = new UIHandler(UIMain.Instance.VolumeRoot);
            await UIVolumeHandler.InitializeAsync(5, UIMain.Instance.VolumeRoot.GetComponent<RectTransform>().rect.height);

            UIWavePlayer = await Tuna.Object<UIWavePlayer>.Create();
            UIWavePlayer.Instance.Initialize(ScoreRepository);

            ScoreHandler = new ScoreHandler(ScoreHandler.ScoreType.Melo, ScoreRepository, UIScoreHandler, UIWavePlayer);
            await ScoreHandler.InitializeAsync();

            VolumeHandler = new ScoreHandler(ScoreHandler.ScoreType.Volume, ScoreRepository, UIVolumeHandler,
                UIWavePlayer);
            await VolumeHandler.InitializeAsync();

            BPM = await Tuna.Object<UIValue>.Create(UIMain.Instance.ScoreValuesRoot);
            BPM.Instance.Initialize("BPM", ScoreRepository.Instance.Score.BPM);
            BPM.Instance.OnEndEdit.Subscribe(
                    value => ScoreRepository.Instance.Score.BPM = (int) value).AddTo(_subscribers);
            
            Trans = await Tuna.Object<UIValue>.Create(UIMain.Instance.ScoreValuesRoot);
            Trans.Instance.Initialize("Trans", ScoreRepository.Instance.Score.Trans);
            Trans.Instance.OnEndEdit.Subscribe(
                value => ScoreRepository.Instance.Score.Trans = (int) value).AddTo(_subscribers);

            ScoreHandler.OnWrite.Subscribe(OnWriteEvent).AddTo(_subscribers);
            VolumeHandler.OnWrite.Subscribe(OnWriteEvent).AddTo(_subscribers);
            ScoreHandler.OnErase.Subscribe(OnEraseEvent).AddTo(_subscribers);
            VolumeHandler.OnErase.Subscribe(OnEraseEvent).AddTo(_subscribers);
            ScoreHandler.OnMove.Subscribe(OnMoveEvent).AddTo(_subscribers);
            VolumeHandler.OnMove.Subscribe(OnMoveEvent).AddTo(_subscribers);

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
            SaveDataManager.Save(ScoreRepository.Instance, "temp.pg");
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

            // ファイル
            if (ExclusiveInput.GetKeyDown(KeyCode.I))
            {
                ScoreRepository.SetInstance(SaveDataManager.Load());
                ScoreHandler.ScoreApply();
                BPM.Instance.SetValue(ScoreRepository.Instance.Score.BPM);
                Trans.Instance.SetValue(ScoreRepository.Instance.Score.Trans);
            }

            if (ExclusiveInput.GetKeyDown(KeyCode.O))
            {
                SaveDataManager.Save(ScoreRepository.Instance);
            }

            // エクスポート
            if (ExclusiveInput.GetKeyDown(KeyCode.E))
            {
                SaveDataManager.Export(ScoreRepository.Instance);
            }
            
            // Undo Redo
            if (ExclusiveInput.GetKeyDown(KeyCode.Z) &&
                (ExclusiveInput.GetKey(KeyCode.RightShift) || ExclusiveInput.GetKey(KeyCode.LeftShift)))
            {
                ScoreRepository.Redo();
                ScoreHandler.ScoreApply();
                VolumeHandler.ScoreApply();
            }
            else if (ExclusiveInput.GetKeyDown(KeyCode.Z))
            {
                ScoreRepository.Undo();
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
        }

        public long GetPlayOffset()
        {
            var bpmRate = 60.0 / (ScoreRepository.Instance.Score.BPM * Track.NOTE_GRID_SIZE);
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
    }
}