using System.Threading.Tasks;
using LinCms.Data;

namespace LinCms.v1.Books
{
    public interface IBookService
    {
        Task CreateAsync(CreateUpdateBookDto inputDto);

        Task DeleteAsync(long id);

        Task UpdateAsync(long id, CreateUpdateBookDto inputDto);

        Task<BookDto> GetAsync(long id);

        Task<long> GetTotalAsync();

        Task<PagedResultDto<BookDto>> GetListAsync(PageDto pageDto);
    }
}