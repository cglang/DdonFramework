using Ddon.Core.Use.Di;
using System;

namespace Ddon.Core.Services.Guids
{
    public interface IGuidGenerator : ITransientDependency
    {
        Guid Create();
    }
}
