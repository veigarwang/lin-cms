using IGeekFan.FreeKit.Extras.Dto;
using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Data;
using LinCms.Entities;
using LinCms.Entities.Base;
using LinCms.Exceptions;
using LinCms.Extensions;
using LinCms.IRepositories;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinCms.v1.Encyclopedias
{
    public class EncyclopediaService : ApplicationService, IEncyclopediaService
    {
        private readonly IAuditBaseRepository<Encyclopedia> _encyclopediaRepository;
        private readonly IAuditBaseRepository<BaseItem> _baseItemRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private enum CreateType
        {
            Insert,
            Append
        }

        public EncyclopediaService(IAuditBaseRepository<Encyclopedia> encyclopediaRepository, IAuditBaseRepository<BaseItem> baseItemRepository, IFileRepository fileRepository, IWebHostEnvironment hostingEnvironment)
        {
            _encyclopediaRepository = encyclopediaRepository;
            _baseItemRepository = baseItemRepository;
            _fileRepository = fileRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<int> CreateAsync(CreateUpdateEncyclopediaDto createEncyclopedia)
        {
            Encyclopedia exist = await _encyclopediaRepository.Where(r => (r.Name == createEncyclopedia.Name) && r.ItemType == createEncyclopedia.ItemType).ToOneAsync();
            if (exist == null)
            {
                exist = await _encyclopediaRepository.Where(r => r.Alias.Contains(createEncyclopedia.Name) && r.ItemType == createEncyclopedia.ItemType).ToOneAsync();
                if (exist != null && !string.IsNullOrEmpty(exist.Alias))
                {
                    var alias = exist.Alias?.Split(",");
                    if (alias.Length <= 0 || !alias.Contains(createEncyclopedia.Name))
                    {
                        exist = null;
                    }
                }
            }
            if (exist == null)
            {
                BaseItem item = await _baseItemRepository.Where(r => r.BaseTypeId == 3 && r.ItemCode == createEncyclopedia.ItemType.ToString()).ToOneAsync();
                exist = await _encyclopediaRepository.Where(r => (r.Name == (createEncyclopedia.Name + item.ItemName) || r.Name == createEncyclopedia.Name.Replace(item.ItemName, string.Empty)) && r.ItemType == createEncyclopedia.ItemType).ToOneAsync();
            }
            if (exist != null)
            {
                if (!string.IsNullOrEmpty(createEncyclopedia.Alias))
                {
                    if (string.IsNullOrEmpty(exist.Alias))
                        exist.Alias = createEncyclopedia.Alias;
                    else if (!exist.Alias.Contains(createEncyclopedia.Alias) && !exist.Name.Contains(createEncyclopedia.Alias))
                        exist.Alias += "," + createEncyclopedia.Alias;
                }

                if (!string.IsNullOrEmpty(createEncyclopedia.Explanation))
                {
                    if (string.IsNullOrEmpty(exist.Explanation))
                        exist.Explanation = createEncyclopedia.Explanation;
                    else if (!exist.Explanation.Contains(createEncyclopedia.Explanation))
                        exist.Explanation += "," + createEncyclopedia.Explanation;
                }

                if (!string.IsNullOrEmpty(createEncyclopedia.Effect))
                {
                    if (string.IsNullOrEmpty(exist.Effect))
                        exist.Effect = createEncyclopedia.Effect;
                    else if (!exist.Effect.Contains(createEncyclopedia.Effect))
                        exist.Effect += "," + createEncyclopedia.Effect;
                }

                if (!string.IsNullOrEmpty(createEncyclopedia.Provenance))
                {
                    if (string.IsNullOrEmpty(exist.Provenance))
                        exist.Provenance = createEncyclopedia.Provenance;
                    else if (!exist.Provenance.Contains(createEncyclopedia.Provenance))
                        exist.Provenance += "," + createEncyclopedia.Provenance;
                }

                if (!exist.OriginalText.Contains(createEncyclopedia.OriginalText.Trim('\n')))
                    exist.OriginalText += "\n" + createEncyclopedia.OriginalText.Trim('\n');

                if (!string.IsNullOrEmpty(createEncyclopedia.Guozhu))
                {
                    string guozhu = CorrectQuatation(createEncyclopedia.Guozhu).Trim('\n');
                    if (string.IsNullOrEmpty(exist.Guozhu))
                        exist.Guozhu = guozhu;
                    else if (!exist.Guozhu.Contains(guozhu))
                        exist.Guozhu += "\n" + guozhu;
                }

                if (!string.IsNullOrEmpty(createEncyclopedia.Tuzan))
                {
                    if (string.IsNullOrEmpty(exist.Tuzan))
                        exist.Tuzan = createEncyclopedia.Tuzan;
                    else if (!exist.Tuzan.Contains(createEncyclopedia.Tuzan.Trim('\n')))
                        exist.Tuzan += "\n" + createEncyclopedia.Tuzan.Trim('\n');
                }

                if (!string.IsNullOrEmpty(createEncyclopedia.Jijie))
                {
                    string jijie = CorrectQuatation(createEncyclopedia.Jijie).Trim('\n');
                    if (string.IsNullOrEmpty(exist.Jijie))
                        exist.Jijie = jijie;
                    else if (!exist.Jijie.Contains(jijie))
                        exist.Jijie += "\n" + jijie;
                }

                if (!string.IsNullOrEmpty(createEncyclopedia.Remarks))
                {
                    string remarks = CorrectQuatation(createEncyclopedia.Remarks).Trim('\n');
                    if (string.IsNullOrEmpty(exist.Remarks))
                        exist.Remarks = remarks;
                    else if (!exist.Jijie.Contains(remarks))
                        exist.Remarks += "\n" + remarks;
                }

                await _encyclopediaRepository.UpdateAsync(exist);
                //throw new LinCmsException("词条" + createEncyclopedia.Name + "已存在");
                return CreateType.Append.ToInt32();
            }
            else
            {
                Encyclopedia encyclopedia = Mapper.Map<Encyclopedia>(createEncyclopedia);
                encyclopedia.SimplifiedPronunciation = RemoveTune(createEncyclopedia.Pronunciation);
                encyclopedia.Guozhu = CorrectQuatation(encyclopedia.Guozhu)?.TrimEnd('\n');
                encyclopedia.Tuzan = encyclopedia.Tuzan?.TrimEnd('\n');
                encyclopedia.Jijie = CorrectQuatation(encyclopedia.Jijie)?.TrimEnd('\n');
                encyclopedia.Remarks = CorrectQuatation(encyclopedia.Remarks)?.TrimEnd('\n');
                await _encyclopediaRepository.InsertAsync(encyclopedia);
                return CreateType.Insert.ToInt32();
            }
        }

        public Task DeleteAsync(long id)
        {
            Encyclopedia encyclopedia = _encyclopediaRepository.Where(a => a.Id == id).ToOne();
            DeletePicFile(encyclopedia);
            return _encyclopediaRepository.DeleteAsync(new Encyclopedia { Id = id, Version = encyclopedia.Version });
        }

        private void DeletePicFile(Encyclopedia encyclopedia)
        {
            string url = _hostingEnvironment.WebRootPath + "/" + encyclopedia.Picture;
            if (File.Exists(@url))
                File.Delete(@url);
        }

        public async Task UpdateAsync(long id, CreateUpdateEncyclopediaDto updateEncyclopedia)
        {
            Encyclopedia encyclopedia = await _encyclopediaRepository.Where(r => r.Id == id).ToOneAsync();
            if (encyclopedia == null)
            {
                throw new LinCmsException("更新失败，指定词条不存在");
            }

            if (encyclopedia.Picture != updateEncyclopedia.Picture && _fileRepository.GetFileUrl(encyclopedia.Picture) != updateEncyclopedia.Picture)
            {
                DeletePicFile(encyclopedia);
            }

            SetUnchangedFieldsToNull(updateEncyclopedia, encyclopedia);
            //encyclopedia.Image = updateEncyclopedia.Image;
            //encyclopedia.Title = updateEncyclopedia.Title;
            //encyclopedia.Author = updateEncyclopedia.Author;
            //encyclopedia.Summary = updateEncyclopedia.Summary;
            //encyclopedia.Summary = updateEncyclopedia.Summary;

            //使用AutoMapper方法简化类与类之间的转换过程
            Mapper.Map(updateEncyclopedia, encyclopedia);

            if (updateEncyclopedia.GetType().GetProperties().All(p => p.GetValue(updateEncyclopedia) == null))
            {
                throw new LinCmsException("未发现任何改动");
            }

            encyclopedia.SimplifiedPronunciation = RemoveTune(encyclopedia.Pronunciation);
            encyclopedia.OriginalText = encyclopedia.OriginalText?.Replace("\n\n", "\n").Trim('\n');
            encyclopedia.Guozhu = CorrectQuatation(encyclopedia.Guozhu?.Replace("\n\n", "\n").Trim('\n'));
            encyclopedia.Tuzan = encyclopedia.Tuzan?.Replace("\n\n", "\n").Trim('\n');
            encyclopedia.Jijie = CorrectQuatation(encyclopedia.Jijie?.Replace("\n\n", "\n").Trim('\n'));
            encyclopedia.Remarks = CorrectQuatation(encyclopedia.Remarks?.Replace("\n\n", "\n").Trim('\n'));
            await _encyclopediaRepository.UpdateAsync(encyclopedia);
        }

        private static void SetUnchangedFieldsToNull(CreateUpdateEncyclopediaDto updateEncyclopedia, Encyclopedia encyclopedia)
        {
            Type dtoType = typeof(CreateUpdateEncyclopediaDto);
            Type entityType = typeof(Encyclopedia);

            foreach (var property in dtoType.GetProperties())
            {
                try
                {
                    var originalProperty = entityType.GetProperty(property.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (originalProperty == null) continue; // 如果原始对象中没有该属性，则跳过

                    var newValue = property.GetValue(updateEncyclopedia);
                    var oldValue = originalProperty.GetValue(encyclopedia);
                    bool isModified = false;
                    if (newValue != null)
                    {
                        if (newValue is DateTime newDate && oldValue is DateTime oldDate)
                        {
                            isModified = !(newDate.Year == oldDate.Year &&
                                           newDate.Month == oldDate.Month &&
                                           newDate.Day == oldDate.Day &&
                                           newDate.Hour == oldDate.Hour &&
                                           newDate.Minute == oldDate.Minute &&
                                           newDate.Second == oldDate.Second &&
                                           newDate.Millisecond == oldDate.Millisecond);
                        }
                        else
                        {
                            if (oldValue == null && newValue.ToString() == "")
                            {
                                isModified = true;
                            }
                            else
                            {
                                isModified = !newValue.Equals(oldValue);
                            }
                        }
                    }
                    else
                    {
                        // 如果 newValue 为空，可能是前端未传该字段，或者用户想清空该字段
                        if (oldValue != null && oldValue.ToString() != "")
                        {
                            isModified = true;
                        }
                    }

                    if (!isModified)
                    {
                        property.SetValue(updateEncyclopedia, null);
                    }

                }
                catch (Exception ex)
                {
                    throw new LinCmsException("更新失败：" + ex.Message);
                }
            }
        }

        public async Task<EncyclopediaDto> GetAsync(long id)
        {
            Encyclopedia encyclopedia = await _encyclopediaRepository.Where(a => a.Id == id).ToOneAsync();
            if (encyclopedia == null)
            {
                throw new LinCmsException("指定词条不存在，ID: " + id);
            }
            encyclopedia.Picture = _fileRepository.GetFileUrl(encyclopedia.Picture);
            return Mapper.Map<EncyclopediaDto>(encyclopedia);
        }

        public async Task<long> GetTotalAsync(int days)
        {
            return await _encyclopediaRepository.WhereIf(days != 0, p => p.CreateTime >= DateTime.Today.AddDays(days) || p.UpdateTime >= DateTime.Today.AddDays(days)).CountAsync();
        }

        public async Task<PagedResultDto<EncyclopediaDto>> GetListAsync(PageDto pageDto)
        {
            List<EncyclopediaDto> items = null;
            long count = 0;

            if (pageDto.ExactMatch)
            {
                items = (await _encyclopediaRepository.Select
                    .WhereIf(!string.IsNullOrEmpty(pageDto.ProvenanceType), p => p.Provenance == pageDto.ProvenanceType
                    || p.Provenance.StartsWith(pageDto.ProvenanceType + ",")
                    || p.Provenance.EndsWith("," + pageDto.ProvenanceType)
                    || p.Provenance.Contains("," + pageDto.ProvenanceType + ","))
                    .WhereIf(!string.IsNullOrEmpty(pageDto.ItemType), p => Convert.ToInt16(pageDto.ItemType) == p.ItemType)
                    .WhereIf(pageDto.Keyword != "{\"isTrusted\":true}" && !string.IsNullOrEmpty(pageDto.Keyword)
                    , p => p.Name == pageDto.Keyword
                    || p.Alias == pageDto.Keyword
                    || p.Alias.StartsWith(pageDto.Keyword + ",")
                    || p.Alias.EndsWith("," + pageDto.Keyword)
                    || p.Alias.Contains("," + pageDto.Keyword + ",")
                    || p.Id.ToString() == pageDto.Keyword
                    || p.SimplifiedPronunciation == pageDto.Keyword)
                    .OrderByDescending(r => r.Id)
                    .ToPagerListAsync(pageDto, out count))
                    .Select(r => Mapper.Map<EncyclopediaDto>(r)).ToList();
            }
            else
            {
                var provenanceList = pageDto.ProvenanceType?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                items = (await _encyclopediaRepository.Select
                .WhereIf(provenanceList?.Count > 0, p => provenanceList.Any(t => p.Provenance.Contains(t)))
                .WhereIf(!string.IsNullOrEmpty(pageDto.ItemType), p => Convert.ToInt16(pageDto.ItemType) == p.ItemType)
                .WhereIf(pageDto.Keyword != "{\"isTrusted\":true}" && !string.IsNullOrEmpty(pageDto.Keyword)
                , p => p.Name.Contains(pageDto.Keyword)
                || p.Alias.Contains(pageDto.Keyword)
                || p.Id.ToString() == pageDto.Keyword
                || p.SimplifiedPronunciation.Contains(pageDto.Keyword))
                .OrderByDescending(r => r.Id)
                .ToPagerListAsync(pageDto, out count))
                .Select(r => Mapper.Map<EncyclopediaDto>(r)).ToList();
            }

            foreach (var encyclopedia in items)
            {
                encyclopedia.ItemTypeName = _baseItemRepository.Where(p => p.BaseTypeId == 3 && p.ItemCode == encyclopedia.ItemType.ToString()).ToOne()?.ItemName;
            }
            return new PagedResultDto<EncyclopediaDto>(items, count);
        }

        private string CorrectQuatation(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            StringBuilder stringBuilder = new StringBuilder(text);
            char[] quatationArray = { '『', '』', '「', '」' };
            char[] leftQuatationArray = { '「', '『' };
            int singleLeftQuatationCount = 0, singleRightQuatationCount = 0, doubleLeftQuatationCount = 0, doubleRightQuatationCount = 0,
                quatationCount = 0;
            char lastQuatation = '『', currentQuatation;
            for (int i = 0; i < stringBuilder.Length; i++)
            {
                if (!quatationArray.Contains(stringBuilder[i])) continue;
                currentQuatation = stringBuilder[i];
                if (quatationCount++ == 0)
                {
                    if (stringBuilder[i] != '『')
                        stringBuilder[i] = '『';
                    doubleLeftQuatationCount++;
                }
                else
                {
                    // 始终判断上一个引号是什么，如果是左，此处应是右，单双相同；
                    // 若此处也是左，则此处应是右，单双与上一个相同；此处是左，则改成右
                    switch (lastQuatation)
                    {
                        case '『':
                        case '」':
                            if (leftQuatationArray.Contains(currentQuatation)) // 若上一个是左双/右单，此处是左引号，则应该是左单引号
                            {
                                currentQuatation = '「';
                                singleLeftQuatationCount++;
                            }
                            else // 若上一个是左双/右单，此处是右引号，则此处应是右双引号
                            {
                                currentQuatation = '』';
                                doubleRightQuatationCount++;
                            }
                            break;
                        case '』':
                        case '「':
                            if (leftQuatationArray.Contains(currentQuatation)) // 若上一个是左单/右双，此处是左引号，则应该是左双引号
                            {
                                currentQuatation = '『';
                                doubleLeftQuatationCount++;
                            }
                            else // 若上一个是左单/右双，此处是右引号，则此处应是右单引号
                            {
                                currentQuatation = '」';
                                singleRightQuatationCount++;
                            }
                            break;
                    }
                    stringBuilder[i] = currentQuatation;
                    lastQuatation = currentQuatation;
                }
            }
            if (doubleLeftQuatationCount != doubleRightQuatationCount)
            {
                throw new LinCmsException("双引号数量不匹配，左双引号数量：" + doubleLeftQuatationCount + "个\n" +
                    "右双引号数量：" + doubleRightQuatationCount + "个\n"
                    );
            }
            if (singleLeftQuatationCount != singleRightQuatationCount)
            {
                throw new LinCmsException("单引号数量不匹配，左单引号数量：" + singleLeftQuatationCount + "个\n" +
                    "右单引号数量：" + singleRightQuatationCount + "个\n"
                    );
            }
            return stringBuilder.ToString();
        }

        private string RemoveTune(string keyword)
        {
            return keyword
                .Replace("ā", "a")
                .Replace("á", "a")
                .Replace("ǎ", "a")
                .Replace("à", "a")
                .Replace("ō", "o")
                .Replace("ó", "o")
                .Replace("ǒ", "o")
                .Replace("ò", "o")
                .Replace("ē", "e")
                .Replace("é", "e")
                .Replace("ě", "e")
                .Replace("è", "e")
                .Replace("ī", "i")
                .Replace("í", "i")
                .Replace("ǐ", "i")
                .Replace("ì", "i")
                .Replace("ū", "u")
                .Replace("ú", "u")
                .Replace("ǔ", "u")
                .Replace("ù", "u")
                .Replace("ǖ", "v")
                .Replace("ǘ", "v")
                .Replace("ǚ", "v")
                .Replace("ǜ", "v")
                .Replace(" ", string.Empty);
        }
    }
}