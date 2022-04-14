using Ddon.Application.Dtos;
using Ddon.Core.Services.LazyService;
using Ddon.Domain;
using Ddon.Domain.Entities;
using Ddon.Domain.Repository;
using Ddon.Uow;
using System;
using System.Threading.Tasks;

namespace Ddon.Application.Service
{
    public class CrudApplicationService<TEntity, TKey, TResponseDto, TRequestDto, TPageDto>
        : UniversalCrudApplicationService<TEntity, TKey, TResponseDto, TRequestDto, TPageDto>
        where TEntity : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TRequestDto : BaseDto<TKey>
        where TPageDto : Page
    {
        /// <summary>
        /// 工作单元
        /// </summary>
        protected IUnitOfWork UnitOfWork => LazyServiceProvider.LazyGetRequiredService<IUnitOfWork>();

        public CrudApplicationService(ILazyServiceProvider lazyServiceProvider, IRepository<TEntity, TKey> repository) : base(lazyServiceProvider, repository)
        {
            Repository = repository;
        }

        private IRepository<TEntity, TKey> Repository { get; }

        protected override async Task DeleteByIdAsync(TKey id, bool autoSave = false) => await Repository.DeleteAsync(id, autoSave);

        protected override async Task<TEntity?> GetByIdAsync(TKey id) => await Repository.FirstOrDefaultAsync(id);
    }
}
