using DugnadAppMvc.Configuration;
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

    private string BuildHtmlEmail(
    string title,
    string introText,
    string buttonText,
    string buttonLink,
    string footerText)
    {
        return $"""
<html>
<body style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;">

<h2>{title}</h2>

<p>{introText}</p>

<p style="margin:30px 0;">
    <a href="{buttonLink}"
       style="
            background:#0d6efd;
            color:white;
            padding:12px 24px;
            text-decoration:none;
            border-radius:6px;
            display:inline-block;
            font-weight:bold;">
        {buttonText}
    </a>
</p>

<p>
Hvis knappen ikke fungerer, kan du kopiere denne lenken inn i nettleseren:
</p>

<p>
<a href="{buttonLink}">
{buttonLink}
</a>
</p>

<hr>

<p style="color:#666;font-size:14px;">
{footerText}
</p>

</body>
</html>
""";
    }

    private async Task SendAsync(
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

        email.Body = new TextPart("html")
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

    public async Task SendActivationEmailAsync(
     string email,
     string activationLink)
    {
        var body = BuildHtmlEmail(
            "Velkommen til DugnadApp",
            "Klikk på knappen nedenfor for å aktivere kontoen din og velge passord.",
            "Aktiver konto",
            activationLink,
            "Denne aktiveringslenken kan bare brukes én gang.");

        await SendAsync(
            email,
            "Aktiver kontoen din",
            body);
    }

    public async Task SendPasswordResetEmailAsync(
    string email,
    string resetLink)
    {
        var body = BuildHtmlEmail(
            "Tilbakestill passord",
            "Klikk på knappen nedenfor for å velge et nytt passord.",
            "Velg nytt passord",
            resetLink,
            "Hvis du ikke ba om å tilbakestille passordet, kan du se bort fra denne e-posten.");

        await SendAsync(
            email,
            "Tilbakestill passord",
            body);
    }

    public async Task SendNewActivityEmailAsync(
    string email,
    string type,
    string navn,
    string datoTekst,
    string link)
    {
        var title = type == "Oppgave"
            ? "Ny dugnadsoppgave i DugnadApp"
            : "Ny fellesdugnad i DugnadApp";

        var introText = type == "Oppgave"
            ? $"Styret har lagt ut en ny dugnadsoppgave: <strong>{navn}</strong>."
            : $"Styret har lagt ut en ny fellesdugnad: <strong>{navn}</strong>.";

        var body = BuildHtmlEmail(
            title,
            $"{introText}<br><br>{datoTekst}",
            type == "Oppgave"
                ? "Se oppgaven"
                : "Se dugnaden",
            link,
            "Du får denne e-posten fordi styret har lagt ut informasjon i DugnadApp.");

        await SendAsync(
            email,
            title,
            body);
    }
}

