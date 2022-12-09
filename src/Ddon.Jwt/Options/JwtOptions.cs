using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ddon.Jwt.Options
{
    public class JwtOptions
    {
        /// <summary>
        /// 签名密钥
        /// </summary>
        public string Security { get; set; } = string.Empty;

        /// <summary>
        /// SecurityKey
        /// </summary>
        public SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Security));

        /// <summary>
        /// AccessToken 有效期
        /// </summary>
        public TimeSpan AccessTokenExpires { get; set; } = TimeSpan.FromHours(JwtDefaults.ExpiresHour);

        /// <summary>
        /// RefreshToken 有效期
        /// </summary>
        public TimeSpan RefreshTokenExpires { get; set; } = TimeSpan.FromDays(JwtDefaults.ExpiresDay);

        /// <summary>
        /// 颁发者
        /// </summary>
        public string Issuer { get; set; } = JwtDefaults.Issuer;

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; } = JwtDefaults.Audience;

        /// <summary>
        /// CookieKey
        /// </summary>
        public string CookieKey { get; set; } = JwtDefaults.CookieKey;
    }
}
