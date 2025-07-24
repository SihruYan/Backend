using Backend.Environment;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Backend.Services;

public class MailKitService: IEmailService
{
    private readonly IConfiguration _config;
    private readonly FormNotifyOptions _options;


    public MailKitService(IConfiguration config,
        IOptions<FormNotifyOptions> options)
    {
        _options = options.Value;
        _config = config;
    }

    public async Task SendAdminNotificationAsync()
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("表單通知", _options.Sender));
        message.To.Add(new MailboxAddress("Admin", _options.AdminEmail));
        message.Subject = "🔔 收到一筆新表單提交";

        var bodyText = $"""
                        有人剛剛填寫了表單：
                        至進後台確認完整內容。
                        """;

        message.Body = new TextPart("plain") { Text = bodyText };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_options.SmtpHost, 587, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_options.SmtpUser, _options.SmtpPass);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}