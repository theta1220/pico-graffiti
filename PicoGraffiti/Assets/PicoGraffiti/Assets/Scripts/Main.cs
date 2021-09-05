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

        private bool _isSetupEnd = false;
        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();

        public async void Start()
        {
            ResourceManager.Instance.Initialize();

            ScoreRepository = new Version<ScoreRepository>();
            SaveDataManager = new SaveDataManager();

            UIMain = await Tuna.Object<UIMain>.Create();

            UIScoreHandler = new UIHandler(UIMain.Instance.Content);
            await UIScoreHandler.InitializeAsync(Wave.MELO_NUM, 2000);
            UIVolumeHandler = new UIHandler(UIMain.Instance.VolumeRoot);
            await UIVolumeHandler.InitializeAsync(10, UIMain.Instance.VolumeRoot.GetComponent<RectTransform>().rect.height);

            UIWavePlayer = await Tuna.Object<UIWavePlayer>.Create();
            UIWavePlayer.Instance.Initialize();

            ScoreHandler = new ScoreHandler(ScoreHandler.ScoreType.Melo, ScoreRepository, UIScoreHandler, UIWavePlayer);
            await ScoreHandler.InitializeAsync();

            VolumeHandler = new ScoreHandler(ScoreHandler.ScoreType.Volume, ScoreRepository, UIVolumeHandler,
                UIWavePlayer);
            await VolumeHandler.InitializeAsync();

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
            if (Input.GetKeyDown(KeyCode.I))
            {
                ScoreRepository.SetInstance(SaveDataManager.Load());
                ScoreHandler.ScoreApply();
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

        public void OnMoveEvent(int offset)
        {
            ScoreHandler.Offset = offset;
            VolumeHandler.Offset = offset;
            
            ScoreHandler.ScoreApply();
            VolumeHandler.ScoreApply();
        }
    }
}