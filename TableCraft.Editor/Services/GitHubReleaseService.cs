using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using TableCraft.Editor.Models;

namespace TableCraft.Editor.Services;

public class GitHubReleaseService : IReleaseService
{
    private readonly HttpClient m_HttpClient;
    private readonly ILogger m_Logger;

    public GitHubReleaseService(HttpClient httpClient)
    {
        m_HttpClient = httpClient;
        m_HttpClient.DefaultRequestHeaders.Add("User-Agent", "TableCraft-Editor");
        m_Logger = Log.ForContext<GitHubReleaseService>();
    }

    public async Task<ReleaseInfo?> GetLatestReleaseAsync(string owner, string repository)
    {
        try
        {
            var url = $"https://api.github.com/repos/{owner}/{repository}/releases/latest";
            var response = await m_HttpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                m_Logger.Warning("GitHub API request failed with status {StatusCode} for '{Owner}/{Repository}'", 
                    response.StatusCode, owner, repository);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var releaseData = JsonSerializer.Deserialize<JsonElement>(json);

            var release = new ReleaseInfo
            {
                TagName = releaseData.GetProperty("tag_name").GetString() ?? string.Empty,
                Name = releaseData.GetProperty("name").GetString() ?? string.Empty,
                Body = releaseData.GetProperty("body").GetString() ?? string.Empty,
                IsPrerelease = releaseData.GetProperty("prerelease").GetBoolean(),
                PublishedAt = releaseData.GetProperty("published_at").GetDateTime()
            };

            if (releaseData.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var assetElement in assetsElement.EnumerateArray())
                {
                    var asset = new ReleaseAsset
                    {
                        Name = assetElement.GetProperty("name").GetString() ?? string.Empty,
                        DownloadUrl = assetElement.GetProperty("browser_download_url").GetString() ?? string.Empty,
                        Size = assetElement.GetProperty("size").GetInt64(),
                        ContentType = assetElement.GetProperty("content_type").GetString() ?? string.Empty
                    };
                    release.Assets.Add(asset);
                }
            }

            return release;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to get latest release for '{Owner}/{Repository}'", owner, repository);
            return null;
        }
    }
}