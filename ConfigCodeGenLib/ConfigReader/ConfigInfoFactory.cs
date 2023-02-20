using System.IO;

namespace ConfigCodeGenLib.ConfigReader
{
    public static class ConfigInfoFactory
    {
        public static ConfigInfo CreateConfigInfo(EConfigType configType, string configFilePath)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            var relatedJsonPath = Configuration.ConfigJsonPath + configName + ".json";
            ConfigInfo configInfo = null;
            switch (configType)
            {
                case EConfigType.CSV:
                    configInfo = new CSVConfigInfo(configType, configName, configFilePath, relatedJsonPath);
                    // config file is a must-have
                    if (!configInfo.ReadConfigFileAttributes())
                    {
                        Debugger.LogError($"[ConfigInfoFactory.CreateConfigInfo] create {configName} failed: '{configFilePath}' not found!");
                        return null;
                    }

                    // json file is optional
                    configInfo.ReadJsonFileAttributes();
                    break;
                default:
                    Debugger.Log("[ConfigInfoFactory.CreateConfigInfo] not supported config type!");
                    break;
            }

            return configInfo;
        }
    }
}