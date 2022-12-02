using System.Collections.Generic;

namespace Ddon.Domain.Dtos
{
    public class PageResult<T> : IPageResult<T>
    {
        public long Total { get; set; }

        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
