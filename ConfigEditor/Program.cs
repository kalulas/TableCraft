using Avalonia;
using System;
using System.IO;
using Avalonia.Logging;
using ConfigEditor.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigEditor;

class Program
{
    private static IHost? ServiceProvider { get; set; }
    private const string m_LibEnvJsonFilename = "libenv.json";
    private const string m_ListJsonFilename = "list.json";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ServiceProvider = Host.CreateDefaultBuilder(args).Build();

        var libEnvJsonFilePath = AppContext.BaseDirectory + m_LibEnvJsonFilename;
        if (File.Exists(libEnvJsonFilePath))
        {
            ConfigCodeGenLib.Configuration.ReadConfigurationFromJson(libEnvJsonFilePath);
        }

        // load list.json to ConfigFileContext
        var listJsonFilePath = AppContext.BaseDirectory + m_ListJsonFilename;
        if (File.Exists(listJsonFilePath))
        {
            var succeed = ConfigFileContext.Load(listJsonFilePath);
            // TODO error log if failed
        }
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace(LogEventLevel.Debug);

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
