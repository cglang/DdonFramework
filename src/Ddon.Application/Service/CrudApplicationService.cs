using Ddon.Application.Dtos;
using Ddon.Core.Services.LazyService;
using Ddon.Domain.Entities;
using Ddon.Identity;
using Ddon.Identity.Repository;
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
        public CrudApplicationService(IRepository<TEntity, TKey> repository, ILazyServiceProvider lazyServiceProvider) : base(repository, lazyServiceProvider)
        {
            Repository = repository;
        }

        private IRepository<TEntity, TKey> Repository { get; }

        protected override async Task DeleteByIdAsync(TKey id, bool autoSave = false) => await Repository.DeleteAsync(id, autoSave);

        protected override async Task<TEntity?> GetByIdAsync(TKey id) => await Repository.FirstOrDefaultAsync(id);
    }
}
