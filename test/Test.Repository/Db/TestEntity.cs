using Ddon.Domain.Entities.Auditing;
using System;

namespace Test.Repository.Db
{
    public class TestEntity : AuditedAggregateRoot<Guid>
    {
        public string? Title { get; set; }
    }
}
