using Ddon.Cache;
using Ddon.Core;
using Ddon.Repository.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ddon.Repository
{
    public class RepositoryModule<TDbContext> : Module<DbContextOptionsBuilder>
        where TDbContext : DbContext
    {
        public override void Load(IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder> optionBuilder)
        {
            Load<CoreModule>(services, configuration);
            Load<CacheModule>(services, configuration);

            Load<RepositoryModule<DbContext>>(services, configuration, (s) => { });

            services.AddTransient<IUnitOfWork<TDbContext>, UnitOfWork<TDbContext>>();
            services.AddDbContext<TDbContext>(RepositoryModuleOptions.OptionsBuilder);
        }
    }
}
