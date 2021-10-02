using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelScreen
{
    public class ImageEffecter : MonoBehaviour
    {
        [SerializeField] Material _mat;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, _mat);
        }
    }
}