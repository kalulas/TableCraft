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
                Debugger.LogError("[ConfigManager] Configuration not setup yet! run Configuration.ReadConfigurationFromJson() first");
            }
        }

        #region Singleton

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

        #endregion

        #region Fields

        /// <summary>
        /// try getting comments from second line if true
        /// </summary>
        public bool ReadComment;

        private readonly Dictionary<string, ConfigInfo> m_ConfigInfoDict = new Dictionary<string, ConfigInfo>();

        #endregion

        #region Private Methods

        private string GetConfigIdentifierInternal(string configFilePath)
        {
            return Path.GetFileNameWithoutExtension(configFilePath);
        }

        #endregion

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
            if (m_ConfigInfoDict.Remove(identifier))
            {
                Debugger.LogWarning("[ConfigManager.AddNewConfigInfo] previous {0} ConfigInfo instance is removed", identifier);
            }
            
            var configInfo = ConfigInfoFactory.CreateConfigInfo(configType, configFilePath);
            m_ConfigInfoDict[identifier] = configInfo;
            return configInfo;
        }

        /// <summary>
        /// A simple getter
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="configInfo"></param>
        /// <returns></returns>
        public bool GetConfigInfo(string identifier, out ConfigInfo configInfo)
        {
            return m_ConfigInfoDict.TryGetValue(identifier, out configInfo);
        }

        /// <summary>
        /// Try get ConfigInfo and save to json file
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns> true if found and saved </returns>
        public bool SaveConfigInfoJsonFile(string identifier)
        {
            if (!m_ConfigInfoDict.TryGetValue(identifier, out var configInfo))
            {
                return false;
            }
            
            configInfo.SaveJsonFile();
            return true;
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
