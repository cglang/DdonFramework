using Ddon.Core;
using Ddon.Core.Services.Guids;
using Ddon.Repository;
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
            Load<RepositoryModule<TestDbContext>, DbContextOptionsBuilder>(services, configuration, options => options.UseInMemoryDatabase("Test"));
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("Test"));

            services.Configure<SequentialGuidGeneratorOptions>(options =>
            {
                options.DefaultSequentialGuidType = SequentialGuidType.SequentialAsString;
            });
        }
    }
}
