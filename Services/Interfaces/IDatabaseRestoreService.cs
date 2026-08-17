namespace DugnadAppMvc.Services.Interfaces;

public interface IDatabaseRestoreService
{
    Task<List<string>> GetBackupsAsync();

    Task StartRestoreAsync(string backupFile);
}