using System.Collections.Generic;

namespace Ddon.Domain.Dtos
{
    public class PageResult<T>
    {
        public long Total { get; set; }

        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
