using Ddon.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Repository.Db;

namespace Test.Repository
{
    public class TestRepositoryModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("Test"));
        }
    }
}
