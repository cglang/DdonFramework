using System.Diagnostics.CodeAnalysis;

namespace Ddon.Application.Dtos
{
    public class BaseDto
    {
        public override string ToString()
        {
            return $"[DTO: {GetType().Name}]";
        }
    }

    public class BaseDto<TKey> : BaseDto
    {
        [AllowNull]
        public TKey Id { get; set; }

        public override string ToString()
        {
            return $"[DTO: {GetType().Name}] Id = {Id}";
        }
    }
}