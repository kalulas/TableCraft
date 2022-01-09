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
            Configuration.ReadConfigurationFromJson(args[0]);
            ConfigManager.singleton.ReadComment = true;
            ConfigManager.singleton.Process(args[1], ConfigCodeGenLib.ConfigReader.EConfigType.CSV);
        }
    }
}
