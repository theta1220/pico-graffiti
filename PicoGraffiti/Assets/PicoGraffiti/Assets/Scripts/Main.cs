using System;
using PicoGraffiti.Model;
using PicoGraffiti.UI;
using Tuna;
using Tuna.Framework;
using UnityEngine;

namespace PicoGraffiti.Assets.Scripts
{
    public class Main : MonoBehaviour
    {
        public ResourceManager ResourceManager { get; private set; }
        public ScoreRepository ScoreRepository { get; private set; }
        public UIHandler UIHandler { get; private set; }
        
        private TunaCompositeDisposable _subscribers = TunaCompositeDisposable.Create();

        private bool _isSetupEnd = false;

        public async void Start()
        {
            ResourceManager.Instance.Initialize();
            ScoreRepository = new ScoreRepository();
            ScoreRepository.Initialize();
            UIHandler = new UIHandler();
            await UIHandler.InitializeAsync();
            UIHandler.OnPointerEvent.Subscribe(OnPointerEvent).AddTo(_subscribers);

            await UIHandler.UIScore.Instance.CreateTrackAsync(ScoreRepository.CurrentTrack);
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
        }

        public void OnPointerEvent(Vector2 pos)
        {
            var index = (int)pos.x;
            var melo = (double)pos.y / UIHandler.UIScore.Instance.UITracks[ScoreRepository.CurrentTrack].Instance.Height;
            ScoreRepository.CurrentTrack.SetNote(index, melo);
            
            UIHandler.UIScore.Instance.UITracks[ScoreRepository.CurrentTrack]?.Instance.Write(index, melo);
        }
    }
}