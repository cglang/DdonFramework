using Ddon.Application.Dtos;
using Ddon.Core.Services.LazyService;
using Ddon.Domain.Dtos;
using Ddon.Domain.Entities;
using Ddon.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Ddon.Application.Service
{
    public class CrudApplicationService<TEntity, TKey, TResultDto, TRequestDto, TPage>
        : UniversalCrudApplicationService<TEntity, TKey, TResultDto, TRequestDto, TPage>
        where TEntity : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TResultDto : BaseDto<TKey>
        where TRequestDto : BaseDto<TKey>
        where TPage : Page
    {
        private readonly IRepository<TEntity, TKey> _repository;

        public CrudApplicationService(ILazyServiceProvider lazyServiceProvider, IRepository<TEntity, TKey> repository) : base(lazyServiceProvider, repository)
        {
            _repository = repository;
        }

        protected override async Task DeleteByIdAsync(TKey id) => await _repository.DeleteAsync(id, true);

        protected override async Task<TEntity?> GetByIdAsync(TKey id) => await _repository.FirstOrDefaultAsync(id);
    }
}
