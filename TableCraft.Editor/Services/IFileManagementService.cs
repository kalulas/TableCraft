using System.Collections.Generic;
using System.Threading.Tasks;

namespace TableCraft.Editor.Services;

public interface IFileManagementService
{
    string GetUpdateDirectory();
    List<string> GetLocalSetupFiles();
    bool CleanupOldSetupFiles(int maxFilesToKeep = 3);
    string? FindLatestLocalSetup();
}