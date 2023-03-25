using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Repository.Extensions;

public static class DbSetExtension
{
    public static async Task<TEntity> FirstAsync<TEntity, TKey>(this DbSet<TEntity> dbset, TKey id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return await dbset.FirstAsync(entity => entity.Id.Equals(id), cancellationToken);
    }
    
    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity, TKey>(this DbSet<TEntity> dbset, TKey id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return await dbset.FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    public static async Task DeleteAsync<TEntity, TKey>(this DbSet<TEntity> dbset, TKey id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var data = await dbset.FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
        if (data is null) return;
        dbset.Remove(data);
    }

    public static void Delete<TEntity, TKey>(this DbSet<TEntity> dbset, IEnumerable<TEntity> entities)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        dbset.RemoveRange(entities);
    }

    public static async Task DeleteAsync<TEntity, TKey>(this DbSet<TEntity> dbset,
        Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var entities = await dbset.Where(predicate).ToListAsync(cancellationToken);

        dbset.RemoveRange(entities);
    }

    public static void Update<TEntity, TKey>(this DbSet<TEntity> dbset, TEntity entity)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        dbset.Attach(entity);
        dbset.Update(entity);
    }

    public static void UpdateRange<TEntity, TKey>(this DbSet<TEntity> dbset, List<TEntity> entities)
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        dbset.AttachRange(entities);
        dbset.UpdateRange(entities);
    }
}
