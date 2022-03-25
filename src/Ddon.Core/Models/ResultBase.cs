using System;
using System.Collections.Generic;

namespace Ddon.Core.Models
{
    [Serializable]
    public class ResultBase<T>
    {
        public IReadOnlyList<T> Items
        {
            get { return _items ??= new List<T>(); }
            set { _items = value; }
        }

        private IReadOnlyList<T>? _items;

        public ResultBase() { }

        public ResultBase(IReadOnlyList<T> items)
        {
            _items = items;
        }
    }
}