using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using Avalonia.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TableCraft.Editor.Models;
using TableCraft.Editor.Services;

namespace TableCraft.Editor;

class Program
{
    public const string ListJsonFilename = "list.json";
    public const string LibEnvJsonFilename = "libenv.json";
    public static IHost Host { get; private set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Initialize logger first to ensure error logging works even if appsettings.json is invalid
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10)
            .CreateLogger();

        Log.Information("Logger is ready");

        try
        {
            // Move host and configuration initialization into try-catch after logger is ready
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IP4ConfigManager, P4ConfigManager>();
                    services.AddSingleton<AppSettings>(provider =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();
                        var appSettings = configuration.Get<AppSettings>();
                        if (appSettings == null)
                        {
                            throw new InvalidOperationException("Failed to load application settings from appsettings.json");
                        }
                        return appSettings;
                    });
                })
                .Build();

            // Trigger AppSettings registration to validate configuration early
            Host.Services.GetRequiredService<AppSettings>();
            Log.Information("Configuration is ready");
            
            var libEnvJsonFilePath = Path.Combine(AppContext.BaseDirectory, LibEnvJsonFilename);
            if (!File.Exists(libEnvJsonFilePath))
            {
                throw new FileNotFoundException("library configuration file not found", LibEnvJsonFilename);
            }
            
            // we initialize library here and build Avalonia app anyway,
            // later we check if it's initialized in App.OnFrameworkInitializationCompleted
            Core.Debugger.InitialCustomLogger(Log.Information, Core.Debugger.LogLevel.Information);
            Core.Debugger.InitialCustomLogger(Log.Warning, Core.Debugger.LogLevel.Warning);
            Core.Debugger.InitialCustomLogger(Log.Error, Core.Debugger.LogLevel.Error);
            Log.Information("try setup TableCraft.Core with: {LibEnvJson}", libEnvJsonFilePath);
            Core.Configuration.ReadConfigurationFromJson(libEnvJsonFilePath);
            Log.Information("TableCraft.Core is ready");

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            HandleException(e);
        }
        finally
        {
            Log.Information("Application shutdown");
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace(LogEventLevel.Debug, LogArea.Control)
            .UseReactiveUI();

    #region Logger

    public static void HandleException(Exception e)
    {
        Log.Error(e, "Unhandled exception");
#pragma warning disable CS4014
        MessageBoxManager.ShowStandardMessageBoxDialog("Unhandled exception", e.ToString());
#pragma warning restore CS4014
    }

    #endregion
}