using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Ddon.Identity.Manager.Dtos
{
    public class TokenDto
    {
        /// <summary>
        /// Success
        /// </summary>
        public bool Success => Errors == null || !Errors.Any();

        /// <summary>
        ///Errors
        /// </summary>
        [AllowNull]
        public IEnumerable<string> Errors { get; set; }

        /// <summary>
        /// AccessToken
        /// </summary>
        [AllowNull]
        public string AccessToken { get; set; }

        /// <summary>
        /// TokenType
        /// </summary>
        [AllowNull]
        public string TokenType { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        [AllowNull]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        [AllowNull]
        public string RefreshToken { get; set; }
    }
}
