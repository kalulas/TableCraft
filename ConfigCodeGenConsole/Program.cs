using System;
using ConfigCodeGenLib;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

namespace ConfigCodeGenConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/core/extensions/configuration
            //var host = Host.CreateDefaultBuilder(args).Build();
            //var config = host.Services.GetRequiredService<IConfiguration>();
            //var dataValueType = config.GetValue<string>("DataValueType:0");
            //Console.WriteLine($"dataValueType is {dataValueType}");

            if (args.Length < 1)
            {
                Console.WriteLine("[ConfigCodeGenConsole.Main] no csv path specified!");
                return;
            }

            var targetCsvConfigFilePath = args[0];
            Debugger.InitialCustomLogger(Console.WriteLine);
            // pass json file with absolute path
            Configuration.ReadConfigurationFromJson(AppContext.BaseDirectory + "appsettings.json");
            if (!Configuration.IsInited)
            {
                return;
            }

            ConfigManager.singleton.ReadComment = true;
            var identifier = ConfigManager.singleton.GetConfigIdentifier(targetCsvConfigFilePath);
            var configInfo = ConfigManager.singleton.AddNewConfigInfo(targetCsvConfigFilePath, ConfigCodeGenLib.ConfigReader.EConfigType.CSV);
            if (!configInfo.HasJsonConfig)
            {
                Debugger.Log($"{identifier} related json file not found, create a new one");
                configInfo.SaveJsonFile();
            }
        }
    }
}
