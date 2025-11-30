using System.Threading.Tasks;

namespace TableCraft.Editor.Services;

public interface IInstallerService
{
    Task<bool> ExecuteInstallerAsync(string installerPath);
}