using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// Generate code with specific usage to <paramref name="outputDirectory"/>
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="configInfo"></param>
        /// <param name="outputDirectory"></param>
        /// <returns></returns>
        public async Task<bool> GenerateCodeForUsage(string usage, ConfigInfo configInfo, string outputDirectory)
        {
            if (configInfo == null)
            {
                Debugger.LogWarning("[ConfigManager.GenerateCodeForUsage] configInfo is null");
                return false;
            }

            if (!Configuration.IsUsageValid(usage))
            {
                Debugger.LogWarning($"[ConfigManager.GenerateCodeForUsage] usage '{usage}' is not valid");
                return false;
            }
            
            var templateFilePath = Configuration.GetTemplateFilePathForUsage(usage);
            var outputExtension = Configuration.GetTargetFileTypeForUsage(usage);
            if (string.IsNullOrEmpty(templateFilePath) || string.IsNullOrEmpty(outputExtension) || !File.Exists(templateFilePath))
            {
                Debugger.LogWarning("[ConfigManager.GenerateCodeForUsage] template file {1} not found for usage '{0}'",
                    usage, templateFilePath ?? string.Empty);
                return false;
            }

            // TODO configInfo: different name under different usage
            var configName = Path.ChangeExtension(configInfo.ConfigName, outputExtension);
            var outputFilePath = Path.Combine(outputDirectory, configName);
            if (File.Exists(outputFilePath))
            {
                Debugger.LogWarning("[ConfigManager.GenerateCodeForUsage] existed file '{0}' will be overwrite", outputDirectory);
            }

            var encoding = new UTF8Encoding(Configuration.UseUTF8WithBOM);
            var host = new CustomHost(templateFilePath, outputExtension, encoding, configInfo);
            var engine = new Engine();
            // read from template file
            string templateContent;
            // templateContent = File.ReadAllText(templateFilePath, encoding);
            using (var fs = File.Open(templateFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs, encoding))
                {
                    templateContent = await sr.ReadToEndAsync();
                };
            };
            
            Debugger.Log($"[ConfigManager.GenerateCodeForUsage] start processing template file {templateFilePath}");
            // transform the text template
            var outputContent = engine.ProcessTemplate(templateContent, host);
            // var outputContent = await Task.Run(() => engine.ProcessTemplate(templateContent, host));
            Debugger.Log($"[ConfigManager.GenerateCodeForUsage] finish processing template file {templateFilePath}");
            using (var fs = File.Open(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs, encoding))
                {
                    await sw.WriteAsync(outputContent);
                }
            }
            
            // File.WriteAllText(outputFilePath, outputContent, encoding);
            Debugger.Log($"[ConfigManager.GenerateCodeForUsage] finish writing generated content to file {outputFilePath}");
            // generation errors to logger
            var success = host.PrintErrors();
            return success;
        }

        #endregion
    }
}
