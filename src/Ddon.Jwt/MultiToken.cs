namespace Ddon.Jwt
{
    public class MultiToken
    {
        public MultiToken(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// 验证 Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 刷新 Token
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
