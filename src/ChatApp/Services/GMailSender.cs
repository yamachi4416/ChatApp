using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ChatApp.Services
{
    public class MailOptions
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public MailAddress FromAddress => new MailAddress(Email, UserName);

        public NetworkCredential Credentials => new NetworkCredential(Email, Password);
    }

    public class GMailSender : IEmailSender
    {
        public MailOptions Options { get; }

        public GMailSender(IOptions<MailOptions> options)
        {
            Options = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var sc = new SmtpClient(Options.Host, Options.Port))
            using (var msg = new MailMessage(Options.FromAddress, new MailAddress(email)))
            {
                //SMTPサーバーを設定する
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;

                //ユーザー名とパスワードを設定する
                sc.Credentials = Options.Credentials;

                //SSLを使用する
                sc.EnableSsl = true;

                // HTML形式
                msg.IsBodyHtml = true;

                // 件名を設定
                msg.Subject = subject;

                // メッセージを設定
                msg.Body = message;

                //メッセージを送信する
                await sc.SendMailAsync(msg);
            }
        }
    }
}