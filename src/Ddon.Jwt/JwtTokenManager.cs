using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Cache;
using Ddon.Jwt.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace Ddon.Jwt;

public class JwtTokenManager : IJwtTokenManager
{
    private readonly JwtOptions _jwtSettings;
    private readonly ICache _cache;
    private static readonly JwtSecurityTokenHandler _jwtTokenHandler = new();

    public JwtTokenManager(JwtOptions jwtSettings, ICache cache)
    {
        _jwtSettings = jwtSettings;
        _cache = cache;
    }

    public string GenerateToken(IEnumerable<Claim>? claims = null)
    {
        var claimsList = claims?.ToList() ?? new List<Claim>();
        if (claimsList.All(claim => claim.Type != JwtRegisteredClaimNames.Jti))
        {
            claimsList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
        }

        var nowDate = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = nowDate,
            NotBefore = nowDate,
            Expires = nowDate.Add(_jwtSettings.AccessTokenExpires),
            SigningCredentials = new SigningCredentials(_jwtSettings.SecurityKey, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var securityToken = _jwtTokenHandler.CreateToken(tokenDescriptor);

        return _jwtTokenHandler.WriteToken(securityToken);
    }

    public ClaimsPrincipal ValidateAccessToken(string token, out SecurityToken validatedToken)
    {
        var validateParameter = new TokenValidationParameters()
        {
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _jwtSettings.SecurityKey,
            ValidateLifetime = true
        };
        try
        {
            return _jwtTokenHandler.ValidateToken(token, validateParameter, out validatedToken);
        }
        catch
        {
            throw new Exception("验证未通过");
        }
    }

    public async Task<string> GenerateTokenAsync(string refreshToken, CancellationToken token = default)
    {
        var claims = await _cache.GetAsync<IEnumerable<Claim>>($"{JwtDefaults.RefreshTokenCacheKey}{refreshToken}", token);
        if (claims is null) throw new Exception("验证 RefreshToken 失败");

        return GenerateToken(claims);
    }

    public async Task<MultiToken> GenerateMultiTokenAsync(IEnumerable<Claim>? claims = null, CancellationToken token = default)
    {
        var accessToken = GenerateToken(claims);
        var refreshToken = GenerateRefreshToken(32);

        var cacheEntryOptions = new DistributedCacheEntryOptions() { SlidingExpiration = _jwtSettings.RefreshTokenExpires };
        await _cache.SetAsync($"{JwtDefaults.RefreshTokenCacheKey}{refreshToken}", claims ?? new List<Claim>(), cacheEntryOptions, token);

        return new(accessToken, refreshToken);
    }

    public async Task<MultiToken> GenerateMultiTokenAsync(string refreshToken, CancellationToken token = default)
    {
        var accessToken = await GenerateTokenAsync(refreshToken, token);
        return new(accessToken, refreshToken);
    }

    /// <summary>
    /// 生成 RefreshToken
    /// </summary>
    private static string GenerateRefreshToken(int len = 32)
    {
        var randomNumber = new byte[len];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
