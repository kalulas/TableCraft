using System;
using ConfigCodeGenLib;

namespace ConfigCodeGenConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("[ConfigCodeGenConsole.Main] no json path specified!");
                return;
            }

            var configJsonFilePath = args[0];
            var targetCsvConfigFilePath = args[1];
            Debugger.InitialCustomLogger(Console.WriteLine);
            // pass json file with absolute path
            Configuration.ReadConfigurationFromJson(configJsonFilePath);
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
