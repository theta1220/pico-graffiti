using System.Collections.Generic;
using System.Linq;

namespace Stocker.Framework
{
    public class QueueStack<T> : List<T>
    {
        public T PopBack()
        {
            var obj = this.Last();
            Remove(obj);
            return obj;
        }

        public void PushBack(T obj)
        {
            Add(obj);
        }

        public T PopFront()
        {
            var obj = this.First();
            Remove(obj);
            return obj;
        }

        public void PushFront(T obj)
        {
            Insert(0, obj);
        }
    }
}