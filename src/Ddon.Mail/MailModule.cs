using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Mail
{
    public class MailModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            var ddonMailOptions = configuration.GetSection(nameof(DdonMailOptions)).Get<DdonMailOptions>();
            services.Configure<DdonMailOptions>(options =>
            {
                options.SmtpHost = ddonMailOptions.SmtpHost;
                options.SmtpPort = ddonMailOptions.SmtpPort;
                options.IsSsl = ddonMailOptions.IsSsl;
                options.MailAccount = ddonMailOptions.MailAccount;
                options.MailPassword = ddonMailOptions.MailPassword;
            });

            services.AddTransient<IDdonMail, MailKitSmtpEmailSender>();
        }
    }
}
