using System;
using Cysharp.Threading.Tasks;
using Tuna;
using UnityEngine;
using UnityEngine.Events;

namespace PicoGraffiti.UI
{
    public class UIHandler : IDisposable
    {
        public Tuna.Object<UIMain> UIMain { get; private set; }
        public Tuna.Object<UIScore> UIScore { get; private set; }
        public Tuna.Object<UILines> UILines { get; private set; }

        public async UniTask InitializeAsync()
        {
            UIMain = await Tuna.Object<UIMain>.Create();
            UILines = await Tuna.Object<UILines>.Create(UIMain.Instance.Root);
            await UILines.Instance.InitializeAsync();
            UIScore = await Tuna.Object<UIScore>.Create(UIMain.Instance.Root);
            await UIScore.Instance.InitializeAsync();
        }

        public void UpdateFrame()
        {
            UIScore.Instance.UpdateFrame();
            UILines.Instance.UpdateFrame();
        }

        public void Dispose()
        {
            UIMain.Dispose();
            UIScore.Dispose();
        }
    }
}