using UnityEngine;

namespace PicoGraffiti.UI
{
    public class TextureBuffer
    {
        public Color[] Buffer;
        public int Width = 0;
        public int Height = 0;

        public TextureBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Buffer = new Color[Width * Height];
        }
            
        public bool Draw(int x, int y, ref Color color)
        {
            var index = x + y * Width;
            if (x >= Width) return false;
            if (index < 0 || index >= Buffer.Length) return false;
            if (x < 0 || y < 0) return false;
                
            Buffer[index].r = color.r;
            Buffer[index].g = color.g;
            Buffer[index].b = color.b;
            Buffer[index].a = 1;

            return true;
        }
            
        public void Clear()
        {
            var len =  Buffer.Length;
            for (var i = 0; i < len; i++)
            {
                Buffer[i].a = 0;
            }
        }
    }
}