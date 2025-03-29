using IGeekFan.FreeKit.Extras.Dto;
using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Aop.Filter;
using LinCms.Data;
using LinCms.FreeSql;
using LinCms.v1.Books;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinCms.Controllers.v1;

/// <summary>
/// 书籍
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
[Route("v1/book")]
[ApiController]
// [Authorize]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IDataSeedContributor _contributor;
    public BookController(IBookService bookService, IDataSeedContributor contributor)
    {
        _bookService = bookService;
        _contributor = contributor;
    }

    [Logger("新增了一本书籍")]
    [LinCmsAuthorize("新增书籍", "书籍")]
    [HttpPost]
    public async Task<UnifyResponseDto> CreateAsync([FromBody] CreateUpdateBookDto createBook)
    {
        await _bookService.CreateAsync(createBook);
        return UnifyResponseDto.Success("书籍《" + createBook.Title + "》" + (!string.IsNullOrEmpty(createBook.Subtitle) ? createBook.Subtitle : string.Empty) + "创建成功");
    }

    [Logger("删除了一本书籍")]
    [HttpDelete("{id}")]
    [LinCmsAuthorize("删除书籍", "书籍")]
    public async Task<UnifyResponseDto> DeleteAsync(int id)
    {
        var item = await _bookService.GetAsync(id);
        await _bookService.DeleteAsync(id);
        return UnifyResponseDto.Success("书籍《" + item.Title + "》" + (!string.IsNullOrEmpty(item.Subtitle) ? item.Subtitle : string.Empty) + "删除成功");
    }

    [HttpGet("list")]
    public async Task<List<BookDto>> GetListAsync()
    {
        return await _bookService.GetListAsync();
    }

    [Logger("更新了一本书籍")]
    [LinCmsAuthorize("更新书籍", "书籍")]
    [HttpPut("{id}")]
    public async Task<UnifyResponseDto> UpdateAsync(int id, [FromBody] CreateUpdateBookDto updateBook)
    {        
        await _bookService.UpdateAsync(id, updateBook);
        var item = await _bookService.GetAsync(id);
        return UnifyResponseDto.Success("书籍《" + item.Title + "》" + (!string.IsNullOrEmpty(item.Subtitle) ? item.Subtitle : string.Empty) + "更新成功");
    }

    [Logger("查看了书籍详情")]
    [HttpGet("{id}")]
    [LinCmsAuthorize("查看书籍详情", "书籍")]
    public async Task<BookDto> GetAsync(int id)
    {
        return await _bookService.GetAsync(id);
    }

    [DisableAuditing]
    [HttpGet("getTotal/{isRead}")]
    public async Task<long> GetTotalAsync(bool isRead = false)
    {
        return await _bookService.GetTotalAsync(isRead);
    }

    [Logger("查询了书籍列表")]
    [HttpGet]
    [LinCmsAuthorize("查询书籍列表", "书籍")]
    public async Task<PagedResultDto<BookDto>> GetPageListAsync([FromQuery] PageDto pageDto)
    {
        return await _bookService.GetPageListAsync(pageDto);
    }
}