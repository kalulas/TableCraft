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
            if (!ConfigManager.singleton.IsRelatedJsonFileExists(identifier))
            {
                Debugger.LogFormat($"{identifier} related json file not found, create a new one");
                ConfigManager.singleton.CreateRelatedJson(targetCsvConfigFilePath, ConfigCodeGenLib.ConfigReader.EConfigType.CSV);
                return;
            }

            // pass config file with absolute path
            Debugger.LogFormat($"{identifier} related json file found");
            ConfigManager.singleton.ProcessWithExisted(targetCsvConfigFilePath, ConfigCodeGenLib.ConfigReader.EConfigType.CSV);
            //ConfigManager.singleton.GenerateCode(configInfo.ConfigName, "client");
        }
    }
}
