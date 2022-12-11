using System.Security.Cryptography;
using System.Text;

namespace Ddon.Core.Utility
{
    public class DdonMd5
    {
        public static string CreateHash(string t)
        {
            var md5 = new StringBuilder(32);
            var bytHash = MD5.HashData(Encoding.UTF8.GetBytes(t));
            foreach (var item in bytHash)
            {
                md5.Append(item.ToString("x2"));
            }
            return md5.ToString();
        }
    }
}
