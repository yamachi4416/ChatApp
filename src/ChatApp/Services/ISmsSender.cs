using System.Threading.Tasks;

namespace ChatApp.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
