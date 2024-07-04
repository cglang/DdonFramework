using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Domain.Entities;
using Ddon.Domain.Extensions.ValueObject;
using Ddon.Domain.Repositories;
using Ddon.Domain.Specifications;
using Ddon.Repository.EntityFrameworkCore.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Repository.EntityFrameworkCore;

class EfCoreRepository<TDbContext, TKey>
    where TDbContext : DbContext
    where TKey : IEquatable<TKey>
{
    protected readonly TDbContext DbContext;

    protected EfCoreRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
    }
}

class EfCoreRepository<TDbContext, TEntity, TKey> : EfCoreRepository<TDbContext, TKey>, IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly ISpecificationEvaluator<TEntity, TKey>
        _specification = new SpecificationEvaluator<TEntity, TKey>();

    public virtual IAsyncQueryableProvider AsyncExecuter => new EfCoreAsyncQueryableProvider();

    protected EfCoreRepository(TDbContext dbContext) : base(dbContext) { }

    protected DbSet<TEntity> Entites => DbContext.Set<TEntity>();

    public async Task<int> SaveChangesAsync()
    {
        return await DbContext.SaveChangesAsync();
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        return await Entites.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        if (entity is IEntity<string> entityWithStringId)
        {
            entityWithStringId.Id = new Guid().ToString();
        }

        await Entites.AddAsync(entity, cancellationToken);

        if (autoSave)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task AddAsync(List<TEntity> entities, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var enumerable = entities.ToList();
        enumerable.ForEach(e =>
        {
            if (e is IEntity<string> entityWithStringId)
            {
                entityWithStringId.Id = new Guid().ToString();
            }
        });
        await Entites.AddRangeAsync(enumerable, cancellationToken);

        if (autoSave)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(TEntity entity, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        Entites.Remove(entity);

        if (autoSave)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(List<TEntity> entities, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        Entites.RemoveRange(entities);

        if (autoSave)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var entities = await Entites.Where(predicate).ToListAsync(cancellationToken);

        await DeleteAsync(entities, autoSave, cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        DbContext.Attach(entity);
        DbContext.Update(entity);

        if (autoSave)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task UpdateAsync(List<TEntity> entities, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var enumerable = entities.ToList();
        DbContext.AttachRange(enumerable);
        DbContext.UpdateRange(enumerable);

        if (autoSave)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await Entites.LongCountAsync(cancellationToken);
    }

    public virtual async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return await BuildQuery(propertySelectors).FirstAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return await BuildQuery(propertySelectors).FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return await BuildQuery(propertySelectors).SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return await BuildQuery(propertySelectors).ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return await BuildQuery(propertySelectors).Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<PageResult<TEntity>> GetListAsync(Page page,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return await BuildQuery(propertySelectors).QueryPageOrderByAsync(page, cancellationToken);
    }

    public virtual async Task<PageResult<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Page page,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return await BuildQuery(propertySelectors).Where(predicate).QueryPageOrderByAsync(page, cancellationToken);
    }

    public virtual async Task<TEntity> FirstAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await BuildQuery().FirstAsync(e => e.Id.Equals(id), cancellationToken);
    }

    protected virtual async Task<TEntity?> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await BuildQuery().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public virtual async Task DeleteAsync(TKey id, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var entity = await FirstOrDefaultAsync(id, cancellationToken);

        if (entity != null)
        {
            await DeleteAsync(entity, autoSave, cancellationToken);
            if (autoSave)
            {
                await DbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task<TResult?> FirstOrDefault<TResult>(ISpecification<TEntity, TKey, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        return await _specification.BuildQuery(Entites, specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<TResult>> GetListAsync<TResult>(ISpecification<TEntity, TKey, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        return await _specification.BuildQuery(Entites, specification).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 构建Query
    /// </summary>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    protected virtual IQueryable<TEntity> BuildQuery(params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        var query = Entites.AsQueryable();

        foreach (var propertySelector in propertySelectors)
        {
            query = query.Include(propertySelector);
        }

        return query;
    }
}
