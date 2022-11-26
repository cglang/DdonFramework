using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Core.TestInvokeClasses;

namespace Test.Core;

public class TestCoreModule : Module
{
    public override void Load(IServiceCollection services, IConfiguration configuration)
    {
        Load<CoreModule>(services, configuration);
        services.AddTransient<TestClass>();
    }
}
