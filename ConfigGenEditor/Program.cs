using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using Avalonia.Logging;
using ConfigCodeGenLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ConfigGenEditor;

class Program
{
    public const string ListJsonFilename = "list.json";
    public const string LibEnvJsonFilename = "libenv.json";
    
    private static IConfiguration? m_Configuration;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        m_Configuration = Host.CreateDefaultBuilder(args).Build().Services.GetRequiredService<IConfiguration>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(m_Configuration)
            .CreateLogger();

        Log.Information("Logger is initialized");

        try
        {
            var libEnvJsonFilePath = AppContext.BaseDirectory + LibEnvJsonFilename;
            if (File.Exists(libEnvJsonFilePath))
            {
                Debugger.InitialCustomLogger(Log.Information);
                Configuration.ReadConfigurationFromJson(libEnvJsonFilePath);
                // TODO ReadComment to libenv.json
                ConfigManager.singleton.ReadComment = true;
                Log.Information("ConfigCodeGenLib is initialized with '{LibEnvJson}'", libEnvJsonFilePath);
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Error(e, "Unhandled exception");
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
    
    #region Application Setting

    // TODO to singleton later?
    
    public static string GetConfigHomePath()
    {
        if (m_Configuration == null)
        {
            throw new Exception("m_Configuration from appsettings is null");
        }
        
        var path = m_Configuration.GetValue<string>("ConfigHomePath");
        return path ?? string.Empty;
    }

    public static string GetJsonHomePath()
    {
        if (m_Configuration == null)
        {
            throw new Exception("m_Configuration from appsettings is null"); 
        }
        
        var path = m_Configuration.GetValue<string>("JsonHomePath");
        return path ?? string.Empty;
    }

    #endregion
}
