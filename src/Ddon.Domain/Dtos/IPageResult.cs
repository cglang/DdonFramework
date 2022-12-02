using System.Collections.Generic;

namespace Ddon.Domain.Dtos
{
    public interface IPageResult<T>
    {
        long Total { get; set; }

        IEnumerable<T> Items { get; set; }
    }
}
