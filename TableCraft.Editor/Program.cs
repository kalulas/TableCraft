using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TableCraft.Core.IO;
using TableCraft.Core.VersionControl;
using TableCraft.Editor.Services;

namespace TableCraft.Editor;

class Program
{
    public const string ListJsonFilename = "list.json";
    public const string LibEnvJsonFilename = "libenv.json";
    public const string AppSettingsFilename = "appsettings.json";

    private static readonly string m_FallbackCodeExportPath = Path.Combine(AppContext.BaseDirectory, "GeneratedCode");
    private static IHost? m_Host;
    private static IConfiguration? m_Configuration;

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

        Log.Information("Logger is initialized");

        try
        {
            // Move host and configuration initialization into try-catch after logger is ready
            m_Host = Host.CreateDefaultBuilder(args).Build();
            m_Configuration = m_Host.Services.GetRequiredService<IConfiguration>();
            Log.Information("Configuration loaded successfully");
            
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
    
    public static PerforceUserConfig GetVersionControlConfig()
    {
        if (m_Configuration == null) throw new NullReferenceException("m_Configuration from appsettings is null");

        var emptyConfig = new PerforceUserConfig();
        var section = m_Configuration.GetSection("P4Config");
        if (!section.Exists())
        {
            return emptyConfig;
        }

        var children = section.GetChildren();
        if (children.Any(child => string.IsNullOrEmpty(child.Value)))
        {
            return emptyConfig;
        }

        var config = section.Get<PerforceUserConfig>();
        if (config == null || config.P4PORT == null)
        {
            Log.Error("[Program.GetVersionControlConfig] get PerforceUserConfig from appsettings failed!");
            return emptyConfig;
        }
        
        config.Decode();
        return config;
    }
    
    /// <summary>
    /// <para> This is (still) a bad approach to update perforce configuration in appsettings.json:
    /// we deserialize the whole appsettings json file, update / add related content, then serialize and write it back </para>
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static async Task<bool> UpdateVersionControlConfig(PerforceUserConfig config)
    {
        config.Encode(); // make sure we have encoded password
        var appSettingsFilePath = Path.Combine(AppContext.BaseDirectory, AppSettingsFilename);
        if (!File.Exists(appSettingsFilePath))
        {
            Log.Error("[UpdateVersionControlConfig] AppSettings file not found: {FilePath}", appSettingsFilePath);
            return false;
        }

        var appSettingsContent = await FileHelper.ReadToEnd(appSettingsFilePath);
        if (string.IsNullOrEmpty(appSettingsContent))
        {
            Log.Error("[UpdateVersionControlConfig] AppSettings file is empty or could not be read: {FilePath}", appSettingsFilePath);
            return false;
        }

        var appSettingsJsonData = JsonNode.Parse(appSettingsContent);
        if (appSettingsJsonData == null)
        {
            Log.Error("[UpdateVersionControlConfig] Failed to parse appsettings.json as valid JSON");
            return false;
        }
        
        var p4ConfigJsonStr = JsonSerializer.Serialize(config);
        var p4ConfigJsonNode = JsonNode.Parse(p4ConfigJsonStr);
        if (p4ConfigJsonNode == null)
        {
            Log.Error("[UpdateVersionControlConfig] Failed to serialize PerforceUserConfig to JSON");
            return false;
        }
        
        // remove sensitive information
        if (p4ConfigJsonNode is JsonObject p4ConfigObject)
        {
            p4ConfigObject.Remove(nameof(config.P4Passwd));
        }
        
        if (appSettingsJsonData is JsonObject appSettingsObject)
        {
            appSettingsObject["P4Config"] = p4ConfigJsonNode;
        }
        else
        {
            Log.Error("[UpdateVersionControlConfig] AppSettings root is not a JSON object, cannot update P4Config");
            return false;
        }

        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var updatedJson = JsonSerializer.Serialize(appSettingsJsonData, jsonOptions);
            await FileHelper.WriteAsync(appSettingsFilePath, updatedJson);
        }
        catch (UnauthorizedAccessException exception)
        {
            Log.Error(exception, "[UpdateVersionControlConfig] UnauthorizedAccessException when writing to appsettings.json: {FilePath}", appSettingsFilePath);
            await MessageBoxManager.ShowStandardMessageBoxDialog("UnauthorizedAccessException",
                $"Failed to write '{appSettingsFilePath}', checkout the file yourself if you're using Perforce \n\nstack trace:\n{exception}");
            return false;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "[UpdateVersionControlConfig] Unexpected exception when writing to appsettings.json: {FilePath}", appSettingsFilePath);
            return false;
        }
        
        return true;
    }

    #endregion

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