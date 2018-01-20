using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using ChatApp.Services;

namespace ChatApp.Test.Mock
{
    public class MailMessageMock
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public AttachmentCollection Attachments { get; }
    }

    public class EmailSenderMock : IEmailSender
    {
        private static IList<MailMessageMock> _mailBox = new List<MailMessageMock>();

        private MailOptions Options { get; }

        public MailMessageMock GetLastMessage => _mailBox.LastOrDefault();

        public EmailSenderMock(IOptions<MailOptions> options)
        {
            Options = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessageMock
            {
                From = Options.Email,
                To = email,
                Subject = subject,
                Body = message
            };

            _mailBox.Add(mailMessage);

            return Task.FromResult(0);
        }
    }
}