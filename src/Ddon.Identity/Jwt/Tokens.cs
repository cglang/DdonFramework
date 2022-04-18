using System;

namespace Ddon.Identity.Jwt
{
    public class Tokens<TKey> where TKey : IEquatable<TKey>
    {
        public Tokens(string accessToken, RefreshToken<TKey> refreshToken, int expiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }
        /// <summary>
        /// AccessToken
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        public RefreshToken<TKey> RefreshToken { get; set; }
    }
}
