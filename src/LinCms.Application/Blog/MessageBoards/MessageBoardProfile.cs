﻿using AutoMapper;
using LinCms.Application.Contracts.Blog.MessageBoards;
using LinCms.Core.Entities.Blog;

namespace LinCms.Application.Blog.MessageBoards
{
    public class MessageBoardProfile:Profile
    {
        public MessageBoardProfile()
        {
            CreateMap<CreateMessageBoardDto, MessageBoard>();
            CreateMap<MessageBoard, MessageBoardDto>();
        }
    }
}