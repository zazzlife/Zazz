using System.Net.Mail;

namespace Zazz.Core.Interfaces.Services
{
    public interface IEmailService
    {
        void Send(MailMessage message);
    }
}