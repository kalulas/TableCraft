using System.IO;

namespace ConfigCodeGenLib.ConfigReader
{
    public static class ConfigInfoFactory
    {
        public static ConfigInfo CreateConfigInfo(EConfigType configType, string configFilePath)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            var relatedJsonPath = Configuration.ConfigJsonPath + configName + ".json";
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
    }
}