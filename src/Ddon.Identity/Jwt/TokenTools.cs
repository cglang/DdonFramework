using Ddon.Cache;
using Ddon.Core;
using Ddon.Core.JwtBearer;
using Ddon.Core.Models;
using Ddon.Identity.Entities;
using Ddon.Identity.Manager.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ddon.Identity.Jwt
{
    public class TokenTools<TKey> : ITokenTools<TKey> where TKey : IEquatable<TKey>
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ICache _cache;

        public TokenTools(JwtSettings jwtSettings, ICache cache)
        {
            _jwtSettings = jwtSettings;
            _cache = cache;
        }

        public SecurityToken SecurityToken(User<TKey> user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecurityKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString() ?? string.Empty)
                }),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.Add(_jwtSettings.ExpiresIn),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtTokenHandler.CreateToken(tokenDescriptor);
            return securityToken;
        }

        public string RefreshToken(int len = 32)
        {
            return GenerateRefreshToken(len);
        }

        /// <summary>
        /// 生成JwtToken
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<TokenDto> GenerateJwtTokenAsync(User<TKey> user)
        {
            var tokens = GenerateTokens(user);

            DistributedCacheEntryOptions distributedCacheEntryOptions = new()
            {
                SlidingExpiration = _jwtSettings.TokenCacheExpiration
            };
            await _cache.SetAsync($"{CacheKey.RefreshTokenKey}{tokens.RefreshToken.Token}", tokens.RefreshToken, distributedCacheEntryOptions);

            await Task.CompletedTask;
            return new TokenDto()
            {
                AccessToken = tokens.AccessToken,
                TokenType = "Bearer",
                RefreshToken = tokens.RefreshToken.Token,
                ExpiresIn = tokens.ExpiresIn,
            };
        }

        /// <summary>
        /// 生成 AccessToken 和 RefreshToken 集合对象
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private Tokens<TKey> GenerateTokens(User<TKey> user)
        {
            var securityToken = SecurityToken(user);

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var accessToken = jwtTokenHandler.WriteToken(securityToken);

            var refreshToken = RefreshToken();

            var refreshTokenDto = new RefreshToken<TKey>(securityToken.Id, refreshToken)
            {
                UserId = user.Id,
                CreationTime = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMonths(6),
            };

            return new Tokens<TKey>(accessToken, refreshTokenDto, (int)_jwtSettings.ExpiresIn.TotalSeconds);
        }

        /// <summary>
        /// 生成RefreshToken
        /// </summary>
        /// <param name="len">长度</param>
        /// <returns></returns>
        private static string GenerateRefreshToken(int len = 32)
        {
            var randomNumber = new byte[len];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// 通过Token获取ClaimsPrincipal
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClaimsPrincipal? GetClaimsPrincipalByToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecurityKey)),
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = false // 不验证过期时间！！！
                };

                var jwtTokenHandler = new JwtSecurityTokenHandler();

                var claimsPrincipal =
                    jwtTokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                var validatedSecurityAlgorithm = validatedToken is JwtSecurityToken jwtSecurityToken
                                                 && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                                     StringComparison.InvariantCultureIgnoreCase);

                return validatedSecurityAlgorithm ? claimsPrincipal : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
