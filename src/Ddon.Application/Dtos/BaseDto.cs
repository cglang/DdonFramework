using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Application.Dtos
{
    [Serializable]
    public abstract class BaseDto
    {
        public override string ToString()
        {
            return $"[DTO: {GetType().Name}]";
        }
    }

    [Serializable]
    public abstract class BaseDto<TKey> : BaseDto
    {
        [AllowNull]
        public TKey Id { get; set; }

        public override string ToString()
        {
            return $"[DTO: {GetType().Name}] Id = {Id}";
        }
    }
}