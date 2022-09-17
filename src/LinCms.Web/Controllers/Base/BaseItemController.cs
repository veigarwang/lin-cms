﻿using System.Collections.Generic;
using System.Threading.Tasks;
using LinCms.Aop.Filter;
using LinCms.Base.BaseItems;
using LinCms.Data;
using Microsoft.AspNetCore.Mvc;

namespace LinCms.Controllers.Base
{
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

        [HttpDelete("{id}")]
        [LinCmsAuthorize("删除字典条目", "字典条目")]
        public async Task<UnifyResponseDto> DeleteAsync(int id)
        {
            await _baseItemService.DeleteAsync(id);
            return UnifyResponseDto.Success();
        }

        [HttpGet]
        public Task<List<BaseItemDto>> GetListAsync([FromQuery] string typeCode)
        {
            return _baseItemService.GetListAsync(typeCode);
        }

        [HttpGet("{id}")]
        public Task<BaseItemDto> GetAsync(int id)
        {
            return _baseItemService.GetAsync(id);
        }

        [HttpPost]
        [LinCmsAuthorize("新增字典条目", "字典条目")]
        public async Task<UnifyResponseDto> CreateAsync([FromBody] CreateUpdateBaseItemDto createBaseItem)
        {
            await _baseItemService.CreateAsync(createBaseItem);
            return UnifyResponseDto.Success("新增字典条目成功");
        }

        [HttpPut("{id}")]
        [LinCmsAuthorize("编辑字典条目", "字典条目")]
        public async Task<UnifyResponseDto> UpdateAsync(int id, [FromBody] CreateUpdateBaseItemDto updateBaseItem)
        {
            await _baseItemService.UpdateAsync(id, updateBaseItem);
            return UnifyResponseDto.Success("更新字典条目成功");
        }
    }
}