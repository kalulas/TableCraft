using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LitJson;
using TableCraft.Core.ConfigElements;

namespace TableCraft.Core
{
    /// <summary>
    /// Setup environmental settings, like directory for code-template, generated code ...
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// template path & code generated path related to ConfigUsageType
        /// </summary>
        private class ConfigUsageInformation
        {
            /// <summary>
            /// .txt file
            /// </summary>
            public string CodeTemplateName { get; set; }
            /// <summary>
            /// example: ".cs"
            /// </summary>
            public string TargetFileType { get; set; }
            /// <summary>
            /// Format string for output filename
            /// </summary>
            public string OutputFormat { get; set; }
        }

        /// <summary>
        /// A ConfigUsageGroup contains a group of ConfigUsageInformation
        /// </summary>
        private class ConfigUsageGroup
        {
            public string GroupName { get; private set; }
            public string[] Usages { get; }

            public ConfigUsageGroup(string name, string[] usages)
            {
                GroupName = name;
                Usages = new string[usages.Length];
                Array.Copy(usages, Usages, usages.Length);
            }
        }

        public static string CodeTemplatePath { get; private set; }

        public static bool UseUTF8WithBOM { get; private set; } = true;
        
        public static string[] DataValueType => m_DataValueType.ToArray();

        public static string[] DataCollectionType => m_DataCollectionType.ToArray();

        public static string[] ConfigUsageType => m_ConfigUsageType.ToArray();
        
        public static string[] ConfigUsageGroupNames
        {
            get
            {
                var keyArr = m_ConfigUsageGroups.Keys.ToArray();
                Array.Sort(keyArr); // alphabet order
                return keyArr;
            }
        }

        public static string[] AttributeTag => m_AttributeTag.ToArray();

        private static bool m_IsInited;
        
        private const string m_TemplatesPath = "Templates";
        public static bool IsInited => m_IsInited;
        
        /// <summary>
        /// define legal data value type
        /// </summary>
        private static readonly List<string> m_DataValueType = new();
        /// <summary>
        /// define legal data container type
        /// </summary>
        private static readonly List<string> m_DataCollectionType = new();
        /// <summary>
        /// define usages like 'client', 'server'
        /// </summary>
        private static readonly List<string> m_ConfigUsageType = new();
        
        private static readonly Dictionary<string, ConfigUsageGroup> m_ConfigUsageGroups = new();

        private static readonly List<string> m_AttributeTag = new();

        private static readonly Dictionary<string, ConfigUsageInformation> m_UsageToInformation = new();

        private static JsonData m_LibEnvJsonData;

        private static void ReadStringArrayFromJson(JsonData data, string key, List<string> destination)
        {
            if (data.ContainsKey(key) && data[key].IsArray)
            {
                destination.Clear();
                foreach (var valueType in data[key])
                {
                    destination.Add(valueType.ToString());
                }
            }
            else
            {
                Debugger.LogError("[ReadStringArrayFromJson] key \'{0}\' not found in json config", key);
            }

        }

        /// <summary>
        /// only add usage member with this method
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="information"></param>
        private static void AddConfigUsageType(string usage, JsonData information)
        {
            if (m_ConfigUsageType.Contains(usage))
            {
                Debugger.LogWarning($"[AddConfigUsageType] usage \'{usage}\' is already defined.");
                return;
            }

            m_ConfigUsageType.Add(usage);
            m_UsageToInformation.Remove(usage);
            m_UsageToInformation.Add(usage, JsonMapper.ToObject<ConfigUsageInformation>(information.ToJson()));
        }

        private static void AddConfigUsageGroup(string groupName, JsonData groupJsonData)
        {
            if (m_ConfigUsageGroups.ContainsKey(groupName))
            {
                Debugger.LogWarning("[Configuration.AddConfigUsageGroup] group \'{0}\' is already defined.", groupName);
                return;
            }
            
            // groupJsonData is array of usage string
            var usages = new string[groupJsonData.Count];
            for (var i = 0; i < groupJsonData.Count; i++)
            {
                var definedUsage = (string)groupJsonData[i];
                if (!m_ConfigUsageType.Contains(definedUsage))
                {
                    throw new Exception(
                        $"usage '{definedUsage}' of group '{groupName}' is not defined in `ConfigUsageType`");
                }
                
                usages[i] = (string)groupJsonData[i];
            }

            var newUsageGroup = new ConfigUsageGroup(groupName, usages);
            m_ConfigUsageGroups[groupName] = newUsageGroup; // override
        }

        /// <summary>
        /// execute only once
        /// </summary>
        /// <param name="libEnvJsonFile">absolute path</param>
        public static void ReadConfigurationFromJson(string libEnvJsonFile)
        {
            if (m_IsInited)
            {
                Debugger.Log("[ReadConfigurationFromJson] already inited! exit");
                return;
            }

            var libEnvDir = Path.GetDirectoryName(libEnvJsonFile) ?? string.Empty;

            // notice: configuration unknown, cannot use FileHelper.ReadAllText now
            var jsonContent = File.ReadAllText(libEnvJsonFile, Encoding.UTF8);
            var configData = JsonMapper.ToObject(jsonContent);

            ReadStringArrayFromJson(configData, "DataValueType", m_DataValueType);
            ReadStringArrayFromJson(configData, "DataCollectionType", m_DataCollectionType);
            ReadStringArrayFromJson(configData, "AttributeTag", m_AttributeTag);
            
            CodeTemplatePath = Path.Combine(libEnvDir, m_TemplatesPath);
            UseUTF8WithBOM = (bool)configData["UTF8BOM"];

            m_ConfigUsageType.Clear();
            foreach (var usageConfig in configData["ConfigUsageType"])
            {
                var usage = (KeyValuePair<string,JsonData>)usageConfig;
                AddConfigUsageType(usage.Key, usage.Value);
            }
            
            m_ConfigUsageGroups.Clear();
            const string configUsageGroupKey = "ConfigUsageGroup";
            // config usage group is optional
            if (configData.ContainsKey(configUsageGroupKey) && configData[configUsageGroupKey].IsObject)
            {
                foreach (var groupConfig in configData[configUsageGroupKey])
                {
                    var groupDefine = (KeyValuePair<string,JsonData>)groupConfig;
                    AddConfigUsageGroup(groupDefine.Key, groupDefine.Value);
                }
            }

            m_IsInited = true;
            m_LibEnvJsonData = configData;
        }
        
        /// <summary>
        /// Create a configuration instance with specific <see cref="TableCraft.Core.Source.IDataSource"/>, a simple approach
        /// </summary>
        /// <param name="dataSourceType"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T GetDataSourceConfiguration<T>(Type dataSourceType)
        {
            if (!dataSourceType.IsAssignableTo(typeof(Source.IDataSource)))
            {
                Debugger.LogWarning($"[Configuration.GetDataSourceConfiguration] type '{dataSourceType}' is not a data source type");
            }
            
            if (!m_LibEnvJsonData.ContainsKey(dataSourceType.Name))
            {
                Debugger.LogWarning($"[Configuration.GetDataSourceConfiguration] configuration for data source type '{dataSourceType}' not found");
                return Activator.CreateInstance<T>(); // return a default one
            }
            
            return JsonMapper.ToObject<T>(m_LibEnvJsonData[dataSourceType.Name].ToJson());
        }

        #region Validation

        public static bool IsDefinedUsageGroup(string groupName)
        {
            return m_ConfigUsageGroups.ContainsKey(groupName);
        }

        public static bool IsDefinedUsage(string usageName)
        {
            return IsUsageValid(usageName);
        }

        internal static bool IsValueTypeValid(string valueType)
        {
            // empty string is legal
            return valueType != null && (valueType == string.Empty || m_DataValueType.Contains(valueType));
        }

        internal static bool IsCollectionTypeValid(string collectionType)
        {
            // empty string is legal
            return collectionType != null && (collectionType == string.Empty || m_DataCollectionType.Contains(collectionType));
        }

        internal static bool IsUsageValid(string usage)
        {
            return !string.IsNullOrEmpty(usage) && m_ConfigUsageType.Contains(usage);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get all extensions for all available Data Sources(no '.' prefix)
        /// </summary>
        /// <returns></returns>
        public static string[] GetDataSourceExtensions()
        {
            return ConfigInfoFactory.GetDataSourceExtensions();
        }

        public static string GetTemplateFilePathForUsage(string usage)
        {
            if (!m_UsageToInformation.ContainsKey(usage))
            {
                Debugger.LogError("[Configuration.GetTemplateFilePathForUsage] usage '{0}' not found", usage);
                return string.Empty;
            }

            var templateName = m_UsageToInformation[usage].CodeTemplateName;
            return Path.Combine(CodeTemplatePath, templateName);
        }
        
        public static string GetTargetFileTypeForUsage(string usage)
        {
            if (!m_UsageToInformation.ContainsKey(usage))
            {
                Debugger.LogError("[Configuration.GetTargetFileTypeForUsage] usage '{0}' not found", usage);
                return string.Empty;
            }

            return m_UsageToInformation[usage].TargetFileType;
        }

        public static string GetTargetFilenameForUsage(string usage, ConfigInfo configInfo)
        {
            var exportName = configInfo.GetExportName(usage);
            var outputExtension = GetTargetFileTypeForUsage(usage);
            var exportFormat = GetOutputFormatForUsage(usage);
            try
            {
                if (!string.IsNullOrEmpty(exportFormat))
                {
                    exportName = string.Format(exportFormat, exportName);
                }
            }
            catch (Exception e)
            {
                Debugger.LogError($"[ConfigManager.GetTargetFilenameForUsage] format {exportName} with {exportFormat} failed with exception {e}");
                exportName = configInfo.GetExportName(usage);// fallback to original
            }

            // if (Path.HasExtension(exportName))
            // {
            //     Debugger.LogWarning($"[ConfigManager.GetTargetFilenameForUsage] original extension in '{exportName}' will be replaced by '{outputExtension}'");
            // }
            
            var outputFileName = Path.ChangeExtension(exportName, outputExtension);
            return outputFileName;
        }

        public static string[] GetUsagesForGroup(string groupName)
        {
            return !m_ConfigUsageGroups.ContainsKey(groupName) ? Array.Empty<string>() : m_ConfigUsageGroups[groupName].Usages;
        }
        
        private static string GetOutputFormatForUsage(string usage)
        {
            if (!m_UsageToInformation.ContainsKey(usage))
            {
                Debugger.LogError("[Configuration.GetOutputFormatForUsage] usage '{0}' not found", usage);
                return string.Empty;
            }

            return m_UsageToInformation[usage].OutputFormat;
        }

        #endregion

    }
}
