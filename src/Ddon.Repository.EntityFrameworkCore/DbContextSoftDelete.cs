using System.Linq;
using System.Threading.Tasks;
using Ddon.Domain.BaseObject;
using Ddon.Repository.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Repository.EntityFrameworkCore
{
    public interface IDbContextSoftDelete
    {
        void OnModleSoftDelete(ModelBuilder modelBuilder);
    }

    public static class DbContextSoftDelete
    {
        public static void Builder(ModelBuilder modelBuilder)
        {
            var softDeleteTypes = modelBuilder.Model.GetEntityTypes().Where(x => typeof(ISoftDelete).IsAssignableFrom(x.ClrType));
            Parallel.ForEach(softDeleteTypes, x =>
            {
                modelBuilder.Entity(x.ClrType).AddQueryFilter<ISoftDelete>(e => !e.IsDeleted);
            });
        }
    }
}
