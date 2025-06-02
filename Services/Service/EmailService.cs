using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpSection = _config.GetSection("Smtp");
        if (!smtpSection.Exists())
        {
            throw new Exception("SMTP configuration is missing in appsettings.json.");
        }

        var smtpHost = smtpSection["Host"];
        var smtpPort = int.TryParse(smtpSection["Port"], out var port) ? port : 587;
        var senderEmail = smtpSection["SenderEmail"];
        var senderPassword = smtpSection["SenderPassword"];
        var senderName = smtpSection["SenderName"] ?? "No-Reply";
        var enableSsl = bool.TryParse(smtpSection["EnableSsl"], out var ssl) && ssl;

        if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
        {
            throw new Exception("Invalid email configuration or recipient.");
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            await client.SendMailAsync(mail);
        }
        catch (SmtpException smtpEx)
        {
            throw new Exception("SMTP Error: " + smtpEx.Message, smtpEx);
        }
        catch (Exception ex)
        {
            throw new Exception("Error sending email: " + ex.Message, ex);
        }
    }
}
