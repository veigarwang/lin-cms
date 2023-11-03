using System.Collections.Generic;
using System.Threading.Tasks;
using IGeekFan.FreeKit.Extras.Dto;
using LinCms.Data;

namespace LinCms.v1.Books;

public interface IBookService
{
    Task<List<BookDto>> GetListAsync();

    Task CreateAsync(CreateUpdateBookDto inputDto);

    Task DeleteAsync(long id);

    Task UpdateAsync(long id, CreateUpdateBookDto inputDto);

    Task<BookDto> GetAsync(long id);

    Task<long> GetTotalAsync(bool isRead = false);

    Task<PagedResultDto<BookDto>> GetPageListAsync(PageDto pageDto);
}