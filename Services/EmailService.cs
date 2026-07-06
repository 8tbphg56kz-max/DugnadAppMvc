using DugnadAppMvc.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DugnadAppMvc.Services;

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body)
    {
        var email = new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail));

        email.To.Add(
            MailboxAddress.Parse(to));

        email.Subject = subject;

        email.Body = new TextPart("plain")
        {
            Text = body
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _settings.SmtpServer,
            _settings.Port,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _settings.Username,
            _settings.Password);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }

    public async Task SendMagicLinkAsync(
     string email,
     string link)
    {
        var body = $"""
Hei!

Klikk på lenken nedenfor for å logge inn i DugnadApp:

{link}

Hvis du ikke forsøkte å logge inn, kan du se bort fra denne e-posten.

Hilsen
DugnadApp
""";

        await SendAsync(
            email,
            "Logg inn i DugnadApp",
            body);
    }

    public async Task SendLoginCodeAsync(
    string epost,
    string kode)
    {
        var body = $"""
Hei!

Din innloggingskode til DugnadApp er:

{kode}

Koden er gyldig i 10 minutter.

Hilsen
DugnadApp
""";

        await SendAsync(
            epost,
            "Innloggingskode",
            body);
    }

    public async Task SendActivationEmailAsync(
    string email,
    string activationLink)
    {
        var body = $"""
Hei!

Velkommen til DugnadApp.

Klikk på lenken nedenfor for å aktivere kontoen din og velge passord.

{activationLink}

Hvis du ikke forventet denne e-posten, kan du se bort fra den.

Hilsen
DugnadApp
""";

        await SendAsync(
            email,
            "Aktiver kontoen din",
            body);
    }
}

