using System.Threading.Tasks;
using LinCms.Data;

namespace LinCms.v1.Encyclopedias
{
    public interface IEncyclopediaService
    {
        Task<PagedResultDto<EncyclopediaDto>> GetListAsync(PageDto pageDto);

        Task<EncyclopediaDto> GetAsync(long id);

        Task CreateAsync(CreateUpdateEncyclopediaDto inputDto);

        Task UpdateAsync(long id, CreateUpdateEncyclopediaDto inputDto);

        Task DeleteAsync(long id);
    }
}