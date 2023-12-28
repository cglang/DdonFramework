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
        public override void Load(IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder>? optionBuilder)
        {
            Load<CoreModule>(services, configuration);

            services.AddScoped<IUnitOfWork<TDbContext>, UnitOfWork<TDbContext>>();
            services.AddDbContext<TDbContext>(optionBuilder);
        }
    }
}
