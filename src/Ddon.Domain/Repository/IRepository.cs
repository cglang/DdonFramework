using Ddon.Domain.Entities;
using Ddon.Identity.Specifications;
using Ddon.Identity.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Identity.Repository
{
    public interface IRepository : IUnitOfWork { }

    public interface IRepository<TEntity> : IRepository where TEntity : class, IEntity
    {
        DbSet<TEntity> DbSet { get; }

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        Task AddAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default);

        Task UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        Task UpdateAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

        Task<long> GetCountAsync(CancellationToken cancellationToken = default);

        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<IEnumerable<TEntity>> GetListAsync(CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<IEnumerable<TEntity>> GetListAsync(Page page, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        #region 规约查询

        Task<long> GetCountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

        Task<TEntity> FirstAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

        Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

        #endregion

        IQueryable<TEntity> Query { get; }

        IAsyncQueryableProvider AsyncExecuter { get; }
    }

    public interface IRepository<TEntity, TKey> : IRepository<TEntity> where TEntity : class, IEntity<TKey>
    {
        Task DeleteAsync(TKey id, bool autoSave = false, CancellationToken cancellationToken = default);

        Task<TEntity> FirstAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetListAsync(Page page, Expression<Func<TEntity, object>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        #region 规约查询

        Task<TResult?> FirstOrDefault<TResult>(ISpecification<TEntity, TKey, TResult> specification, CancellationToken cancellationToken = default);

        Task<IEnumerable<TResult>> GetListAsync<TResult>(ISpecification<TEntity, TKey, TResult> specification, CancellationToken cancellationToken = default);

        #endregion
    }
}