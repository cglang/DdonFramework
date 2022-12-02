using Ddon.Application.Dtos;
using Ddon.Domain.Dtos;
using System.Threading.Tasks;

namespace Ddon.Application.Service
{
    public interface ICrudApplicationService<TKey, TResponseDto, TRequestDto, TPageDto> where TPageDto : Page
    {
        Task<TResponseDto> CreateAsync(TRequestDto requestModel, bool autoSave = false);

        Task DeleteAsync(TKey id, bool autoSave = false);

        Task<TResponseDto> UpdateAsync(TRequestDto requestModel, bool autoSave = false);

        Task<TResponseDto> GetAsync(TKey id);

        Task<IPageResult<TResponseDto>> GetListAsync(TPageDto requestModel);
    }
}
