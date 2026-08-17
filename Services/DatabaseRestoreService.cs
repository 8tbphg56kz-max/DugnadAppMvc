using DugnadAppMvc.Services.Interfaces;
using Renci.SshNet;
using System.Text.RegularExpressions;

namespace DugnadAppMvc.Services;

public class DatabaseRestoreService : IDatabaseRestoreService
{
    private readonly IConfiguration _configuration;

    public DatabaseRestoreService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<List<string>> GetBackupsAsync()
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
            "find /volume1/docker/postgres-backups " +
            "-maxdepth 1 " +
            "-type f " +
            "-name 'dugnadapp-*.backup' " +
            "-print");

        if (command.ExitStatus != 0)
        {
            throw new InvalidOperationException(
                $"Kunne ikke hente backupfiler: {command.Error}");
        }

        ssh.Disconnect();

        var backups = command.Result
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Select(Path.GetFileName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => x != "dugnadapp-before-restore.backup")
            .OrderByDescending(x => x)
            .ToList();

        return Task.FromResult(backups);
    }

    public Task StartRestoreAsync(string backupFile)
    {
        if (string.IsNullOrWhiteSpace(backupFile))
        {
            throw new ArgumentException(
                "Ingen backupfil er valgt.",
                nameof(backupFile));
        }

        // Backupfilen skal kun være et filnavn,
        // ikke en sti eller et shell-uttrykk.
        if (!Regex.IsMatch(
                backupFile,
                @"^[a-zA-Z0-9._-]+\.backup$"))
        {
            throw new ArgumentException(
                "Ugyldig backupfilnavn.",
                nameof(backupFile));
        }

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

        var logFile =
            $"/volume1/docker/postgres-backups/restore-web-{DateTime.Now:yyyyMMdd-HHmmss}.log";

        var commandText =
            $"nohup sudo /usr/local/bin/restore_dugnadapp.sh '{backupFile}' " +
            $"> '{logFile}' 2>&1 < /dev/null &";

        var command = ssh.RunCommand(commandText);

        if (command.ExitStatus != 0)
        {
            throw new InvalidOperationException(
                $"Kunne ikke starte restore: {command.Error}");
        }

        ssh.Disconnect();

        return Task.CompletedTask;
    }
}