using System.Net.Mail;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IEmailService
    {
        void Send(MailMessage message);
    }
}