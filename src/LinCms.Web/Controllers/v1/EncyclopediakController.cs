using AutoMapper;
using IGeekFan.FreeKit.Extras.Dto;
using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Aop.Filter;
using LinCms.Data;
using LinCms.Entities;
using LinCms.v1.Encyclopedias;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LinCms.Controllers.v1
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("v1/encyclopedia")]
    [ApiController]
    // [Authorize]
    public class EncyclopediaController : ControllerBase
    {
        private readonly IEncyclopediaService _encyclopediaService;
        public EncyclopediaController(IEncyclopediaService encyclopediaService)
        {
            _encyclopediaService = encyclopediaService;
        }

        [Logger("新增了一个词条")]
        [LinCmsAuthorize("新增词条", "山海百科")]
        [HttpPost]
        public async Task<UnifyResponseDto> CreateAsync([FromBody] CreateUpdateEncyclopediaDto createEncyclopedia)
        {
            int res = await _encyclopediaService.CreateAsync(createEncyclopedia);
            return UnifyResponseDto.Success("词条【" + createEncyclopedia.Name + "】" + (res == 0 ? "新增" : "追加") + "成功");
        }

        [Logger("删除了一个词条")]
        [HttpDelete("{id}")]
        [LinCmsAuthorize("删除词条", "山海百科")]
        public async Task<UnifyResponseDto> DeleteAsync(int id)
        {
            var item = await _encyclopediaService.GetAsync(id);
            await _encyclopediaService.DeleteAsync(id);
            return UnifyResponseDto.Success("词条【" + item.Name + "】删除成功");

        }

        [Logger("更新了一个词条")]
        [LinCmsAuthorize("更新词条", "山海百科")]
        [HttpPut("{id}")]
        public async Task<UnifyResponseDto> UpdateAsync(int id, [FromBody] CreateUpdateEncyclopediaDto updateEncyclopedia)
        {
            await _encyclopediaService.UpdateAsync(id, updateEncyclopedia);
            return UnifyResponseDto.Success("词条【" + updateEncyclopedia.Name + "】更新成功");
        }

        [Logger("查看了词条详情")]
        [HttpGet("{id}")]
        [LinCmsAuthorize("查看百科词条详情", "山海百科")]
        public async Task<EncyclopediaDto> GetAsync(int id)
        {
            return await _encyclopediaService.GetAsync(id);
        }

        [DisableAuditing]
        [HttpGet("getTotal/{days}")]
        public async Task<long> GetTotalAsync(int days = 0)
        {
            return await _encyclopediaService.GetTotalAsync(days);
        }

        [Logger("查询了百科词条列表")]
        [HttpGet]
        [LinCmsAuthorize("查询百科词条列表", "山海百科")]
        public async Task<PagedResultDto<EncyclopediaDto>> GetListAsync([FromQuery] PageDto pageDto)
        {
            return await _encyclopediaService.GetListAsync(pageDto);
        }
    }
}