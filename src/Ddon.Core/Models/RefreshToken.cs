using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Core.Models
{
    public class RefreshToken<TKey> where TKey : IEquatable<TKey>
    {
        public RefreshToken(string jwtId, string token)
        {
            JwtId = jwtId;
            Token = token;
        }

        [AllowNull]
        public string Id { get; set; }

        [Required]
        [StringLength(128)]
        public string JwtId { get; set; }

        [Required]
        [StringLength(256)]
        public string Token { get; set; }

        /// <summary>
        /// 是否开启一个RefreshToken只能使用一次
        /// </summary>
        [Required]
        public bool Used { get; set; }

        /// <summary>
        /// 是否失效。修改用户重要信息时可将此字段更新为true，使用户重新登录
        /// </summary>
        [Required]
        public bool Invalidated { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }

        [Required]
        public DateTime ExpiryTime { get; set; }

        [Required]
        [AllowNull]
        public TKey UserId { get; set; }
    }
}
