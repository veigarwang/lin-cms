using System.Threading.Tasks;
using LinCms.Aop.Attributes;
using LinCms.Aop.Filter;
using LinCms.Data;
using LinCms.v1.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinCms.Controllers.v1
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("v1/book")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [Logger("新增了一本书籍")]
        [LinCmsAuthorize("新增书籍", "书籍")]
        [HttpPost]
        public async Task<UnifyResponseDto> CreateAsync([FromBody] CreateUpdateBookDto createBook)
        {
            await _bookService.CreateAsync(createBook);
            return UnifyResponseDto.Success("新增书籍成功");
        }

        [Logger("删除了一本书籍")]
        [HttpDelete("{id}")]
        [LinCmsAuthorize("删除书籍", "书籍")]
        public async Task<UnifyResponseDto> DeleteAsync(int id)
        {
            await _bookService.DeleteAsync(id);
            return UnifyResponseDto.Success();

        }

        [Logger("更新了一本书籍")]
        [LinCmsAuthorize("更新书籍", "书籍")]
        [HttpPut("{id}")]
        public async Task<UnifyResponseDto> UpdateAsync(int id, [FromBody] CreateUpdateBookDto updateBook)
        {
            await _bookService.UpdateAsync(id, updateBook);
            return UnifyResponseDto.Success("更新书籍成功");
        }
        
        [HttpGet("{id}")]
        public async Task<BookDto> GetAsync(int id)
        {
            return await _bookService.GetAsync(id);
        }

        [HttpGet("getTotal")]
        public async Task<long> GetTotalAsync()
        {
            return await _bookService.GetTotalAsync();
        }

        [Logger("查询了书籍列表")]
        [HttpGet]
        public async Task<PagedResultDto<BookDto>> GetListAsync([FromQuery] PageDto pageDto)
        {
            return await _bookService.GetListAsync(pageDto);
        }
    }
}