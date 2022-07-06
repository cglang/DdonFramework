using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.Job
{
    public interface IDdonJob
    {
        Task Add(Job plan);

        Task Remove(Guid id);

        Task Update(Job plan);

        Task<Job?> Get(Guid id);

        Task<IEnumerable<Job>> All();
    }
}
