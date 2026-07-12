using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;

namespace FoodPower.Features.Auth.Services.Emails;

public class EmailService(EmailSettings settings) : IEmailService
{
    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        using var smtp = new SmtpClient(settings.Host, settings.Port);
        smtp.Credentials = new NetworkCredential(
            settings.SenderAddress,
            settings.Password);
        smtp.EnableSsl = settings.UseSsl;

        var message = new MailMessage
        {
            From = new MailAddress(
                settings.SenderAddress,
                settings.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        message.To.Add(to);

        await smtp.SendMailAsync(message, cancellationToken);
    }
}
