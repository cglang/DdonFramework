using Ddon.Domain;
using Ddon.Domain.Entities;
using Ddon.Domain.Repository;
using Ddon.Identity.Repository;
using Ddon.Identity.Specifications;
using Ddon.Repositiry.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Repositiry.EntityFrameworkCore
{
    public class EfCoreRepository<TDbContext, TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        protected readonly TDbContext DbContext;

        protected EfCoreRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }

    public class EfCoreRepository<TDbContext, TEntity, TKey> : EfCoreRepository<TDbContext, TKey>, IRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ISpecificationEvaluator<TEntity> _specification = new SpecificationEvaluator<TEntity>();
        private readonly ISpecificationEvaluator<TEntity, TKey> _specification2 = new SpecificationEvaluator<TEntity, TKey>();
        public virtual IAsyncQueryableProvider AsyncExecuter => new EfCoreAsyncQueryableProvider();

        protected EfCoreRepository(TDbContext dbContext) : base(dbContext) { }

        public IQueryable<TEntity> Query => DbSet.AsQueryable();

        public DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

        public async Task<int> SaveChangesAsync()
        {
            return await DbContext.SaveChangesAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            return await DbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task AddAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entity is IEntity<string> entityWithStringId)
            {
                entityWithStringId.Id = new Guid().ToString();
            }

            if (entity is IMultTenant<string> multEntityWithStringId)
            {
                multEntityWithStringId.TenantId = new Guid().ToString();
            }

            await DbSet.AddAsync(entity, cancellationToken);

            if (autoSave)
            {
                await DbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task AddAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var enumerable = entities.ToList();
            enumerable.ForEach(e =>
            {
                if (e is IEntity<string> entityWithStringId)
                {
                    entityWithStringId.Id = new Guid().ToString();
                }
            });
            await DbSet.AddRangeAsync(enumerable, cancellationToken);

            if (autoSave)
            {
                await DbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            DbSet.Remove(entity);

            if (autoSave)
            {
                await DbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task DeleteAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            DbSet.RemoveRange(entities);

            if (autoSave)
            {
                await DbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entities = await DbSet.Where(predicate).ToListAsync();

            await DeleteAsync(entities, autoSave, cancellationToken);
        }

        public virtual async Task UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            DbContext.Attach(entity);
            DbContext.Update(entity);

            if (autoSave)
            {
                await DbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task UpdateAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
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
            return await DbSet.LongCountAsync(cancellationToken);
        }

        public virtual async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            return await BuildQuery(propertySelectors).FirstAsync(predicate, cancellationToken);
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            return await BuildQuery(propertySelectors).FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            return await BuildQuery(propertySelectors).SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetListAsync(CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            return await BuildQuery(propertySelectors).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            return await BuildQuery(propertySelectors).Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetListAsync(Page page, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            return await BuildQuery(propertySelectors).QueryPageOrderBy(page).ToListAsync(cancellationToken);
        }

        #region 归约查询

        public async Task<long> GetCountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await _specification.BuildQuery(Query, specification).LongCountAsync(cancellationToken);
        }

        public async Task<TEntity> FirstAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await _specification.BuildQuery(Query, specification).FirstAsync(cancellationToken);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await _specification.BuildQuery(Query, specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await _specification.BuildQuery(Query, specification).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await _specification.BuildQuery(Query, specification).ToListAsync(cancellationToken);
        }

        #endregion

        public virtual async Task<TEntity> FirstAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await BuildQuery().FirstAsync(e => e.Id!.Equals(id), cancellationToken);

            if (entity is null)
                throw new InvalidOperationException();

            return entity;
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await BuildQuery().FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetListAsync(Page page, Expression<Func<TEntity, object>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            return await BuildQuery(propertySelectors).QueryPageOrderBy(page, predicate).ToListAsync(cancellationToken);
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

        public async Task<TResult?> FirstOrDefault<TResult>(ISpecification<TEntity, TKey, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await _specification2.BuildQuery(Query, specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<TResult>> GetListAsync<TResult>(ISpecification<TEntity, TKey, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await _specification2.BuildQuery(Query, specification).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 构建Query
        /// </summary>
        /// <param name="propertySelectors"></param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> BuildQuery(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            var query = Query;

            foreach (var propertySelector in propertySelectors)
            {
                query = query.Include(propertySelector);
            }

            return query;
        }
    }
}
