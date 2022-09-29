namespace Ddon.Jwt
{
    public class Tokens
    {
        public Tokens(string accessToken, RefreshToken refreshToken, int expiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }
        /// <summary>
        /// AccessToken
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public int ExpiresIn { get; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        public RefreshToken RefreshToken { get; }
    }
}
