namespace DugnadAppMvc.Services.Interfaces;

public interface IDatabaseBackupService
{
    Task<string> CreateBackupAsync();
}