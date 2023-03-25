using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

namespace ConfigCodeGenLib
{
    /// <summary>
    /// Setup environmetal settings, like directory for code-template, generated code ...
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
        }
        
        public static string CodeTemplatePath { get; private set; }

        public static bool UseUTF8WithBOM { get; private set; } = true;
        
        public static string[] DataValueType => m_DataValueType.ToArray();

        public static string[] DataCollectionType => m_DataCollectionType.ToArray();

        public static string[] ConfigUsageType => m_ConfigUsageType.ToArray();

        public static string[] AttributeTag => m_AttributeTag.ToArray();

        private static bool m_IsInited;
        
        private const string m_TemplatesPath = "Templates";
        public static bool IsInited => m_IsInited;
        
        /// <summary>
        /// define legal data value type
        /// </summary>
        private static readonly List<string> m_DataValueType = new List<string>();
        /// <summary>
        /// define legal data container type
        /// </summary>
        private static readonly List<string> m_DataCollectionType = new List<string>();
        /// <summary>
        /// define usages like 'client', 'server'
        /// </summary>
        private static readonly List<string> m_ConfigUsageType = new List<string>();

        private static readonly List<string> m_AttributeTag = new List<string>();

        private static readonly Dictionary<string, ConfigUsageInformation> m_UsageToInformation =
            new Dictionary<string, ConfigUsageInformation>();

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

            m_IsInited = true;
        }

        #region Validation

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

    }
}
