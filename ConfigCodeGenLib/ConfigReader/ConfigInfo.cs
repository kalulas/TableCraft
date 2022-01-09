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
        private const string ATTRIBUTES_KEY = "Attributes";

        private bool m_HasJsonConfig;
        protected string m_ConfigFilePath;
        protected string m_RelatedJsonFilePath;
        protected List<string> m_Usage;
        /// <summary>
        /// AttributeName -> AttributeInfo instance
        /// </summary>
        protected Dictionary<string, ConfigAttributeInfo> m_Attribtues;
        public string ConfigName { get; private set; }
        public EConfigType ConfigType { get; private set; }


        public ConfigInfo(EConfigType configType, string configName, string configFilePath, string relatedJsonFilePath)
        {
            ConfigType = configType;
            ConfigName = configName;
            m_Usage = new List<string>();
            m_Attribtues = new Dictionary<string, ConfigAttributeInfo>();
            m_ConfigFilePath = configFilePath;
            m_RelatedJsonFilePath = relatedJsonFilePath;
            if (!File.Exists(m_ConfigFilePath))
            {
                Console.WriteLine("[ConfigReader.ConfigInfo] {0} file not found", m_ConfigFilePath);
                return;
            }

            ReadConfigFileAttributes();
            m_HasJsonConfig = File.Exists(m_RelatedJsonFilePath);
            if (!m_HasJsonConfig)
            {
                return;
            }

            ReadJsonFileAttributes();
        }

        #region LOAD ATTRIBUTES FROM CONFIG & JSON

        /// <summary>
        /// NOTICE: clean up attributes first
        /// </summary>
        /// <param name="configInfo"></param>
        /// <param name="attibutes"></param>
        protected abstract void ReadConfigFileAttributes();

        /// <summary>
        /// add json related information to attributes
        /// </summary>
        protected void ReadJsonFileAttributes()
        {
            var jsonContent = File.ReadAllText(m_RelatedJsonFilePath, Encoding.UTF8);
            var jsonData = JsonMapper.ToObject(jsonContent);
            if (!jsonData.ContainsKey(ATTRIBUTES_KEY))
            {
                // TODO Error Log
                return;
            }

            var attributesJsonData = jsonData["Attributes"];
            foreach (var attributeJson in attributesJsonData)
            {
                var attributeJsonData = attributeJson as JsonData;
                if (attributeJsonData == null)
                {
                    continue;
                }
                var name = attributeJsonData[ConfigAttributeInfo.ATTRIBUTE_NAME_KEY].ToString();
                if (!m_Attribtues.ContainsKey(name))
                {
                    // TODO error log
                    continue;
                }

                m_Attribtues[name].SetJsonFileInfo(attributeJsonData);
            }
        }

        #endregion

        #region SAVE JSON

        public void SaveJsonFile()
        {
            var builder = new StringBuilder();
            var writer = new JsonWriter(builder)
            {
                PrettyPrint = true
            };

            writer.WriteObjectStart();
            writer.WritePropertyName("ConfigName");
            writer.Write(ConfigName);
            // NOTE: type is user-specified
            //writer.WritePropertyName("ConfigType");
            //writer.Write((int)ConfigType);

            writer.WritePropertyName("Usage");
            writer.WriteArrayStart();
            foreach (var _usage in m_Usage)
            {
                writer.Write(_usage);
            }
            writer.WriteArrayEnd();

            writer.WritePropertyName("Attributes");
            writer.WriteArrayStart();
            foreach (var attribute in m_Attribtues)
            {
                attribute.Value.WriteToJson(writer);
            }
            writer.WriteArrayEnd();

            writer.WriteObjectEnd();
            var dataBytes = Encoding.UTF8.GetBytes(builder.ToString());
            using (var fs = File.Open(m_RelatedJsonFilePath, FileMode.Create))
            {
                fs.Write(dataBytes, 0, dataBytes.Length);
            }
        }

        #endregion

        /// <summary>
        /// Factory Method Example
        /// </summary>
        /// <param name="configType"></param>
        /// <param name="configFilePath"></param>
        /// <returns></returns>
        public static ConfigInfo CreateConfigInfo(EConfigType configType, string configFilePath, bool createJsonFile = false)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            var relatedJsonPath = Configuration.ConfigJsonPath + configName + ".json";
            Console.WriteLine("\'{0}\' related json file is \'{1}\'", configName, relatedJsonPath);
            ConfigInfo configInfo = null;
            switch (configType)
            {
                case EConfigType.CSV:
                    configInfo = new CSVConfigInfo(configType, configName, configFilePath, relatedJsonPath);
                    break;
                default:
                    Console.WriteLine("[ConfigInfo.CreateConfigInfo] no supoorted config type!");
                    break;
            }

            if (createJsonFile)
            {
                configInfo.SaveJsonFile();
            }

            return configInfo;
        }
    }
}
