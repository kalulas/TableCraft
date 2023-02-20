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
        /// <para> Add a config file to manager, automatically search for a json file with the same name</para>
        /// <para> An existed json file is required, call <see cref="IsRelatedJsonFileExists(string)"/> first </para>
        /// <para> generated information of all attributes in this config file </para>
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <param name="configType"></param>
        /// <param name="refresh"></param>
        public ConfigInfo ProcessWithExisted(string configFilePath, EConfigType configType, bool refresh=false)
        {
            var identifier = GetConfigIdentifierInternal(configFilePath);
            if (ConfigInfoDict.ContainsKey(identifier) && !refresh)
            {
                Debugger.LogWarningFormat("[ConfigManager.ProcessWithExisted] {0} is already processed! set refresh to \'true\'", identifier);
                return ConfigInfoDict[identifier];
            }

            var configInfo = ConfigInfo.CreateConfigInfo(configType, configFilePath);

            ConfigInfoDict.Remove(identifier);
            ConfigInfoDict.Add(identifier, configInfo);
            return configInfo;
        }

        /// <summary>
        /// <para> Create a configInfo instance and related json file. </para>
        /// <para> Call this when related json file is not existed yet. </para>
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <param name="configType"></param>
        /// <returns></returns>
        public ConfigInfo CreateRelatedJson(string configFilePath, EConfigType configType)
        {
            var identifier = GetConfigIdentifierInternal(configFilePath);
            if (ConfigInfoDict.Remove(identifier))
            {
                Debugger.LogWarningFormat("[ConfigManager.CreateRelatedJson] previous {0} is removed");
            }

            var configInfo = ConfigInfo.CreateConfigInfo(configType, configFilePath, true);
            ConfigInfoDict[identifier] = configInfo;
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
            //CodeGenerate.Generator.Process(configInfo, usage);
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
