using Ddon.Core.DependencyInjection;
using System;

namespace Ddon.Core.Services.Guids
{
    public interface IGuidGenerator : ITransientDependency
    {
        Guid Create();
    }
}
