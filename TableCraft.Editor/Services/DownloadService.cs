using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;

namespace TableCraft.Editor.Services;

public class DownloadService : IDownloadService
{
    private readonly HttpClient m_HttpClient;
    private readonly ILogger m_Logger;

    public DownloadService(HttpClient httpClient)
    {
        m_HttpClient = httpClient;
        m_Logger = Log.ForContext<DownloadService>();
    }

    public async Task<bool> DownloadFileAsync(string url, string destinationPath, IProgress<double>? progress = null)
    {
        try
        {
            m_Logger.Information("Starting download from {Url} to {DestinationPath}", url, destinationPath);

            var response = await m_HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                m_Logger.Warning("Download failed with status {StatusCode} for {Url}", response.StatusCode, url);
                return false;
            }

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var directory = Path.GetDirectoryName(destinationPath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);

            var buffer = new byte[131072]; // 128KB buffer for better performance
            var totalBytesRead = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;

                if (totalBytes > 0 && progress != null)
                {
                    var progressPercentage = (double)totalBytesRead / totalBytes;
                    progress.Report(progressPercentage);
                }
            }

            m_Logger.Information("Download completed successfully. File saved to {DestinationPath}, Size: {Size} bytes", 
                destinationPath, totalBytesRead);
            
            return true;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to download file from {Url} to {DestinationPath}", url, destinationPath);
            return false;
        }
    }
}