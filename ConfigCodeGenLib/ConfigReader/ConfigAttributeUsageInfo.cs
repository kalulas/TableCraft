

using System;
using LitJson;

namespace ConfigCodeGenLib.ConfigReader
{
    /// <summary>
    /// Contains attribute's information at specific usage
    /// </summary>
    public class ConfigAttributeUsageInfo
    {
        private const string USAGE_KEY = "Usage";
        private const string FIELD_NAME_KEY = "FieldName";
        public string FieldName { get; private set; }
        public string Usage { get; private set; }

        public ConfigAttributeUsageInfo()
        {
            FieldName = string.Empty;
            Usage = string.Empty;
        }

        public ConfigAttributeUsageInfo ReadFromJson(JsonData jsonData)
        {
            FieldName = jsonData[FIELD_NAME_KEY].ToString();
            Usage = jsonData[USAGE_KEY].ToString();
            if (!Configuration.IsUsageValid(Usage))
            {
                Debugger.LogErrorFormat("usage '{0}' is not valid", Usage);
                return null;
            }
            
            return this;
        }

        public void WriteToJson(JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(USAGE_KEY);
            writer.Write(Usage);
            writer.WritePropertyName(FIELD_NAME_KEY);
            writer.Write(FieldName);
            writer.WriteObjectEnd();
        }
    }
}