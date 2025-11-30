using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace TableCraft.Editor.Services;

public class AutoUpdateService : IAutoUpdateService
{
    private const string GitHubOwner = "kalulas";
    private const string GitHubRepository = "TableCraft";

    private readonly IReleaseService m_ReleaseService;
    private readonly IVersionService m_VersionService;
    private readonly IDownloadService m_DownloadService;
    private readonly IFileManagementService m_FileManagementService;
    private readonly IInstallerService m_InstallerService;
    private readonly ILogger m_Logger;

    public AutoUpdateService(
        IReleaseService releaseService,
        IVersionService versionService,
        IDownloadService downloadService,
        IFileManagementService fileManagementService,
        IInstallerService installerService)
    {
        m_ReleaseService = releaseService;
        m_VersionService = versionService;
        m_DownloadService = downloadService;
        m_FileManagementService = fileManagementService;
        m_InstallerService = installerService;
        m_Logger = Log.ForContext<AutoUpdateService>();
    }

    public async Task CheckLocalDownloadedUpdatesAsync()
    {
        try
        {
            m_Logger.Information("Checking for locally downloaded updates");
            
            var latestSetupFile = m_FileManagementService.FindLatestLocalSetup();
            if (string.IsNullOrEmpty(latestSetupFile))
            {
                m_Logger.Information("No local setup files found, checking for new releases");
                await CheckForNewReleaseAsync();
                return;
            }

            var fileName = Path.GetFileName(latestSetupFile);
            var confirmInstall = await MessageBoxManager.ShowConfirmationDialog(
                "Found Local Update", 
                $"A previously downloaded update was found: '{fileName}'.\nWould you like to install it now?");

            if (confirmInstall)
            {
                m_Logger.Information("User confirmed installation of local update: {SetupFile}", latestSetupFile);
                await m_InstallerService.ExecuteInstallerAsync(latestSetupFile);
            }
            else
            {
                m_Logger.Information("User declined installation of local update");
            }
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to check local downloaded updates");
        }
    }

    public async Task<bool> CheckForNewReleaseAsync()
    {
        try
        {
            m_Logger.Information("Checking for new releases from GitHub repository '{Owner}/{Repository}'", 
                GitHubOwner, GitHubRepository);
            
            var latestRelease = await m_ReleaseService.GetLatestReleaseAsync(GitHubOwner, GitHubRepository);
            if (latestRelease == null)
            {
                m_Logger.Warning("Failed to get latest release information");
                return false;
            }

            var currentVersion = m_VersionService.GetCurrentVersion();
            var latestVersion = m_VersionService.ParseVersion(latestRelease.TagName);

            if (latestVersion == null)
            {
                m_Logger.Warning("Failed to parse latest version: {TagName}", latestRelease.TagName);
                return false;
            }

            if (!m_VersionService.IsNewerVersion(currentVersion, latestVersion))
            {
                m_Logger.Information("Current version '{CurrentVersion}' is up to date with latest '{LatestVersion}'", 
                    currentVersion, latestVersion);
                return false;
            }

            m_Logger.Information("New version available '{LatestVersion}' (current '{CurrentVersion}')", 
                latestVersion, currentVersion);
            
            // var confirmDownload = await MessageBoxManager.ShowConfirmationDialog(
            const string newLine = "\r\n\r\n";
            var confirmDownload = await MessageBoxManager.ShowCustomMarkdownMessageBoxConfirmationDialog(
                "New Version Released", 
                $"New version [{latestRelease.TagName}] has been detected.{newLine}" +
                $"Would you like to download it now?{newLine}" +
                $"**Release Notes**{newLine}" +
                $"{latestRelease.Body}");

            if (confirmDownload)
            {
                return await DownloadAndInstallUpdateAsync();
            }

            m_Logger.Information("New version available '{LatestVersion}' but user declined download", latestVersion);
            return true;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to check for new releases");
            return false;
        }
    }

    public async Task<bool> DownloadAndInstallUpdateAsync()
    {
        try
        {
            var latestRelease = await m_ReleaseService.GetLatestReleaseAsync(GitHubOwner, GitHubRepository);
            if (latestRelease == null)
            {
                return false;
            }

            var setupAsset = latestRelease.Assets.FirstOrDefault(a => 
                a.Name.Contains("setup", StringComparison.OrdinalIgnoreCase) && 
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

            if (setupAsset == null)
            {
                m_Logger.Warning("No setup.exe file found in release assets");
                return false;
            }

            var updateDirectory = m_FileManagementService.GetUpdateDirectory();
            var destinationPath = Path.Combine(updateDirectory, setupAsset.Name);

            m_Logger.Information("Starting download '{FileName}' to {DestinationPath}", setupAsset.Name, destinationPath);

            var downloadSuccess = await m_DownloadService.DownloadFileAsync(
                setupAsset.DownloadUrl, 
                destinationPath);

            if (!downloadSuccess)
            {
                m_Logger.Error("Failed to download update file");
                return false;
            }

            // Cleanup old files after successful download
            m_FileManagementService.CleanupOldSetupFiles();

            var confirmInstall = await MessageBoxManager.ShowConfirmationDialog(
                "Update Downloaded", 
                $"New version [{latestRelease.TagName}] has been downloaded.\nWould you like to install it now?");

            if (confirmInstall)
            {
                m_Logger.Information("User confirmed installation of downloaded update");
                return await m_InstallerService.ExecuteInstallerAsync(destinationPath);
            }

            m_Logger.Information("Update downloaded but user declined installation");
            return true;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to download and install update");
            return false;
        }
    }
}