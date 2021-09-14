using Tuna;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace PicoGraffiti.UI
{
    public class UIValue : TunaBehaviour
    {
        [SerializeField] private Text _name = default;
        [SerializeField] private InputField _input = default;

        public float Value
        {
            get
            {
                var value = 0f;
                if (!float.TryParse(_input.text, out value))
                {
                    return 0;
                }

                return value;
            }
        }
        public int IntValue => (int) Value;
        public UnityEvent<float> OnEndEdit = new UnityEvent<float>();
        
        private TunaCompositeDisposable _disposable = TunaCompositeDisposable.Create();

        public void Initialize(string name, float value)
        {
            _name.text = name;
            SetValue(value);
            
            _input.onEndEdit.RemoveAllListeners();
            _input.onEndEdit.Subscribe(str =>
            {
                var value = 0.0f;
                if (!float.TryParse(str, out value))
                {
                    SetValue(Value);
                    return;
                }
                SetValue(value);
                OnEndEdit.Invoke(value);
            }).AddTo(_disposable);
        }

        public void SetValue(float value)
        {
            _input.SetTextWithoutNotify($"{value:0.00}");
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}