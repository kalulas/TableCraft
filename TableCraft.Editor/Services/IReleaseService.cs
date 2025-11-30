using System.Threading.Tasks;
using TableCraft.Editor.Models;

namespace TableCraft.Editor.Services;

public interface IReleaseService
{
    Task<ReleaseInfo?> GetLatestReleaseAsync(string owner, string repository);
}