using Ddon.Core.Models;
using Ddon.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ddon.AspNetCore.DdonController
{
    public abstract class DdonController<TKey, TResponseDto, TRequestDto, TPageDto> : ControllerBase
        where TPageDto : Page
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public abstract Task<TResponseDto> CreateAsync([FromQuery] TRequestDto requestModel);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task DeleteAsync([FromBody] TKey id);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public abstract Task<TResponseDto> UpdateAsync([FromBody] TRequestDto requestModel);

        /// <summary>
        /// 根据 Id 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task<TResponseDto> GetAsync([FromQuery] TKey id);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public abstract Task<PageResult<TResponseDto>> GetListAsync([FromQuery] TPageDto requestModel);
    }
}
