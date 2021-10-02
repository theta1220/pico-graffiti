using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;

namespace Stocker.Extension
{
    public static class Binary
    {
        public static MemoryStream Serialize<T>(this T src) where T : class
        {
            var memoryStream = new MemoryStream();
            var binaryFormatter
                = new System.Runtime.Serialization
                    .Formatters.Binary.BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, src); // シリアライズ
            return memoryStream;
        }

        public static T Deserialize<T>(this MemoryStream memoryStream) where T : class
        {
            var binaryFormatter = new BinaryFormatter();
            memoryStream.Seek(0, SeekOrigin.Begin);
            var obj = (T)binaryFormatter.Deserialize(memoryStream);
            return obj;
        }
    }
}