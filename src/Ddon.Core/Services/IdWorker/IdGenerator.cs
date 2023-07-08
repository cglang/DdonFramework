using System;

namespace Ddon.Core.Services.IdWorker
{
    public interface IIdGenerator
    {
        long CreateId();

        Guid CreateGuid();
    }
}
