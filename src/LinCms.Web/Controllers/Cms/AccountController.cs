﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using LinCms.Aop.Attributes;
using LinCms.Cms.Account;
using LinCms.Cms.Users;
using LinCms.Data;
using LinCms.Data.Enums;
using LinCms.Entities;
using LinCms.Exceptions;
using LinCms.IRepositories;
using LinCms.Middleware;
using LinCms.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LinCms.Controllers.Cms
{
    /// <summary>
    ///  账号
    /// </summary>
    [ApiExplorerSettings(GroupName = "cms")]
    [AllowAnonymous]
    [ApiController]
    [Route("cms/user")]
    public class AccountController : ApiControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IAccountService _accountService;
        private readonly IAuditBaseRepository<BlackRecord> _blackRecordRepository;
        public AccountController(IComponentContext componentContext, IConfiguration configuration, IAccountService accountService, IAuditBaseRepository<BlackRecord> blackRecordRepository)
        {
            bool isIdentityServer4 = configuration.GetSection("Service:IdentityServer4").Value?.ToBoolean() ?? false;
            _tokenService = componentContext.ResolveNamed<ITokenService>(isIdentityServer4 ? nameof(IdentityServer4Service) : nameof(JwtTokenService));
            _accountService = accountService;
            _blackRecordRepository = blackRecordRepository;
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="loginInputDto">用户名/密码：admin/123qwe</param>
        [Logger("{context.ActionArguments.loginInputDto.Username}登录后台系统")]
        [ServiceFilter(typeof(RecaptchaVerifyActionFilter))]
        [HttpPost("login")]
        public Task<Tokens> Login(LoginInputDto loginInputDto)
        {
            return _tokenService.LoginAsync(loginInputDto);
        }

        /// <summary>
        /// 刷新用户的token
        /// </summary>
        /// <returns></returns>
        [HttpGet("refresh")]
        public async Task<Tokens> GetRefreshTokenAsync()
        {
            string? refreshToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            if (refreshToken == null)
            {
                throw new LinCmsException("请先登录.", ErrorCode.RefreshTokenError);
            }

            return await _tokenService.GetRefreshTokenAsync(refreshToken);
        }

        /// <summary>
        /// 退出登录时，将AccessToken插入到BlackRecord表
        /// </summary>
        /// <returns></returns>
        [HttpGet("logout")]
        [Authorize]
        public async Task<UnifyResponseDto> LogoutAsync()
        {
            var username = User.FindUserName();
            string? Jti = await HttpContext.GetTokenAsync("Bearer", "access_token");
            //string Jti = Request.Headers["Authorization"].ToString().Substring(JwtBearerDefaults.AuthenticationScheme.Length + 1).Trim();
            await _blackRecordRepository.InsertAsync(new BlackRecord { Jti = Jti, UserName = username });
            return UnifyResponseDto.Success("退出登录");
        }



        /// <summary>
        /// 注册前先发送邮件才能正常注册
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>
        [HttpPost("account/send_email_code")]
        public async Task<string> SendEmailCodeAsync([FromBody] RegisterEmailCodeInput registerDto)
        {
            return await _accountService.SendEmailCodeAsync(registerDto);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="registerDto"></param>
        /// <param name="mapper"></param>
        /// <param name="userSevice"></param>
        [Logger("用户注册")]
        [ServiceFilter(typeof(RecaptchaVerifyActionFilter))]
        [HttpPost("account/register")]
        public async Task<UnifyResponseDto> Register([FromBody] RegisterDto registerDto, [FromServices] IMapper mapper, [FromServices] IUserService userSevice)
        {
            string uuid = await RedisHelper.GetAsync("SendEmailCode." + registerDto.Email);

            if (uuid != registerDto.EmailCode)
            {
                return UnifyResponseDto.Error("非法请求");
            }

            string verificationCode = await RedisHelper.GetAsync("SendEmailCode.VerificationCode" + registerDto.Email);
            if (verificationCode != registerDto.VerificationCode)
            {
                return UnifyResponseDto.Error("验证码不正确");
            }

            LinUser user = mapper.Map<LinUser>(registerDto);
            await userSevice.CreateAsync(user, new List<long>(), registerDto.Password);
            return UnifyResponseDto.Success("注册成功");
        }

        /// <summary>
        /// 发送邮件：重置密码的验证码
        /// </summary>
        /// <param name="sendEmailCode"></param>
        /// <returns></returns>
        [Logger("发送了重置密码的验证码的邮件")]
        [HttpPost("account/send_password_reset_code")]
        public async Task<string> SendPasswordResetCodeAsync([FromBody] SendEmailCodeInput sendEmailCode)
        {
            return await _accountService.SendPasswordResetCodeAsync(sendEmailCode);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="resetPassword"></param>
        /// <returns></returns>
        [Logger("修改了密码")]
        [HttpPost("account/reset_password")]
        public Task ResetPasswordAsync([FromBody] ResetEmailPasswordDto resetPassword)
        {
            return _accountService.ResetPasswordAsync(resetPassword);
        }
    }


}
