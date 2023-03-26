using System;
using ConfigCodeGenLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigCodeGenConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/core/extensions/configuration
            var host = Host.CreateDefaultBuilder(args).Build();
            var config = host.Services.GetRequiredService<IConfiguration>();
            var targetCsvConfigFilePath = config.GetValue<string>("CsvFilePath");
            var jsonFilePath = config.GetValue<string>("JsonFilePath");

            if (string.IsNullOrEmpty(targetCsvConfigFilePath))
            {
                Console.WriteLine("[ConfigCodeGenConsole.Main] no csv path specified!");
                return;
            }

            Debugger.InitialCustomLogger(Console.WriteLine);
            // pass json file with absolute path
            Configuration.ReadConfigurationFromJson(AppContext.BaseDirectory + "libenv.json");
            if (!Configuration.IsInited)
            {
                return;
            }

            ConfigManager.singleton.ReadComment = true;
            // var identifier = ConfigManager.singleton.GetConfigIdentifier(targetCsvConfigFilePath);
            // var relatedJsonFilePath = $"{jsonHomeDir}\\{identifier}.json";
            var configInfo = ConfigManager.singleton.AddNewConfigInfo(targetCsvConfigFilePath, jsonFilePath, ConfigCodeGenLib.ConfigReader.EConfigType.CSV);
            ConfigManager.singleton.GenerateCodeForUsage(Configuration.ConfigUsageType[0], configInfo,
                AppContext.BaseDirectory);
        }
    }
}
