using Ddon.Core;
using Ddon.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test.FileStorage;

public class TestFileStorageModule : Module
{
    public override void Load(IServiceCollection services, IConfiguration configuration)
    {
        Load<FileStorageModule>(services, configuration);
    }
}
