using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Linq;
using Avalonia.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TableCraft.Editor;

class Program
{
    public const string ListJsonFilename = "list.json";
    public const string LibEnvJsonFilename = "libenv.json";

    private static readonly string m_FallbackCodeExportPath = Path.Combine(AppContext.BaseDirectory, "GeneratedCode");
    private static IHost? m_Host;
    private static IConfiguration? m_Configuration;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        m_Host = Host.CreateDefaultBuilder(args).Build();
        m_Configuration = m_Host.Services.GetRequiredService<IConfiguration>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(m_Configuration)
            .CreateLogger();

        Log.Information("Logger is initialized");

        try
        {
            var libEnvJsonFilePath = AppContext.BaseDirectory + LibEnvJsonFilename;
            if (!File.Exists(libEnvJsonFilePath))
                throw new FileNotFoundException("library configuration file not found", LibEnvJsonFilename);
            
            // we initialize library here and build Avalonia app anyway,
            // later we check if it's initialized in App.OnFrameworkInitializationCompleted
            Core.Debugger.InitialCustomLogger(Log.Information, Core.Debugger.LogLevel.Information);
            Core.Debugger.InitialCustomLogger(Log.Warning, Core.Debugger.LogLevel.Warning);
            Core.Debugger.InitialCustomLogger(Log.Error, Core.Debugger.LogLevel.Error);
            Log.Information("try initializing library with '{LibEnvJson}'", libEnvJsonFilePath);
            Core.Configuration.ReadConfigurationFromJson(libEnvJsonFilePath);
            
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

    /// <summary>
    /// Try get the export path for code generation from appsettings.json, if not found, use <see cref="m_FallbackCodeExportPath"/>
    /// </summary>
    /// <param name="usage"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static string GetCodeExportPath(string usage)
    {
        if (m_Configuration == null) throw new NullReferenceException("m_Configuration from appsettings is null");

        var section = m_Configuration.GetSection("ExportPath");
        // section is not null, if 'ExportPath' not found, empty section will return a null string
        var exportPath = section.GetValue<string>(usage);
        return exportPath ?? m_FallbackCodeExportPath;
    }
    
    public static Core.VersionControl.Perforce? GetVersionControlWithConfig()
    {
        if (m_Configuration == null) throw new NullReferenceException("m_Configuration from appsettings is null");
        if (!m_Configuration.GetSection("P4Config").Exists())
        {
            return null;
        }

        var children = m_Configuration.GetSection("P4Config").GetChildren();
        if (children.Any(section => string.IsNullOrEmpty(section.Value)))
        {
            return null;
        }
        
        var uri = m_Configuration.GetValue<string>("P4Config:P4PORT");
        var user = m_Configuration.GetValue<string>("P4Config:P4USER");
        var client = m_Configuration.GetValue<string>("P4Config:P4CLIENT");
        var password = m_Configuration.GetValue<string>("P4Config:P4PASSWD(base64)");
        return new Core.VersionControl.Perforce(uri, user, client, password);
    }

    #endregion

    #region Logger

    public static void HandleException(Exception e)
    {
        Log.Error(e, "Unhandled exception");
    }

    #endregion
}