namespace Ddon.Identity.Manager.Dtos
{
    public class AccessTokenInPutDto
    {
        public AccessTokenInPutDto(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
