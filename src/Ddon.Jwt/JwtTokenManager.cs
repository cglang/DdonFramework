using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Ddon.Jwt.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ddon.Jwt;

public class JwtTokenManager
{
    private readonly JwtSettings _jwtSettings;

    private SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecurityKey));

    private static JwtSecurityTokenHandler JwtTokenHandler => new();

    public JwtTokenManager(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    /// <summary>
    /// 生成JwtToken
    /// </summary>
    /// <returns></returns>
    public string GenerateJwtToken(IEnumerable<Claim>? claims = null)
    {
        var claimsList = claims?.ToList() ?? new List<Claim>();
        if (claimsList.All(claim => claim.Type != JwtRegisteredClaimNames.Jti))
        {
            claimsList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
        }

        var securityToken = GenerateSecurityToken(claimsList);

        return JwtTokenHandler.WriteToken(securityToken);
    }

    public bool ValidateToken(string token)
    {
        //校验token
        var validateParameter = new TokenValidationParameters()
        {
            // ValidateLifetime = true,
            // ValidateAudience = true,
            // ValidateIssuer = true,
            // ValidateIssuerSigningKey = true,
            // ValidIssuer = "fan",
            // ValidAudience = "audi~~!",
            IssuerSigningKey = SecurityKey,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        //不校验，直接解析token
        //jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token1);
        try
        {
            //校验并解析token
            JwtTokenHandler.ValidateToken(token, validateParameter, out _);
            // var claimsPrincipal =
            //     new JwtSecurityTokenHandler().ValidateToken(token, validateParameter,
            //         out var validatedToken); //validatedToken:解密后的对象
            // var jwtPayload = ((JwtSecurityToken)validatedToken).Payload.SerializeToJson(); //获取payload中的数据 
        }
        catch (SecurityTokenExpiredException)
        {
            //表示过期
            return false;
        }
        catch (SecurityTokenException)
        {
            //表示token错误
            return false;
        }

        return true;
    }

    private SecurityToken GenerateSecurityToken(IEnumerable<Claim> claims)
    {
        var nowDate = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            // IssuedAt = nowDate,
            // NotBefore = nowDate,
            Expires = nowDate.Add(_jwtSettings.ExpiresIn),
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = JwtTokenHandler.CreateToken(tokenDescriptor);
        return securityToken;
    }
}