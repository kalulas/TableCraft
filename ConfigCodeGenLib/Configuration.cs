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

        private static readonly Dictionary<string, ConfigUsageInformation> m_UsageToInformation = new Dictionary<string, ConfigUsageInformation>();

        public static string CodeTemplatePath { get; private set; }
        public static string DefaultCollectionType { get; private set; }
        // TODO fix frequent gc alloc, list to array
        public static string[] DataValueType => m_DataValueType.ToArray();

        private static bool m_IsInited;
        public static bool IsInited => m_IsInited;

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

            DefaultCollectionType = configData["DefaultCollectionType"].ToString();
            CodeTemplatePath = Path.Combine(libEnvDir, configData["CodeTemplatePath"].ToString());

            m_ConfigUsageType.Clear();
            foreach (var usageConfig in configData["ConfigUsageType"])
            {
                var usage = (KeyValuePair<string,JsonData>)usageConfig;
                AddConfigUsageType(usage.Key, usage.Value);
            }

            #region Test Code
            //var clientTemplateFullPath = CodeTemplatePath + m_UsageToInformation["client"].CodeTemplateName;
            //Debugger.LogForamt("client code template full path {0}", clientTemplateFullPath);
            //var content = File.ReadAllText(clientTemplateFullPath);
            //Debugger.LogForamt(content);
            #endregion

            m_IsInited = true;
        }

        #region Validation

        internal static bool IsValueTypeValid(string valueType)
        {
            return !string.IsNullOrEmpty(valueType) && m_DataValueType.Contains(valueType);
        }

        internal static bool IsCollectionTypeValid(string collectionType)
        {
            return !string.IsNullOrEmpty(collectionType) && m_DataCollectionType.Contains(collectionType);
        }

        internal static bool IsUsageValid(string usage)
        {
            return !string.IsNullOrEmpty(usage) && m_ConfigUsageType.Contains(usage);
        }

        #endregion

    }
}
