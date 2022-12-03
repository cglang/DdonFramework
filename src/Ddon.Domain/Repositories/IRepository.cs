using Ddon.Domain.Dtos;
using Ddon.Domain.Entities;
using Ddon.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Domain.Repositories
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        Task AddAsync(List<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(List<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default);

        Task UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        Task UpdateAsync(List<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

        Task<long> GetCountAsync(CancellationToken cancellationToken = default);

        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<PageResult<TEntity>> GetListAsync(Page page, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        Task<PageResult<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Page page, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        IAsyncQueryableProvider AsyncExecuter { get; }

        Task<int> SaveChangesAsync();
    }

    public interface IRepository<TEntity, TKey> : IRepository<TEntity> where TEntity : class, IEntity<TKey>
    {
        Task DeleteAsync(TKey id, bool autoSave = false, CancellationToken cancellationToken = default);

        Task<TEntity> FirstAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetListAsync(Page page, Expression<Func<TEntity, object>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] propertySelectors);

        #region 规约查询

        Task<TResult?> FirstOrDefault<TResult>(ISpecification<TEntity, TKey, TResult> specification, CancellationToken cancellationToken = default);

        Task<List<TResult>> GetListAsync<TResult>(ISpecification<TEntity, TKey, TResult> specification, CancellationToken cancellationToken = default);

        #endregion
    }
}