using System.Net.Mail;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure
{
    /// <summary>
    /// This class sends an email to localhost smtp server. Use a fake smtp server and only use for debugging.
    /// </summary>
    public class FakeEmailService : IEmailService
    {
        public async Task SendAsync(MailMessage message)
        {
            using (var smtp = new SmtpClient("localhost"))
                await smtp.SendMailAsync(message);
        }
    }
}