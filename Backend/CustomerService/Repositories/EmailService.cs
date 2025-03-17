using System.Net.Mail;
using System.Net;

namespace CustomerService.Repositories
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _email;
        private readonly string _password;

        public EmailService(string smtpServer, int port, string email, string password)
        {
            _smtpServer = smtpServer;
            _port = port;
            _email = email;
            _password = password;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var message = new MailMessage(_email, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            using var client = new SmtpClient(_smtpServer, _port)
            {
                Credentials = new NetworkCredential(_email, _password),
                EnableSsl = false 
            };

            await client.SendMailAsync(message);
        }
    }
}
