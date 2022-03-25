using System;
using System.Collections.Generic;

namespace Ddon.Core.Models
{
    [Serializable]
    public class PageResult<T> : ResultBase<T>
    {
        public long TotalCount { get; set; }

        public PageResult()
        {

        }

        public PageResult(long totalCount, IReadOnlyList<T> items) : base(items)
        {
            TotalCount = totalCount;
        }
    }
}