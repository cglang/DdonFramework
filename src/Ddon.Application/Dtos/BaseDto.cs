using System.Diagnostics.CodeAnalysis;

namespace Ddon.Application.Dtos
{
    public class BaseDto<TKey>
    {
        [AllowNull]
        public TKey Id { get; set; }
    }
}