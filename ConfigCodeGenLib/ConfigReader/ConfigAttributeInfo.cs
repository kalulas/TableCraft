using System;
using System.Collections.Generic;
using System.Text;
using LitJson;

namespace ConfigCodeGenLib.ConfigReader
{
    /// <summary>
    /// Information about a single attribute in config file
    /// </summary>
    public class ConfigAttributeInfo
    {
        private readonly List<string> m_Usage = new List<string>();
        public int Index { get; private set; }
        public string AttributeName { get; private set; }
        public string Comment { get; private set; }
        public string ValueType { get; private set; } = string.Empty;
        public string CollectionType { get; private set; } = string.Empty;

        /// AttributeName & Comment can be read from config file (for example the first line and the second line of csv file)
        public ConfigAttributeInfo SetConfigFileInfo(int index, string attributeName, string comment)
        {
            Index = index;
            AttributeName = attributeName;
            Comment = comment;
            return this;
        }

        public void WriteToJson(JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("AttributeName");
            writer.Write(AttributeName);
            writer.WritePropertyName("ValueType");
            writer.Write(ValueType);
            writer.WritePropertyName("CollectionType");
            writer.Write(CollectionType);
            writer.WritePropertyName("Usage");
            writer.WriteArrayStart();
            foreach (var _usage in m_Usage)
            {
                writer.Write(_usage);
            }
            writer.WriteArrayEnd();
            writer.WriteObjectEnd();
        }

    }
}
