using Ddon.Domain.Dtos;
using System.Threading.Tasks;

namespace Ddon.Application.Service
{
    public interface ICrudApplicationService<TKey, TResultDto, TRequestDto, TPage> where TPage : Page
    {
        Task<TResultDto> GetAsync(TKey id);

        Task<PageResult<TResultDto>> GetListAsync(TPage page);

        Task<TResultDto> CreateAsync(TRequestDto model);

        Task<TResultDto> UpdateAsync(TRequestDto model);

        Task DeleteAsync(TKey id);
    }
}
