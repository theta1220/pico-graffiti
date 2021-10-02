namespace Stocker.Framework
{
    public interface ICloneable<T>
    {
        T DeepClone();
    }
}