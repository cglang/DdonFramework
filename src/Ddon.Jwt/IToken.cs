using Ddon.Jwt.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Jwt
{
    public interface IToken
    {
        Task<TokenDto> GenerateJwtTokenAsync();

        SecurityToken SecurityToken();

        string RefreshToken(int len = 32);

        ClaimsPrincipal? GetClaimsPrincipalByToken(string token);
    }
}
