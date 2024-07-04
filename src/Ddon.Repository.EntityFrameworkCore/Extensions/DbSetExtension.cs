using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Domain.Entities;

namespace Microsoft.EntityFrameworkCore;

public static class DbSetExtension
{
    public static Task<TEntity> FirstAsync<TEntity, TKey>(this DbSet<TEntity> dbset, TKey id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return dbset.FirstAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    public static Task<TEntity?> FirstOrDefaultAsync<TEntity, TKey>(this DbSet<TEntity> dbset, TKey id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return dbset.FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    public static async Task RemoveAsync<TEntity, TKey>(this DbSet<TEntity> dbset, TKey id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var data = await dbset.FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
        if (data is null) return;
        dbset.Remove(data);
    }

    public static Task RemoveRangeAsync<TEntity, TKey>(this DbSet<TEntity> dbset, IEnumerable<TEntity> entities)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return Task.Run(() => dbset.RemoveRange(entities));
    }

    public static async Task RemoveRangeAsync<TEntity, TKey>(this DbSet<TEntity> dbset,
        Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var entities = await dbset.Where(predicate).ToListAsync(cancellationToken);

        dbset.RemoveRange(entities);
    }

    public static Task UpdateAsync<TEntity, TKey>(this DbSet<TEntity> dbset, TEntity entity)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return Task.Run(() =>
        {
            dbset.Attach(entity);
            dbset.Update(entity);
        });
    }

    public static Task UpdateRangeAsync<TEntity, TKey>(this DbSet<TEntity> dbset, List<TEntity> entities)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return Task.Run(() =>
        {
            dbset.AttachRange(entities);
            dbset.UpdateRange(entities);
        });
    }
}
