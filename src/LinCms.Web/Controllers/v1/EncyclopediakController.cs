using System.Threading.Tasks;
using LinCms.Aop.Filter;
using LinCms.Data;
using LinCms.v1.Encyclopedias;
using Microsoft.AspNetCore.Mvc;

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

        [HttpDelete("{id}")]
        [LinCmsAuthorize("删除词条", "山海百科")]
        public async Task<UnifyResponseDto> DeleteAsync(int id)
        {
            await _encyclopediaService.DeleteAsync(id);
            return UnifyResponseDto.Success();

        }

        [HttpGet]
        public async Task<PagedResultDto<EncyclopediaDto>> GetListAsync([FromQuery] PageDto pageDto)
        {
            return await _encyclopediaService.GetListAsync(pageDto);
        }

        [HttpGet("{id}")]
        public async Task<EncyclopediaDto> GetAsync(int id)
        {
            return await _encyclopediaService.GetAsync(id);
        }

        [LinCmsAuthorize("新增词条", "山海百科")]
        [HttpPost]
        public async Task<UnifyResponseDto> CreateAsync([FromBody] CreateUpdateEncyclopediaDto createEncyclopedia)
        {
            await _encyclopediaService.CreateAsync(createEncyclopedia);
            return UnifyResponseDto.Success("新增词条成功");
        }

        [LinCmsAuthorize("更新词条", "山海百科")]
        [HttpPut("{id}")]
        public async Task<UnifyResponseDto> UpdateAsync(int id, [FromBody] CreateUpdateEncyclopediaDto updateEncyclopedia)
        {
            await _encyclopediaService.UpdateAsync(id, updateEncyclopedia);
            return UnifyResponseDto.Success("更新词条成功");
        }
    }
}