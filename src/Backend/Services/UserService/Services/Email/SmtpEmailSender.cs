using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using UserService.Services.Email.Abstractions;

namespace UserService.Services.Email;

public sealed class SmtpEmailSender(IOptions<SmtpEmailOptions> options) : IEmailSender
{
    private readonly SmtpEmailOptions smtpOptions = options.Value;

    public bool IsConfigured => smtpOptions.IsConfigured;

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (!smtpOptions.IsConfigured)
            throw new InvalidOperationException("SMTP email sender is not configured.");

        using var message = new MailMessage
        {
            From = new MailAddress(
                smtpOptions.SenderEmail!,
                string.IsNullOrWhiteSpace(smtpOptions.SenderName) ? smtpOptions.SenderEmail : smtpOptions.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(smtpOptions.Host!, smtpOptions.Port)
        {
            EnableSsl = smtpOptions.EnableSsl,
            Credentials = new NetworkCredential(smtpOptions.UserName, smtpOptions.Password)
        };

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
