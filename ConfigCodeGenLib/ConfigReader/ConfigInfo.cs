using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using System.Text;

namespace ConfigCodeGenLib.ConfigReader
{
    /// <summary>
    /// Information about a single config file, for example: SomeTable.csv
    /// </summary>
    public abstract class ConfigInfo
    {
        #region Fields

        private const string ATTRIBUTES_KEY = "Attributes";
        private const string CONFIG_NAME_KEY = "ConfigName";
        
        protected readonly string m_ConfigFilePath;
        protected readonly string m_RelatedJsonFilePath;

        #endregion

        #region Properties
        
        public bool HasJsonConfig { get; private set; }

        public string ConfigName { get; }
        public EConfigType ConfigType { get; }

        /// <summary>
        /// AttributeName -> AttributeInfo instance
        /// </summary>
        protected readonly Dictionary<string, ConfigAttributeInfo> ConfigAttributeDict;

        public ICollection<ConfigAttributeInfo> AttributeInfos => ConfigAttributeDict.Values;

        #endregion

        public ConfigInfo(EConfigType configType, string configName, string configFilePath, string relatedJsonFilePath)
        {
            ConfigType = configType;
            ConfigName = configName;
            ConfigAttributeDict = new Dictionary<string, ConfigAttributeInfo>();
            m_ConfigFilePath = configFilePath;
            m_RelatedJsonFilePath = relatedJsonFilePath;
        }

        #region LOAD ATTRIBUTES FROM CONFIG & JSON

        /// <summary>
        /// NOTICE: clean up attributes first
        /// </summary>
        public abstract ConfigInfo ReadConfigFileAttributes();

        /// <summary>
        /// add json related information to attributes
        /// </summary>
        public ConfigInfo ReadJsonFileAttributes()
        {
            HasJsonConfig = !string.IsNullOrEmpty(m_RelatedJsonFilePath) && File.Exists(m_RelatedJsonFilePath);
            if (!HasJsonConfig)
            {
                return this;
            }
            
            var jsonContent = File.ReadAllText(m_RelatedJsonFilePath, Encoding.UTF8);
            var jsonData = JsonMapper.ToObject(jsonContent);
            if (!jsonData.ContainsKey(ATTRIBUTES_KEY))
            {
                Debugger.LogError("'{0}' not found in json file {1}", ATTRIBUTES_KEY, m_RelatedJsonFilePath);
                return this;
            }

            var attributesJsonData = jsonData[ATTRIBUTES_KEY];
            foreach (var attributeJson in attributesJsonData)
            {
                var attributeJsonData = attributeJson as JsonData;
                if (attributeJsonData == null)
                {
                    continue;
                }

                var name = attributeJsonData[ConfigAttributeInfo.ATTRIBUTE_NAME_KEY].ToString();
                if (!ConfigAttributeDict.ContainsKey(name))
                {
                    Debugger.LogError("attribute '{0}' not found in config file {1}", name, m_ConfigFilePath);
                    continue;
                }

                ConfigAttributeDict[name].SetJsonFileInfo(attributeJsonData);
            }

            return this;
        }

        #endregion

        #region Seriailize

        internal void SaveJsonFile(string jsonFilePath)
        {
            var builder = new StringBuilder();
            var writer = new JsonWriter(builder)
            {
                PrettyPrint = true
            };

            writer.WriteObjectStart();
            writer.WritePropertyName(CONFIG_NAME_KEY);
            writer.Write(ConfigName);

            writer.WritePropertyName(ATTRIBUTES_KEY);
            writer.WriteArrayStart();
            foreach (var attribute in ConfigAttributeDict)
            {
                attribute.Value.WriteToJson(writer);
            }

            writer.WriteArrayEnd();
            writer.WriteObjectEnd();

            // make sure the directory existed
            Directory.CreateDirectory(Path.GetDirectoryName(jsonFilePath));
            var utf8 = new UTF8Encoding(false);
            using (var fs = File.Open(jsonFilePath, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs, utf8))
                {
                    sw.Write(builder.ToString());
                }
            }

            HasJsonConfig = true;
        }

        #endregion
    }
}