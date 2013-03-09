using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure
{
    /// <summary>
    /// This class sends an email to localhost smtp server. Will ignore erros if smtp is not present. Only use for debugging.
    /// </summary>
    public class FakeEmailService : IEmailService
    {
        public async Task SendAsync(MailMessage message)
        {
            try
            {
                using (var smtp = new SmtpClient("localhost"))
                    await smtp.SendMailAsync(message);
            }
            catch (Exception)
            {}
        }
    }
}