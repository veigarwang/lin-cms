using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Aop.Filter;
using LinCms.Base.BaseTypes;
using LinCms.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinCms.Controllers.Base;

/// <summary>
/// 数据字典-分类
/// </summary>
[ApiExplorerSettings(GroupName = "base")]
[Area("base")]
[Route("api/base/type")]
[ApiController]
public class BaseTypeController : ControllerBase
{
    private readonly IBaseTypeService _baseTypeService;
    public BaseTypeController(IBaseTypeService baseTypeService)
    {
        _baseTypeService = baseTypeService;
    }

    [Logger("删除了一个字典类型")]
    [HttpDelete("{id}")]
    [LinCmsAuthorize("删除字典类别", "字典类别")]
    public async Task<UnifyResponseDto> DeleteAsync(int id)
    {
        await _baseTypeService.DeleteAsync(id);
        return UnifyResponseDto.Success();
    }

    [Logger("查询了字典类型列表")]
    [HttpGet]
    [LinCmsAuthorize("查询字典类别列表", "字典类别")]
    public Task<List<BaseTypeDto>> GetListAsync()
    {
        return _baseTypeService.GetListAsync();
    }

    [Logger("查看了字典类型详情")]
    [HttpGet("{id}")]
    [LinCmsAuthorize("查看字典类型详情", "字典类别")]
    public Task<BaseTypeDto> GetAsync(int id)
    {
        return _baseTypeService.GetAsync(id);
    }

    [Logger("新增了一个字典类型")]
    [HttpPost]
    [LinCmsAuthorize("新增字典类别", "字典类别")]
    public async Task<UnifyResponseDto> CreateAsync([FromBody] CreateUpdateBaseTypeDto createBaseType)
    {
        await _baseTypeService.CreateAsync(createBaseType);
        return UnifyResponseDto.Success("新增类别成功");
    }

    [Logger("更新了一个字典类型")]
    [HttpPut("{id}")]
    [LinCmsAuthorize("编辑字典类别", "字典类别")]
    public async Task<UnifyResponseDto> UpdateAsync(int id, [FromBody] CreateUpdateBaseTypeDto updateBaseType)
    {
        await _baseTypeService.UpdateAsync(id, updateBaseType);
        return UnifyResponseDto.Success("更新类别成功");
    }
}
