using DugnadAppMvc.Services.Interfaces;
using Renci.SshNet;

namespace DugnadAppMvc.Services;

public class DatabaseBackupService : IDatabaseBackupService
{
    private readonly IConfiguration _configuration;

    public DatabaseBackupService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private (string Host, int Port, string Username, string Password) GetSshSettings()
    {
        var host = _configuration["BackupSettings:Host"];
        var username = _configuration["BackupSettings:Username"];
        var password = _configuration["BackupSettings:Password"];
        var port = _configuration.GetValue<int>("BackupSettings:SshPort", 22);

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "BackupSettings mangler i konfigurasjonen.");
        }

        return (host, port, username, password);
    }

    public Task<string> CreateBackupAsync()
    {
        var settings = GetSshSettings();

        using var ssh = new SshClient(
            settings.Host,
            settings.Port,
            settings.Username,
            settings.Password);

        ssh.Connect();

        var command = ssh.RunCommand(
            "sudo /usr/local/bin/backup_dugnadapp.sh");

        if (command.ExitStatus != 0)
        {
            throw new InvalidOperationException(
                $"Backup feilet: {command.Error}");
        }

        ssh.Disconnect();

        return Task.FromResult(command.Result.Trim());
    }
}