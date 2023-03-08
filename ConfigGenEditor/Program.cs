using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using Avalonia.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigGenEditor;

class Program
{
    public const string ListJsonFilename = "list.json";
    public const string LibEnvJsonFilename = "libenv.json";
    
    private static IHost? ServiceProvider { get; set; }
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ServiceProvider = Host.CreateDefaultBuilder(args).Build();

        var libEnvJsonFilePath = AppContext.BaseDirectory + LibEnvJsonFilename;
        if (File.Exists(libEnvJsonFilePath))
        {
            ConfigCodeGenLib.Configuration.ReadConfigurationFromJson(libEnvJsonFilePath);
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace(LogEventLevel.Debug, LogArea.Control)
            .UseReactiveUI();
    
    #region Application Setting

    // TODO to singleton later?
    
    public static string GetConfigHomePath()
    {
        if (ServiceProvider == null)
        {
            return string.Empty;
        }

        var config = ServiceProvider.Services.GetRequiredService<IConfiguration>();
        var path = config.GetValue<string>("ConfigHomePath");
        return path ?? string.Empty;
    }

    public static string GetJsonHomePath()
    {
        if (ServiceProvider == null)
        {
            return string.Empty;
        }

        var config = ServiceProvider.Services.GetRequiredService<IConfiguration>();
        var path = config.GetValue<string>("JsonHomePath");
        return path ?? string.Empty;
    }

    #endregion
}
