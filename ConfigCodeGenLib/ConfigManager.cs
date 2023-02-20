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

        private string GetConfigIdentifierInternal(string configFilePath)
        {
            return Path.GetFileNameWithoutExtension(configFilePath);
        }

        #region Public API

        /// <summary>
        /// Add new <see cref="ConfigInfo"/> with no json file created
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <param name="configType"></param>
        /// <returns></returns>
        public ConfigInfo AddNewConfigInfo(string configFilePath, EConfigType configType)
        {
            var identifier = GetConfigIdentifierInternal(configFilePath);
            if (ConfigInfoDict.Remove(identifier))
            {
                Debugger.LogWarningFormat("[ConfigManager.AddNewConfigInfo] previous {0} ConfigInfo instance is removed", identifier);
            }
            
            var configInfo = ConfigInfo.CreateConfigInfo(configType, configFilePath);
            ConfigInfoDict[identifier] = configInfo;
            return configInfo;
        }

        public bool GetConfigInfo(string identifier, out ConfigInfo configInfo)
        {
            return ConfigInfoDict.TryGetValue(identifier, out configInfo);
        }

        public string GetConfigIdentifier(string configFilePath)
        {
            return GetConfigIdentifierInternal(configFilePath);
        }

        public bool IsRelatedJsonFileExists(string configIdentifier)
        {
            if (!Configuration.IsInited)
            {
                return false;
            }

            var relatedJsonFilePath = Configuration.ConfigJsonPath + configIdentifier + ".json";
            return File.Exists(relatedJsonFilePath);
        }

        #endregion
    }
}
