using LinCms.Data;
using LinCms.Entities;
using LinCms.Entities.Base;
using LinCms.Exceptions;
using LinCms.Extensions;
using LinCms.IRepositories;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LinCms.v1.Encyclopedias
{
    public class EncyclopediaService : ApplicationService, IEncyclopediaService
    {
        private readonly IAuditBaseRepository<Encyclopedia> _encyclopediaRepository;
        private readonly IAuditBaseRepository<BaseItem> _baseItemRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public EncyclopediaService(IAuditBaseRepository<Encyclopedia> encyclopediaRepository, IAuditBaseRepository<BaseItem> baseItemRepository, IFileRepository fileRepository, IWebHostEnvironment hostingEnvironment)
        {
            _encyclopediaRepository = encyclopediaRepository;
            _baseItemRepository = baseItemRepository;
            _fileRepository = fileRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task CreateAsync(CreateUpdateEncyclopediaDto createEncyclopedia)
        {
            Encyclopedia exist = await _encyclopediaRepository.Select.Where(r => r.Name == createEncyclopedia.Name || r.Alias.Contains(createEncyclopedia.Name)).ToOneAsync();
            if (exist != null)
            {
                if (!string.IsNullOrEmpty(createEncyclopedia.Alias))
                    exist.Alias += string.IsNullOrEmpty(exist.Alias) ? createEncyclopedia.Alias : "," + createEncyclopedia.Alias;
                if (!string.IsNullOrEmpty(createEncyclopedia.Explanation))
                    exist.Explanation += string.IsNullOrEmpty(exist.Explanation) ? createEncyclopedia.Explanation : "," + createEncyclopedia.Explanation;
                if (!string.IsNullOrEmpty(createEncyclopedia.Effect))
                    exist.Effect += string.IsNullOrEmpty(exist.Effect) ? createEncyclopedia.Effect : "," + createEncyclopedia.Effect;
                if (!string.IsNullOrEmpty(createEncyclopedia.Provenance))
                    exist.Provenance += string.IsNullOrEmpty(exist.Provenance) ? createEncyclopedia.Provenance : "," + createEncyclopedia.Provenance;
                exist.OriginalText += "\n" + createEncyclopedia.OriginalText;
                if (!string.IsNullOrEmpty(createEncyclopedia.Guozhu))
                    exist.Guozhu += string.IsNullOrEmpty(exist.Guozhu) ? createEncyclopedia.Guozhu : "\n" + createEncyclopedia.Guozhu;
                if (!string.IsNullOrEmpty(createEncyclopedia.Tuzan))
                    exist.Tuzan += string.IsNullOrEmpty(exist.Tuzan) ? createEncyclopedia.Tuzan : "\n" + createEncyclopedia.Tuzan;
                if (!string.IsNullOrEmpty(createEncyclopedia.Jijie))
                    exist.Jijie += string.IsNullOrEmpty(exist.Jijie) ? createEncyclopedia.Jijie : "\n" + createEncyclopedia.Jijie;
                if (!string.IsNullOrEmpty(createEncyclopedia.Remarks))
                    exist.Remarks += string.IsNullOrEmpty(exist.Remarks) ? createEncyclopedia.Remarks : "\n" + createEncyclopedia.Remarks;
                
                await _encyclopediaRepository.UpdateAsync(exist);
                //throw new LinCmsException("词条" + createEncyclopedia.Name + "已存在");
            }
            else
            {
                Encyclopedia encyclopedia = Mapper.Map<Encyclopedia>(createEncyclopedia);
                await _encyclopediaRepository.InsertAsync(encyclopedia);
            }
        }

        public Task DeleteAsync(long id)
        {
            Encyclopedia encyclopedia = _encyclopediaRepository.Where(a => a.Id == id).ToOne();
            DeletePicFile(encyclopedia);
            return _encyclopediaRepository.DeleteAsync(new Encyclopedia { Id = id });
        }

        private void DeletePicFile(Encyclopedia encyclopedia)
        {
            string url = _hostingEnvironment.WebRootPath + "/" + encyclopedia.Picture;
            if (File.Exists(@url))
                File.Delete(@url);
        }

        public async Task<EncyclopediaDto> GetAsync(long id)
        {
            Encyclopedia encyclopedia = await _encyclopediaRepository.Where(a => a.Id == id).ToOneAsync();
            if (encyclopedia == null)
            {
                throw new LinCmsException("指定词条不存在");
            }
            encyclopedia.Picture = _fileRepository.GetFileUrl(encyclopedia.Picture);
            return Mapper.Map<EncyclopediaDto>(encyclopedia);
        }

        public async Task<PagedResultDto<EncyclopediaDto>> GetListAsync(PageDto pageDto)
        {
            List<EncyclopediaDto> items = (await _encyclopediaRepository.WhereIf(pageDto.Keyword != "{\"isTrusted\":true}" && !string.IsNullOrEmpty(pageDto.Keyword), p => p.Name.Contains(pageDto.Keyword)).OrderByDescending(r => r.Id).ToPagerListAsync(pageDto, out long count)).Select(r => Mapper.Map<EncyclopediaDto>(r)).ToList();

            foreach (var encyclopedia in items)
            {
                encyclopedia.ItemTypeName = _baseItemRepository.Where(p => p.BaseTypeId == 3 && p.ItemCode == encyclopedia.ItemType.ToString()).ToOne().ItemName;
            }
            return new PagedResultDto<EncyclopediaDto>(items, count);
        }

        public async Task UpdateAsync(long id, CreateUpdateEncyclopediaDto updateEncyclopedia)
        {
            Encyclopedia encyclopedia = await _encyclopediaRepository.Where(r => r.Id == id).ToOneAsync();
            if (encyclopedia == null)
            {
                throw new LinCmsException("指定词条不存在");
            }

            if (_fileRepository.GetFileUrl(encyclopedia.Picture) != updateEncyclopedia.Picture)
            {
                DeletePicFile(encyclopedia);
            }
            //encyclopedia.Image = updateEncyclopedia.Image;
            //encyclopedia.Title = updateEncyclopedia.Title;
            //encyclopedia.Author = updateEncyclopedia.Author;
            //encyclopedia.Summary = updateEncyclopedia.Summary;
            //encyclopedia.Summary = updateEncyclopedia.Summary;

            //使用AutoMapper方法简化类与类之间的转换过程
            Mapper.Map(updateEncyclopedia, encyclopedia);

            await _encyclopediaRepository.UpdateAsync(encyclopedia);
        }
    }
}