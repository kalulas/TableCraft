using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConfigCodeGenLib.ConfigReader;

namespace ConfigCodeGenLib
{
    /// <summary>
    /// ConfigManager(singleton)
    /// </summary>
    public class ConfigManager
    {
        private ConfigManager()
        {
            if (!Configuration.IsInited)
            {
                Debugger.LogErrorFormat("[ConfigManager] Configuration not setup yet! run Configuration.ReadConfigurationFromJson() first");
            }
        }

        private static ConfigManager m_Singleton;

        public static ConfigManager singleton
        {
            get
            {
                if (m_Singleton == null)
                {
                    m_Singleton = new ConfigManager();
                }

                return m_Singleton;
            }
        }

        /// <summary>
        /// try getting comments from second line if true
        /// </summary>
        public bool ReadComment;
        public Dictionary<string, ConfigInfo> ConfigInfoDict = new Dictionary<string, ConfigInfo>();

        /// <summary>
        /// <param> Add a config file to manager, automatically search for a json file with the same name</param>
        /// <param> generated information of all attributes in this config file </param>
        /// </summary>
        /// <param name="configFilePath"></param>
        public ConfigInfo Process(string configFilePath, EConfigType configType, bool refresh=false)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            if (ConfigInfoDict.ContainsKey(configName) && !refresh)
            {
                Debugger.LogWarningFormat("[ConfigManager.Process] {0} is already processed! set refresh to \'true\'", configName);
                return ConfigInfoDict[configName];
            }

            var configInfo = ConfigInfo.CreateConfigInfo(configType, configFilePath, true);

            ConfigInfoDict.Remove(configName);
            ConfigInfoDict.Add(configName, configInfo);
            return configInfo;
        }

        public bool GenerateCode(string configName, string usage)
        {
            if (!ConfigInfoDict.ContainsKey(configName))
            {
                Debugger.LogErrorFormat("unknown configName '{0}', try Process() first", configName);
                return false;
            }

            // TODO usage validation

            var configInfo = ConfigInfoDict[configName];
            CodeGenerate.Generator.Process(configInfo, usage);
            return true;
        }
    }
}
