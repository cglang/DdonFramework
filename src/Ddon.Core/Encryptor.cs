using System;
using System.Security.Cryptography;
using System.Text;

namespace Ddon.Core
{
    public static class Encryptor
    {
        public static string MD5Hash(string text)
        {
            using var md5Hash = MD5.Create();
            // Byte array representation of source string
            var sourceBytes = Encoding.UTF8.GetBytes(text);

            // Generate hash value(Byte Array) for input data
            var hashBytes = md5Hash.ComputeHash(sourceBytes);

            // Convert hash byte array to string
            var hash = BitConverter.ToString(hashBytes);

            return hash;
        }
    }
}
