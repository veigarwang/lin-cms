﻿using AutoMapper;
using IGeekFan.FreeKit.Extras.FreeSql;
using IGeekFan.FreeKit.Extras.Security;
using LinCms.Aop.Filter;
using LinCms.Blog.Articles;
using LinCms.Data;
using LinCms.Entities.Blog;
using LinCms.Exceptions;
using LinCms.Extensions;
using LinCms.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeRedis;
using IGeekFan.FreeKit.Extras.Dto;


namespace LinCms.Controllers.Blog;

/// <summary>
/// 随笔
/// </summary>
[ApiExplorerSettings(GroupName = "blog")]
[Area("blog")]
[Route("api/blog/articles")]
[ApiController]
[Authorize]
public class ArticleController(
        IAuditBaseRepository<Article> articleRepository,
        IMapper mapper,
        ICurrentUser currentUser,
        IArticleService articleService,
        IRedisClient redisClient)
    : ControllerBase
{
    /// <summary>
    /// 我所有的随笔
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public PagedResultDto<ArticleListDto> GetMyArticleList([FromQuery] ArticleSearchDto searchDto)
    {
        List<ArticleListDto> articles = articleRepository
            .Select
            .IncludeMany(r => r.Tags, r => r.Where(u => u.Status == true))
            .Where(r => r.CreateUserId == currentUser.FindUserId())
            .WhereIf(searchDto.Title.IsNotNullOrEmpty(), r => r.Title.Contains(searchDto.Title))
            .WhereIf(searchDto.ClassifyId.HasValue, r => r.ClassifyId == searchDto.ClassifyId)
            .OrderByDescending(r => r.IsStickie)
            .OrderByDescending(r => r.Id)
            .ToPagerList(searchDto, out long totalCount)
            .Select(a => mapper.Map<ArticleListDto>(a))
            .ToList();

        return new PagedResultDto<ArticleListDto>(articles, totalCount);
    }


    #region CRUD

    /// <summary>
    /// 得到所有已审核过的随笔,最新的随笔/三天、七天、月榜、全部
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet("query")]
    [AllowAnonymous]
    // [Cacheable]
    public Task<PagedResultDto<ArticleListDto>> GetArticleAsync([FromQuery] ArticleSearchDto searchDto)
    {
        return articleService.GetArticleAsync(searchDto);

        //string redisKey = "article:query:" + EncryptUtil.Encrypt(JsonConvert.SerializeObject(searchDto, Formatting.Indented, new JsonSerializerSettings
        //{
        //    DefaultValueHandling = DefaultValueHandling.Ignore
        //}));

        //return RedisHelper.CacheShellAsync(
        //    redisKey, 60, () => _articleService.GetArticleAsync(searchDto)
        // );
    }

    /// <summary>
    /// 管理员删除违规随笔
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("cms/{id}")]
    [LinCmsAuthorize("删除随笔", "随笔")]
    public async Task<UnifyResponseDto> Delete(Guid id)
    {
        await articleService.DeleteAsync(id);
        return UnifyResponseDto.Success();
    }

    /// <summary>
    /// 删除自己的随笔
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<UnifyResponseDto> DeleteMyArticleAsync(Guid id)
    {
        bool isCreateArticle =
            await articleRepository.Select.AnyAsync(r => r.Id == id && r.CreateUserId == currentUser.FindUserId());
        if (!isCreateArticle)
        {
            throw new LinCmsException("无法删除别人的随笔!");
        }

        await articleService.DeleteAsync(id);
        return UnifyResponseDto.Success();
    }

    /// <summary>
    /// 随笔详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ArticleDto> GetAsync(Guid id)
    {
        await articleRepository.UpdateDiy.Set(r => r.ViewHits + 1).Where(r => r.Id == id).ExecuteAffrowsAsync();
        return await articleService.GetAsync(id);
    }

    [HttpPost]
    public async Task<Guid> CreateAsync([FromBody] CreateUpdateArticleDto createArticle)
    {
        Guid id = await articleService.CreateAsync(createArticle);
        string[] keys = await redisClient.KeysAsync("ArticleController:GetArticle:*");
        if (keys.Length > 0)
        {
            await redisClient.DelAsync(keys);
        }
        return id;
    }

    [Transactional]
    [HttpPut("{id}")]
    public async Task<UnifyResponseDto> UpdateAsync(Guid id, [FromBody] CreateUpdateArticleDto updateArticleDto)
    {
        await articleService.UpdateAsync(id, updateArticleDto);
        return UnifyResponseDto.Success("更新随笔成功");
    }

    #endregion

    /// <summary>
    /// 得到所有的随笔
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet("all")]
    [LinCmsAuthorize("所有随笔", "随笔")]
    public async Task<PagedResultDto<ArticleListDto>> GetAllArticleAsync([FromQuery] ArticleSearchDto searchDto)
    {
        return await articleService.GetAllArticleAsync(searchDto);
    }

    /// <summary>
    /// 审核随笔-拉黑/取消拉黑
    /// </summary>
    /// <param name="id">审论Id</param>
    /// <param name="isAudit"></param>
    /// <returns></returns>
    [LinCmsAuthorize("审核随笔", "随笔")]
    [HttpPut("audit/{id}")]
    public async Task<UnifyResponseDto> AuditAsync(Guid id, bool isAudit)
    {
        Article article = await articleRepository.Select.Where(r => r.Id == id).ToOneAsync();
        if (article == null)
        {
            throw new LinCmsException("没有找到相关随笔");
        }

        article.IsAudit = isAudit;
        await articleRepository.UpdateAsync(article);
        return UnifyResponseDto.Success();
    }

    /// <summary>
    /// 得到我关注的人 发布的随笔
    /// </summary>
    /// <param name="pageDto"></param>
    /// <returns></returns>
    [HttpGet("subscribe")]
    public Task<PagedResultDto<ArticleListDto>> GetSubscribeArticleAsync([FromQuery] PageDto pageDto)
    {
        return articleService.GetSubscribeArticleAsync(pageDto);
    }

    /// <summary>
    /// 修改随笔 是否允许其他人评论
    /// </summary>
    /// <param name="id">随笔主键</param>
    /// <param name="commentable">true:允许评论;false:不允许评论</param>
    /// <returns></returns>
    [HttpPut("{id}/comment-able/{commentable}")]
    public Task UpdateCommentable(Guid id, bool commentable)
    {
        return articleService.UpdateCommentable(id, commentable);
    }
}