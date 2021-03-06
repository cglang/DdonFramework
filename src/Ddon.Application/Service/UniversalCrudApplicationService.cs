using AutoMapper;
using Ddon.Application.Dtos;
using Ddon.Core.Services.LazyService;
using Ddon.Domain;
using Ddon.Domain.Entities;
using Ddon.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ddon.Application.Service
{
    public abstract class UniversalCrudApplicationService<TEntity, TKey, TResponseDto, TRequestDto, TPageDto>
        : ApplicationService<TKey>, ICrudApplicationService<TKey, TResponseDto, TRequestDto, TPageDto>
        where TEntity : Entity<TKey>
        where TKey : IEquatable<TKey>
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

        public virtual async Task<TResponseDto> CreateAsync(TRequestDto requestDto, bool autoSave = false)
        {
            TEntity entity = Mapper.Map<TEntity>(requestDto);
            await _repository.AddAsync(entity, true);
            return Mapper.Map<TResponseDto>(entity);
        }

        public virtual async Task DeleteAsync(TKey id, bool autoSave = false)
        {
            await DeleteByIdAsync(id, autoSave);
        }

        protected abstract Task DeleteByIdAsync(TKey id, bool autoSave = false);

        public virtual async Task<TResponseDto> GetAsync(TKey id)
        {
            return Mapper.Map<TResponseDto>(await GetByIdAsync(id));
        }

        protected abstract Task<TEntity?> GetByIdAsync(TKey id);

        public virtual async Task<PageResult<TResponseDto>> GetListAsync(TPageDto requestDto)
        {
            var entitiesTask = _repository.GetListAsync(requestDto);
            var countTask = _repository.GetCountAsync();
            var entityDtos = Mapper.Map<List<TResponseDto>>(await entitiesTask);

            return new PageResult<TResponseDto>(await countTask, entityDtos);
        }

        protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TPageDto requestDto)
        {
            if (requestDto is Page model && !string.IsNullOrWhiteSpace(model.Sorting))
            {
                return query.OrderBy(model.Sorting);
            }

            return query;
        }

        protected virtual IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> query, TPageDto requestDto)
        {
            if (requestDto is Page model)
            {
                return query.Skip((model.Index - 1) * model.Size).Take(model.Size);
            }

            return query;
        }

        public async Task<TResponseDto> UpdateAsync(TRequestDto requestDto, bool autoSave = false)
        {
            var entity = await _repository.FirstAsync(requestDto.Id);
            Mapper.Map(requestDto, entity);
            await _repository.UpdateAsync(entity, autoSave);
            return Mapper.Map<TResponseDto>(entity);
        }

    }
}
