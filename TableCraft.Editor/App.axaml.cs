using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TableCraft.Editor.Services;
using TableCraft.Editor.ViewModels;
using TableCraft.Editor.Views;
using Serilog;
using TableCraft.Editor.Models;

namespace TableCraft.Editor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (!Core.Configuration.IsInited)
            {
                const string errorMessage = $"library's initialization failed, please check '{Program.LibEnvJsonFilename}'";
                MessageBoxManager.ShowStandardMessageBox(MessageBoxManager.ErrorTitle, errorMessage);
                Log.Error(errorMessage);
                // return here so main window will not be opened
                return;
            }

            var listJsonFilePath = AppContext.BaseDirectory + Program.ListJsonFilename;
            if (!File.Exists(listJsonFilePath))
            {
                throw new FileNotFoundException(listJsonFilePath + " not found, construct config file registry failed");
            }

            var configFileRegistry = new ConfigFileRegistry(listJsonFilePath);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(configFileRegistry),
            };
        }

        base.OnFrameworkInitializationCompleted();
        Task.Run(LoadAndRegisterVersionControl);
        if (Program.Host.Services.GetRequiredService<AppSettings>().CheckForUpdates)
        {
            Task.Run(CheckForUpdatesOnStartup);
        }
    }

    private static void LoadAndRegisterVersionControl()
    {
        var versionControlConfig = Program.Host.Services.GetRequiredService<IP4ConfigManager>().GetVersionControlConfig();
        if (!versionControlConfig.IsReady())
        {
            Log.Information("[App.LoadAndRegisterVersionControl] perforce config not ready, exit");
            return;
        }

        var versionControl = new Core.VersionControl.Perforce(versionControlConfig);
        Core.IO.FileHelper.RegisterFileEvent(versionControl);
    }

    private static async void CheckForUpdatesOnStartup()
    {
        try
        {
            var autoUpdateService = Program.Host.Services.GetRequiredService<IAutoUpdateService>();
            await autoUpdateService.CheckLocalDownloadedUpdatesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[App.CheckForUpdatesOnStartup] Failed to check for updates");
        }
    }
}