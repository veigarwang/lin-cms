using System.Threading.Tasks;
using LinCms.Data;

namespace LinCms.v1.Encyclopedias
{
    public interface IEncyclopediaService
    {
        Task<int> CreateAsync(CreateUpdateEncyclopediaDto inputDto);

        Task DeleteAsync(long id);

        Task UpdateAsync(long id, CreateUpdateEncyclopediaDto inputDto);

        Task<EncyclopediaDto> GetAsync(long id);

        Task<long> GetTotalAsync();

        Task<PagedResultDto<EncyclopediaDto>> GetListAsync(PageDto pageDto);
    }
}