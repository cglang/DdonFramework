using AutoMapper;
using Ddon.Application.Dtos;
using Ddon.Core.Services.LazyService;
using Ddon.Domain.Dtos;
using Ddon.Domain.Entities;
using Ddon.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Ddon.Application.Service
{
    public abstract class UniversalCrudApplicationService<TEntity, TKey, TResultDto, TRequestDto, TPageDto> : ApplicationService<TKey>, ICrudApplicationService<TKey, TResultDto, TRequestDto, TPageDto>
        where TEntity : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TResultDto : BaseDto<TKey>
        where TRequestDto : BaseDto<TKey>
        where TPageDto : Page
    {
        /// <summary>
        /// Mapper 映射
        /// </summary>
        protected IMapper Mapper => LazyServiceProvider.LazyGetRequiredService<IMapper>();

        private readonly IRepository<TEntity, TKey> _repository;

        public UniversalCrudApplicationService(ILazyServiceProvider lazyServiceProvider, IRepository<TEntity, TKey> repository) : base(lazyServiceProvider)
        {
            _repository = repository;
        }

        public virtual async Task<TResultDto> GetAsync(TKey id) => Mapper.Map<TResultDto>(await GetByIdAsync(id));

        protected abstract Task<TEntity?> GetByIdAsync(TKey id);

        public virtual async Task<IPageResult<TResultDto>> GetListAsync(TPageDto page)
        {
            var entities = await _repository.GetListAsync(page);
            return Mapper.Map<PageResult<TResultDto>>(entities);
        }

        public virtual async Task<TResultDto> CreateAsync(TRequestDto model)
        {
            TEntity entity = Mapper.Map<TEntity>(model);
            await _repository.AddAsync(entity, true);
            return Mapper.Map<TResultDto>(entity);
        }

        public async Task<TResultDto> UpdateAsync(TRequestDto model)
        {
            var entity = await _repository.FirstAsync(model.Id);
            Mapper.Map(model, entity);
            await _repository.UpdateAsync(entity, true);
            return Mapper.Map<TResultDto>(entity);
        }

        public virtual async Task DeleteAsync(TKey id) => await DeleteByIdAsync(id);

        protected abstract Task DeleteByIdAsync(TKey id);
    }
}
