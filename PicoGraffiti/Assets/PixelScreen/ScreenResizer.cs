using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelScreen
{
    public class ScreenResizer : MonoBehaviour
    {
        [SerializeField] Vector2Int _targetScreenSize = new Vector2Int(320, 180);
        [SerializeField] int _scale = 3;
        void Awake()
        {
            Screen.SetResolution(_targetScreenSize.x * _scale, _targetScreenSize.y * _scale, false);
            // Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}