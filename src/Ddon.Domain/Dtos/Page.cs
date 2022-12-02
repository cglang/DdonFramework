using Ddon.Domain.Entities;
using System;

namespace Ddon.Domain.Dtos
{
    public class Page
    {
        public virtual string Sorting { get; set; } = nameof(Entity<Guid>.Id);

        public virtual int Index { get; set; } = 1;

        public virtual int Size { get; set; } = 20;
    }
}
