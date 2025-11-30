using System.Threading.Tasks;

namespace TableCraft.Editor.Services;

public interface IAutoUpdateService
{
    Task CheckLocalDownloadedUpdatesAsync();
    Task<bool> CheckForNewReleaseAsync();
    Task<bool> DownloadAndInstallUpdateAsync();
}