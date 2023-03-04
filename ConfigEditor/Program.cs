using Avalonia;
using System;
using Avalonia.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigEditor;

class Program
{
    public static IHost? ServiceProvider { get; private set; }
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ServiceProvider = Host.CreateDefaultBuilder(args).Build();
        var config = ServiceProvider.Services.GetRequiredService<IConfiguration>();
        var targetCsvConfigFilePath = config.GetValue<string>("CsvFilePath");
        System.Diagnostics.Trace.TraceInformation(targetCsvConfigFilePath);

        var libEnvJsonFilePath = AppContext.BaseDirectory + "libenv.json";
        ConfigCodeGenLib.Configuration.ReadConfigurationFromJson(libEnvJsonFilePath);
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace(LogEventLevel.Debug);
}
