using System.Threading.Tasks;
using ChatApp.Services;

namespace ChatApp.Test.Mock
{
    public class EmailSenderMock : IEmailSender
    {
        public static string _email;

        public static string _subject;

        public static string _message;

        public Task SendEmailAsync(string email, string subject, string message)
        {
            _email = email;
            _subject = subject;
            _message = message;

            return Task.FromResult(0);
        }
    }
}