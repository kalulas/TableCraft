using System;
using System.Threading.Tasks;

namespace TableCraft.Editor.Services;

public interface IDownloadService
{
    Task<bool> DownloadFileAsync(string url, string destinationPath, IProgress<double>? progress = null);
}