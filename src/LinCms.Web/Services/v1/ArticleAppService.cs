﻿using System;
using AutoMapper;
using FreeSql;
using LinCms.Web.Models.v1.Articles;
using LinCms.Web.Services.v1.Interfaces;
using LinCms.Zero.Domain.Blog;
using LinCms.Zero.Repositories;
using LinCms.Zero.Security;

namespace LinCms.Web.Services.v1
{
    public class ArticleAppService:IArticleService
    {
        private readonly AuditBaseRepository<Article> _articleRepository;
        private readonly GuidRepository<TagArticle> _tagArticleRepository;
        private readonly AuditBaseRepository<UserLike> _userLikeRepository;
        private readonly AuditBaseRepository<Comment> _commentBaseRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IFreeSql _freeSql;

        public ArticleAppService(AuditBaseRepository<Article> articleRepository, GuidRepository<TagArticle> tagArticleRepository, IMapper mapper, ICurrentUser currentUser, AuditBaseRepository<UserLike> userLikeRepository, AuditBaseRepository<Comment> commentBaseRepository, IFreeSql freeSql)
        {
            _articleRepository = articleRepository;
            _tagArticleRepository = tagArticleRepository;
            _mapper = mapper;
            _currentUser = currentUser;
            _userLikeRepository = userLikeRepository;
            _commentBaseRepository = commentBaseRepository;
            _freeSql = freeSql;
        }

        public void Delete(Guid id)
        {
            _articleRepository.Delete(new Article { Id = id });
            _tagArticleRepository.Delete(r => r.ArticleId == id);
            _commentBaseRepository.Delete(r => r.SubjectId == id);
            _userLikeRepository.Delete(r => r.SubjectId == id);
        }

        public ArticleDto Get(Guid id)
        {
            Article article = _articleRepository.Select.IncludeMany(r => r.Tags).Include(r=>r.UserInfo).Where(a => a.Id == id).ToOne();

            ArticleDto articleDto = _mapper.Map<ArticleDto>(article);
            articleDto.UserInfo.Avatar = _currentUser.GetFileUrl(articleDto.UserInfo.Avatar);

            articleDto.IsLiked = _userLikeRepository.Select.Any(r => r.SubjectId == id && r.CreateUserId == _currentUser.Id);

            articleDto.IsComment = _commentBaseRepository.Select.Any(r => r.SubjectId == id&&r.CreateUserId==_currentUser.Id);

            articleDto.ThumbnailDisplay = _currentUser.GetFileUrl(article.Thumbnail);

            return articleDto;
        }
    }
}