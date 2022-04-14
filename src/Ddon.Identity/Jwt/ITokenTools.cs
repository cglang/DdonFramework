using Ddon.Identity.Entities;
using Ddon.Identity.Manager.Dtos;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Identity.Jwt
{
    public interface ITokenTools<TKey> where TKey : IEquatable<TKey>
    {
        Task<TokenDto> GenerateJwtTokenAsync(User<TKey> user);

        SecurityToken SecurityToken(User<TKey> user);

        string RefreshToken(int len = 32);

        ClaimsPrincipal? GetClaimsPrincipalByToken(string token);
    }
}
