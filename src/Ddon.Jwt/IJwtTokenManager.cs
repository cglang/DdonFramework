using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Ddon.Jwt;

public interface IJwtTokenManager
{
    /// <summary>
    /// 生成 Token
    /// </summary>
    string GenerateToken(IEnumerable<Claim>? claims = null);

    /// <summary>
    /// 验证 Token
    /// </summary>
    ClaimsPrincipal ValidateAccessToken(string token, out SecurityToken? validatedToken);

    /// <summary>
    /// 通过 TefreshToken 生成 Token
    /// </summary>
    Task<string> GenerateTokenAsync(string refreshToken, CancellationToken token = default);

    /// <summary>
    /// 生成联合Token
    /// </summary>
    Task<MultiToken> GenerateMultiTokenAsync(IEnumerable<Claim>? claims = null, CancellationToken token = default);

    /// <summary>
    /// 生成联合Token
    /// </summary>
    Task<MultiToken> GenerateMultiTokenAsync(string refreshToken, CancellationToken token = default);

}
