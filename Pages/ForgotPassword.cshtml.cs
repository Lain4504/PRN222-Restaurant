using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;

public class ForgotPasswordModel : PageModel
{
    private readonly IConfiguration _config;

    public ForgotPasswordModel(IConfiguration config)
    {
        _config = config;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var verificationCode = new Random().Next(100000, 999999).ToString();

        // Lưu vào TempData
        TempData["ResetCode"] = verificationCode;
        TempData["Email"] = Input.Email;

        try
        {
            string smtpHost = _config["Smtp:Host"];
            int smtpPort = int.Parse(_config["Smtp:Port"]);
            string senderEmail = _config["Smtp:SenderEmail"];
            string senderPassword = _config["Smtp:SenderPassword"];
            string senderName = _config["Smtp:SenderName"];
            bool enableSsl = bool.Parse(_config["Smtp:EnableSsl"]);

            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Password Reset Code",
                Body = $"Your verification code is: {verificationCode}",
                IsBodyHtml = false
            };

            mail.To.Add(Input.Email);

            await smtpClient.SendMailAsync(mail);

            Message = "Verification code sent to your email.";
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error sending email: " + ex.Message;
        }

        return Page();
    }
}
