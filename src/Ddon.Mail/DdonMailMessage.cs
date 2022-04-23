using MimeKit.Text;

namespace Ddon.Mail
{
    /// <summary>
    /// 邮件内容实体
    /// </summary>
    public class DdonMailMessage
    {
        public DdonMailMessage(string? senderName, string subject, string? body, params string[] recipients)
        {
            Recipients = recipients;
            SenderName = senderName;
            Subject = subject;
            Body = body;
        }

        /// <summary>
        /// 邮件内容类型
        /// </summary>
        public TextFormat MailBodyType { get; set; } = TextFormat.Html;

        /// <summary>
        /// 收件人
        /// </summary>
        public string[] Recipients { get; set; }

        /// <summary>
        /// 抄送
        /// </summary>
        public string[]? Cc { get; set; }

        /// <summary>
        /// 密送
        /// </summary>
        public string[]? Bcc { get; set; }

        /// <summary>
        /// 发件人名称
        /// </summary>
        public string? SenderName { get; set; }

        /// <summary>
        /// 邮件主题
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 邮件内容
        /// </summary>
        public string? Body { get; set; }
    }
}
