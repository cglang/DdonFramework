namespace Ddon.Identity.Manager.Dtos
{
    public class RefreshTokenInPutDto
    {
        public RefreshTokenInPutDto(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
