namespace Ddon.Mail
{
    /// <summary>
    /// 邮件发送服务器配置
    /// </summary>
    public class DdonMailOptions
    {
        public DdonMailOptions()
        {
            SmtpHost = string.Empty;
            MailAccount = string.Empty;
            MailPassword = string.Empty;
        }

        /// <summary>
        /// 邮箱SMTP服务器地址
        /// </summary>
        public string SmtpHost { get; set; }

        /// <summary>
        /// 邮箱SMTP服务器端口
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// 是否启用IsSsl
        /// </summary>
        public bool IsSsl { get; set; }

        /// <summary>
        /// 邮箱账号
        /// </summary>
        public string MailAccount { get; set; }

        /// <summary>
        /// 邮箱密码
        /// </summary>
        public string MailPassword { get; set; }
    }
}
