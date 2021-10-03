using System;
using UnityEngine;
using UnityEngine.UI;

namespace PicoGraffiti.UI
{
    public class UIAppVersionText : MonoBehaviour
    {
        [SerializeField] private Text _text;

        public void Start()
        {
            _text.text = $"version {Application.version}";
        }
    }
}