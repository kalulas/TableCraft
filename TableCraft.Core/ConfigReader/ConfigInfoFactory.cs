using System.IO;

namespace TableCraft.Core.ConfigReader
{
    public static class ConfigInfoFactory
    {
        public static ConfigInfo CreateConfigInfo(EConfigType configType, string configFilePath)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            var relatedJsonPath = string.Empty;
            switch (configType)
            {
                case EConfigType.CSV:
                    ConfigInfo configInfo = new CSVConfigInfo(configType, configName, configFilePath, relatedJsonPath);
                    return configInfo.ReadConfigFileAttributes()?.ReadJsonFileAttributes();
                default:
                    Debugger.Log("[ConfigInfoFactory.CreateConfigInfo] not supported config type!");
                    return null;
            }
        }

        public static ConfigInfo CreateConfigInfo(EConfigType configType, string configFilePath, string jsonFilePath)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            switch (configType)
            {
                case EConfigType.CSV:
                    ConfigInfo configInfo = new CSVConfigInfo(configType, configName, configFilePath, jsonFilePath);
                    return configInfo.ReadConfigFileAttributes()?.ReadJsonFileAttributes();
                default:
                    Debugger.Log("[ConfigInfoFactory.CreateConfigInfo] not supported config type!");
                    return null;
            }
        }
    }
}