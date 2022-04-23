using System.Net.Mail;
using System.Threading.Tasks;

namespace Ddon.Mail
{
    public interface IDdonMail
    {
        Task SendEmailAsync(MailMessage mailBody);

        Task SendEmailAsync(DdonMailMessage mailBody);
    }
}
