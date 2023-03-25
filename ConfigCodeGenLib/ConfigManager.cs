using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConfigCodeGenLib.ConfigReader;
using ConfigCodeGenLib.Generation;
using Microsoft.VisualStudio.TextTemplating;

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
        /// TODO this should be moved to libenv.json, specific line number
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

        public string GetConfigIdentifier(string configFilePath)
        {
            return GetConfigIdentifierInternal(configFilePath);
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
        /// Add new <see cref="ConfigInfo"/> with specific json file
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <param name="jsonFilePath"></param>
        /// <param name="configType"></param>
        /// <returns></returns>
        public ConfigInfo AddNewConfigInfo(string configFilePath, string jsonFilePath, EConfigType configType)
        {
            var identifier = GetConfigIdentifierInternal(configFilePath);
            if (m_ConfigInfoDict.Remove(identifier))
            {
                Debugger.LogWarning("[ConfigManager.AddNewConfigInfo] previous {0} ConfigInfo instance is removed", identifier);
            }

            var configInfo = ConfigInfoFactory.CreateConfigInfo(configType, configFilePath, jsonFilePath);
            m_ConfigInfoDict[identifier] = configInfo;
            return configInfo;
        }

        /// <summary>
        /// Try get ConfigInfo and save to json file
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="jsonFilePath"></param>
        /// <returns> true if found and saved </returns>
        public bool SaveConfigInfoJsonFile(string identifier, string jsonFilePath)
        {
            if (!m_ConfigInfoDict.TryGetValue(identifier, out var configInfo))
            {
                return false;
            }
            
            configInfo.SaveJsonFile(jsonFilePath);
            return true;
        }

        /// <summary>
        /// Save related json file, a wrapper method of <see cref="ConfigInfo.SaveJsonFile"/>
        /// </summary>
        /// <param name="configInfo"></param>
        /// <param name="jsonFilePath"></param>
        /// <returns></returns>
        public bool SaveConfigInfoJsonFile(ConfigInfo configInfo, string jsonFilePath)
        {
            if (configInfo == null)
            {
                return false;
            }
            
            configInfo.SaveJsonFile(jsonFilePath);
            return true;
        }

        /// <summary>
        /// Generate code with specific usage to <paramref name="outputFilePath"/>
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="configInfo"></param>
        /// <param name="outputFilePath"></param>
        /// <returns></returns>
        public async Task<bool> GenerateCodeForUsage(string usage, ConfigInfo configInfo, string outputFilePath)
        {
            if (configInfo == null)
            {
                return false;
            }

            if (!Configuration.IsUsageValid(usage))
            {
                return false;
            }

            if (File.Exists(outputFilePath))
            {
                Debugger.LogWarning("existed file '{0}' will be overwrite", outputFilePath);
            }

            var templateFilePath = Configuration.GetTemplateFilePathForUsage(usage);
            if (string.IsNullOrEmpty(templateFilePath))
            {
                return false;
            }

            var host = new ReaderScriptHost();
            var engine = new Engine();
            // TODO implement custom host
            return false;
        }

        #endregion
    }
}
