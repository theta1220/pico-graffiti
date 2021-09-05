using System;
using Cysharp.Threading.Tasks;
using PicoGraffiti.Framework;
using PicoGraffiti.Model;
using Tuna;
using UnityEngine;
using UnityEngine.Events;

namespace PicoGraffiti.UI
{
    public class UIHandler : IDisposable
    {
        public Tuna.Object<UIScore> UIScore { get; private set; }
        public Tuna.Object<UILines> UILines { get; private set; }

        private Transform _contentTransform = null;
        
        public UIHandler(Transform contentTransform)
        {
            _contentTransform = contentTransform;
        }

        public async UniTask InitializeAsync(int linesSplit, float height)
        {
            UILines = await Tuna.Object<UILines>.Create(_contentTransform);
            await UILines.Instance.InitializeAsync(height, linesSplit);
            UIScore = await Tuna.Object<UIScore>.Create(UILines.Instance.transform);
            await UIScore.Instance.InitializeAsync(height);
        }

        public void UpdateFrame()
        {
            UIScore.Instance.UpdateFrame();
            UILines.Instance.UpdateFrame();
        }

        public void Dispose()
        {
            UIScore.Dispose();
            UILines.Dispose();
        }
    }
}