using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Utility
{
    internal class FixedSizeList<T> : List<T>
    {
        public int MaxSize { get; }

        public FixedSizeList(int size)
        {
            MaxSize = size;
        }

        public new void Add(T item)
        {
            if (Count >= MaxSize)
                RemoveAt(Count - 1);
            base.Add(item);
        }
    }
}
