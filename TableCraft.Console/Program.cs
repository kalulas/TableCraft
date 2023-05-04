using System;
using System.IO;
using System.Threading.Tasks;
using TableCraft.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TableCraft.Core.ConfigElements;

namespace TableCraft.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/core/extensions/configuration
            var host = Host.CreateDefaultBuilder(args).Build();
            var config = host.Services.GetRequiredService<IConfiguration>();
            var targetCsvConfigFilePath = config.GetValue<string>("CsvFilePath");
            var jsonFilePath = config.GetValue<string>("JsonFilePath");

            if (string.IsNullOrEmpty(targetCsvConfigFilePath))
            {
                System.Console.WriteLine("[TableCraft.Console.Main] no csv path specified!");
                return;
            }

            Debugger.InitialCustomLogger(System.Console.WriteLine, Debugger.LogLevel.All);
            // pass json file with absolute path
            Configuration.ReadConfigurationFromJson(AppContext.BaseDirectory + "libenv.json");
            if (!Configuration.IsInited)
            {
                return;
            }
            
            var experimentalInfo = ConfigManager.singleton.CreateConfigInfo(targetCsvConfigFilePath, new[] {jsonFilePath});
            var success = ConfigManager.singleton.SaveConfigInfoWithDecorator(experimentalInfo,
                Path.Combine(AppContext.BaseDirectory, "experimental.json"));
            await ConfigManager.singleton.GenerateCodeForUsage(Configuration.ConfigUsageType[0], experimentalInfo,
                AppContext.BaseDirectory);
        }
    }
}
