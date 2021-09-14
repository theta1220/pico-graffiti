
namespace PicoGraffiti.Framework
{
    public static class CodeGetter
    {
        public static int[] Get(string codeName, int rotate = 0)
        {
            // デフォルトはメジャー
            int first = 0;
            int second = 4;
            int third = 7;
            int fourth = -1;

            if (codeName.Contains("m"))
            {
                second = 3;
            }
            if (codeName.Contains("sus4"))
            {
                second = 5;
            }
            if (codeName.Contains("add9"))
            {
                second = 2;
            }
            if (codeName.Contains("aug"))
            {
                third = 8;
            }
            if (codeName.Contains("5"))
            {
                third = 6;
            }
            if (codeName.Contains("6"))
            {
                fourth = 9;
            }
            if (codeName.Contains("7"))
            {
                fourth = 10;
            }
            if (codeName.Contains("M"))
            {
                fourth = 11;
            }
            if (codeName.Contains("dim"))
            {
                second = 3;
                third = 6;
                fourth = 9;
            }

            if (codeName.Contains("oct"))
            {
                second = 12;
            }

            var arr = new int[] { first, second, third, fourth };

            if (rotate == 1)
            {
                arr = new int[] { first, second, third, fourth - 12 };
            }
            if (rotate == 2)
            {
                arr = new int[] { first, second, third - 12, fourth - 12 };
            }
            if (rotate == 3)
            {
                arr = new int[] { first, second - 12, third - 12, fourth - 12 };
            }

            return arr;
        }

        static T[] Rotate<T>(T[] xs, int n)
        {
            var ret = new T[xs.Length];
            if (ret.Length == 0) return ret;

            int m = n % ret.Length;
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = xs[m++];
                if (m == ret.Length) m = 0;
            }
            return ret;
        }
    }
}