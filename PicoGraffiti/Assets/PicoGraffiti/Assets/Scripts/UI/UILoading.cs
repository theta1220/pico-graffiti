using System.Windows.Forms;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PicoGraffiti.UI
{
    public class UILoading : MonoBehaviour
    {
        private int _referenceCount = 0;
        
        public async UniTask ShowAsync()
        {
            _referenceCount++;
            await UpdateViewAsync();
        }

        public async UniTask Hide()
        {
            _referenceCount--;
            await UpdateViewAsync();
        }

        public async UniTask UpdateViewAsync()
        {
            await UniTask.SwitchToMainThread();
            gameObject.SetActive(_referenceCount > 0);
            await UniTask.SwitchToThreadPool();
        }
    }
}