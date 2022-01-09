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

            Debugger.InitialCustomLogger(Console.WriteLine);
            // pass json file with absolute path
            Configuration.ReadConfigurationFromJson(args[0]);
            ConfigManager.singleton.ReadComment = true;
            // pass config file with absolute path
            ConfigManager.singleton.Process(args[1], ConfigCodeGenLib.ConfigReader.EConfigType.CSV);
        }
    }
}
