﻿using System;
using System.Threading.Tasks;
using LinCms.Application.Contracts.Blog.Channels;
using LinCms.Core.Data;

namespace LinCms.Application.Blog.Channels
{
    public interface IChannelService
    {
        void Delete(Guid id);
      
        Task<PagedResultDto<ChannelDto>> GetListAsync(PageDto pageDto);

        /// <summary>
        /// 首页减少不必要的字段后，流量字节更少
        /// </summary>
        /// <param name="pageDto"></param>
        /// <returns></returns>
        Task<PagedResultDto<NavChannelListDto>> GetNavListAsync(PageDto pageDto);

        Task<ChannelDto> GetAsync(Guid id);

        Task CreateAsync(CreateUpdateChannelDto createChannel);

        Task UpdateAsync(Guid id, CreateUpdateChannelDto updateChannel);
    }
}
