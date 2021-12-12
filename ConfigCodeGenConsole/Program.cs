using System;

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

            ConfigCodeGenLib.Configuration.ReadConfigurationFromJson(args[0]);
            ConfigCodeGenLib.ConfigManager.singleton.ReadComment = true;
            ConfigCodeGenLib.ConfigManager.singleton.Process(args[1], ConfigCodeGenLib.ConfigReader.EConfigType.CSV);
        }
    }
}
