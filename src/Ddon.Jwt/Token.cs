using Ddon.Cache;
using Ddon.Core;
using Ddon.Jwt.Dtos;
using Ddon.Jwt.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ddon.Jwt
{
    public class Token : IToken
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ICache _cache;

        public Token(JwtSettings jwtSettings, ICache cache)
        {
            _jwtSettings = jwtSettings;
            _cache = cache;
        }

        public SecurityToken SecurityToken()
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecurityKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                }),
                IssuedAt = DateTime.Now,
                NotBefore = DateTime.Now,
                Expires = DateTime.Now.Add(_jwtSettings.ExpiresIn),
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
        /// <returns></returns>
        public async Task<TokenDto> GenerateJwtTokenAsync()
        {
            var tokens = GenerateTokens();

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
        /// <returns></returns>
        private Tokens GenerateTokens()
        {
            var securityToken = SecurityToken();

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var accessToken = jwtTokenHandler.WriteToken(securityToken);

            var refreshToken = RefreshToken();

            var refreshTokenDto = new RefreshToken(securityToken.Id, refreshToken)
            {
                CreationTime = DateTime.Now,
                ExpiryTime = DateTime.Now.AddMonths(6),
            };

            return new Tokens(accessToken, refreshTokenDto, (int)_jwtSettings.ExpiresIn.TotalSeconds);
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
