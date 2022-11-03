using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Aop.Filter;
using LinCms.Base.BaseItems;
using LinCms.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinCms.Controllers.Base;

/// <summary>
/// 数据字典-详情项
/// </summary>
[ApiExplorerSettings(GroupName = "base")]
[Area("base")]
[Route("api/base/item")]
[ApiController]
public class BaseItemController : ControllerBase
{
    private readonly IBaseItemService _baseItemService;

    public BaseItemController(IBaseItemService baseItemService)
    {
        _baseItemService = baseItemService;
    }

    [Logger("删除了一个字典条目")]
    [HttpDelete("{id}")]
    [LinCmsAuthorize("删除字典条目", "字典条目")]
    public async Task<UnifyResponseDto> DeleteAsync(int id)
    {
        await _baseItemService.DeleteAsync(id);
        return UnifyResponseDto.Success();
    }

    [Logger("查询了字典条目列表")]
    [HttpGet]
    [LinCmsAuthorize("查询字典条目列表", "字典条目")]
    public Task<List<BaseItemDto>> GetListAsync([FromQuery] string typeCode)
    {
        return _baseItemService.GetListAsync(typeCode);
    }

    [Logger("查看了字典条目详情")]
    [HttpGet("{id}")]
    [LinCmsAuthorize("查看字典条目详情", "字典条目")]
    public Task<BaseItemDto> GetAsync(int id)
    {
        return _baseItemService.GetAsync(id);
    }

    [Logger("新增了一个字典条目")]
    [HttpPost]
    [LinCmsAuthorize("新增字典条目", "字典条目")]
    public async Task<UnifyResponseDto> CreateAsync([FromBody] CreateUpdateBaseItemDto createBaseItem)
    {
        await _baseItemService.CreateAsync(createBaseItem);
        return UnifyResponseDto.Success("新增字典条目成功");
    }

    [Logger("更新了一个字典条目")]
    [HttpPut("{id}")]
    [LinCmsAuthorize("编辑字典条目", "字典条目")]
    public async Task<UnifyResponseDto> UpdateAsync(int id, [FromBody] CreateUpdateBaseItemDto updateBaseItem)
    {
        await _baseItemService.UpdateAsync(id, updateBaseItem);
        return UnifyResponseDto.Success("更新字典条目成功");
    }
}