﻿using DotNetCore.Security;
using LinCms.Cms.Account;
using LinCms.Data.Enums;
using LinCms.Entities;
using LinCms.Exceptions;
using LinCms.IRepositories;
using LinCms.Security;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LinCms.Cms.Users
{
    public class JwtTokenService : ITokenService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly IJsonWebTokenService _jsonWebTokenService;

        private readonly IServiceProvider serviceProvider;

        public JwtTokenService(IUserRepository userRepository, ILogger<JwtTokenService> logger, IUserIdentityService userIdentityService, IJsonWebTokenService jsonWebTokenService, IServiceProvider serviceProvider)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userIdentityService = userIdentityService;
            _jsonWebTokenService = jsonWebTokenService;
            this.serviceProvider = serviceProvider;
        }
        /// <summary>
        /// JWT登录
        /// </summary>
        /// <param name="loginInputDto"></param>
        /// <returns></returns>
        public async Task<Tokens> LoginAsync(LoginInputDto loginInputDto)
        {
            _logger.LogInformation("JwtLogin");

            LinUser user = await _userRepository.GetUserAsync(r => r.Username == loginInputDto.Username);

            if (user == null)
            {
                throw new LinCmsException("用户不存在", ErrorCode.NotFound);
            }

            bool valid = await _userIdentityService.VerifyUserPasswordAsync(user.Id, loginInputDto.Password);

            if (!valid)
            {
                throw new LinCmsException("请输入正确密码", ErrorCode.ParameterError);
            }


            List<Claim> claims = new List<Claim>()
            {
                new Claim (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim (ClaimTypes.Email, user.Email?? ""),
                new Claim (ClaimTypes.GivenName, user.Nickname?? ""),
                new Claim (ClaimTypes.Name, user.Username?? "")
            };

            user.LinGroups?.ForEach(r =>
            {
                claims.Add(new Claim(ClaimTypes.Role, r.Name));
                claims.Add(new Claim(LinCmsClaimTypes.Groups, r.Id.ToString()));
            });

            _logger.LogInformation($"用户{loginInputDto.Username},登录成功，{JsonConvert.SerializeObject(claims)}");

            string token = _jsonWebTokenService.Encode(claims);

            var refreshToken = GenerateToken();
            user.AddRefreshToken(refreshToken);
            await _userRepository.UpdateAsync(user);

            return new Tokens(token, refreshToken);
        }

        private string GenerateToken(int size = 32)
        {
            var randomNumber = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<Tokens> GetRefreshTokenAsync(string refreshToken)
        {
            LinUser user = await _userRepository.GetUserAsync(r => r.RefreshToken == refreshToken);

            if (user.IsNull())
            {
                throw new LinCmsException("该refreshToken无效!");
            }

            if (DateTime.Compare(user.LastLoginTime, DateTime.Now) > new TimeSpan(30, 0, 0, 0).Ticks)
            {
                throw new LinCmsException("请重新登录", ErrorCode.RefreshTokenError);
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim (ClaimTypes.Email, user.Email?? ""),
                new Claim (ClaimTypes.GivenName, user.Nickname?? ""),
                new Claim (ClaimTypes.Name, user.Username?? ""),
            };

            _logger.LogInformation($"用户{user.Username},JwtRefreshToken 刷新-登录成功，{JsonConvert.SerializeObject(claims)}");

            string token = _jsonWebTokenService.Encode(claims);

            refreshToken = GenerateToken();
            user.AddRefreshToken(refreshToken);
            await _userRepository.UpdateAsync(user);

            return new Tokens(token, refreshToken);
        }
    }
}