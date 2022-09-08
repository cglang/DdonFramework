using Ddon.Core;
using Ddon.Core.Use.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Core.TestInvokeClasses;

namespace Test.Core;

public class TestCoreModule : Module
{
    public override void Load(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDdonServiceInvoke, DdonServiceInvoke>();
        services.AddTransient<TestClass>();        
    }
}
