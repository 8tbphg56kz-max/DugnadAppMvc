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

    public Task<string> CreateBackupAsync()
    {
        var host = _configuration["BackupSettings:Host"];
        var username = _configuration["BackupSettings:Username"];
        var password = _configuration["BackupSettings:Password"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "BackupSettings mangler i konfigurasjonen.");
        }

        using var ssh = new SshClient(host, username, password);

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