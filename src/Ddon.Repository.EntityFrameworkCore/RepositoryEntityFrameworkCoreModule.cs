using System;
using Ddon.Core;
using Ddon.Repository.EntityFrameworkCore.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Repository.EntityFrameworkCore
{
    public class RepositoryEntityFrameworkCoreModule<TDbContext> : Module<DbContextOptionsBuilder>
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
