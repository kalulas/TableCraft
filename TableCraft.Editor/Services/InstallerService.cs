using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Serilog;

namespace TableCraft.Editor.Services;

public class InstallerService : IInstallerService
{
    private readonly ILogger m_Logger;

    public InstallerService()
    {
        m_Logger = Log.ForContext<InstallerService>();
    }

    public async Task<bool> ExecuteInstallerAsync(string installerPath)
    {
        try
        {
            if (!File.Exists(installerPath))
            {
                m_Logger.Warning("Installer file not found at {InstallerPath}", installerPath);
                return false;
            }

            m_Logger.Information("Starting installer execution: {InstallerPath}", installerPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                m_Logger.Error("Failed to start installer process");
                return false;
            }

            m_Logger.Information("Installer process started with PID {ProcessId}", process.Id);

            // Close current application after starting installer
            await CloseApplicationAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to execute installer at {InstallerPath}", installerPath);
            return false;
        }
    }

    private async Task CloseApplicationAsync()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                m_Logger.Information("Shutting down application for installer execution");
                
                // Allow some time for the installer to start
                await Task.Delay(1000);
                
                desktop.Shutdown();
            }
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to close application gracefully");
        }
    }
}