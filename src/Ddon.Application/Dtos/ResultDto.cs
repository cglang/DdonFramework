using System;
using System.Collections.Generic;

namespace Ddon.Application.Dtos
{
    [Serializable]
    public class ResultDto<T>
    {
        public IReadOnlyList<T> Items
        {
            get { return _items ??= new List<T>(); }
            set { _items = value; }
        }

        private IReadOnlyList<T>? _items;

        public ResultDto() { }

        public ResultDto(IReadOnlyList<T> items)
        {
            _items = items;
        }
    }
}