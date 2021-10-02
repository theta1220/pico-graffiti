using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelScreen
{
    public class AnalogPixel : MonoBehaviour
    {
        [SerializeField] Material _mat;

        void Update()
        {
            _mat.SetInt("_RandomSeed", (int)(Random.value * 1000));
        }
    }
}