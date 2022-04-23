using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Ddon.Mail
{
    public class MailKitSmtpEmailSender : IDdonMail
    {
        private readonly DdonMailOptions Options;

        public MailKitSmtpEmailSender(IOptions<DdonMailOptions> options)
        {
            Options = options.Value;
        }

        public async Task SendEmailAsync(MailMessage mailBody)
        {
            using var client = await BuildClientAsync();
            var message = MimeMessage.CreateFromMailMessage(mailBody);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendEmailAsync(DdonMailMessage mailBody)
        {
            using var client = await BuildClientAsync();
            var message = BuildMailMessage(mailBody);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task<SmtpClient> BuildClientAsync()
        {
            var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(Options.SmtpHost, Options.SmtpPort);
                await client.AuthenticateAsync(Options.MailAccount, Options.MailPassword);
                return client;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 组装邮件文本/附件邮件信息
        /// </summary>
        /// <param name="mailBody">邮件消息实体</param>
        /// <returns></returns>
        private MimeMessage BuildMailMessage(DdonMailMessage mailBody)
        {
            //设置邮件基本信息
            var minMessag = new MimeMessage();
            //插入发件人
            minMessag.From.Add(new MailboxAddress(mailBody.SenderName ?? Options.MailAccount, Options.MailAccount));

            //插入收件人
            if (mailBody.Recipients.Any())
            {
                foreach (var recipients in mailBody.Recipients)
                {
                    minMessag.To.Add(new MailboxAddress(recipients, recipients));
                }
            }

            //插入抄送人
            if (mailBody.Cc != null && mailBody.Cc.Any())
            {
                foreach (var cc in mailBody.Cc)
                {
                    minMessag.Cc.Add(new MailboxAddress(cc, cc));
                }
            }

            //插入密送人
            if (mailBody.Bcc != null && mailBody.Bcc.Any())
            {
                foreach (var bcc in mailBody.Bcc)
                {
                    minMessag.Bcc.Add(new MailboxAddress(bcc, bcc));
                }
            }

            //插入主题
            minMessag.Subject = mailBody.Subject;

            var multipart = new Multipart("mixed");
            //插入文本消息
            if (!string.IsNullOrEmpty(mailBody.Body))
            {
                var textBody = new TextPart(mailBody.MailBodyType);
                textBody.SetText(Encoding.Default, mailBody.Body);
                var alternative = new MultipartAlternative(textBody);
                multipart.Add(alternative);
            }

            //组合邮件内容
            minMessag.Body = multipart;
            return minMessag;
        }
    }
}
