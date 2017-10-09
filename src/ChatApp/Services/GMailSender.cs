using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace ChatApp.Services
{
    public class GMailOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";

        public int Port { get; set; } = 587;

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string FromUser => $"{UserName}<{Email}>";
    }

    public class GMailSender : IEmailSender
    {
        public GMailOptions Options { get; }

        public GMailSender(IOptions<GMailOptions> options)
        {
            Options = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var sc = new SmtpClient(Options.Host, Options.Port))
            using (var msg = new MailMessage(Options.FromUser, email, subject, message))
            {
                //SMTPサーバーを設定する
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;

                //ユーザー名とパスワードを設定する
                sc.Credentials = new System.Net.NetworkCredential(Options.Email, Options.Password);

                //SSLを使用する
                sc.EnableSsl = true;

                // HTML形式
                msg.IsBodyHtml = true;

                //メッセージを送信する
                await sc.SendMailAsync(msg);
            }
        }
    }
}