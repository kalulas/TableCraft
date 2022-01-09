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
        public const string ATTRIBUTE_NAME_KEY = "AttributeName";
        private const string VALUE_TYPE_KEY = "ValueType";
        private const string COLLECTION_TYPE_KEY = "CollectionType";
        private const string USAGE_KEY = "Usage";

        private readonly List<string> m_Usage = new List<string>();
        private string m_ValueType = string.Empty;
        private string m_CollectionType = string.Empty;

        public int Index { get; private set; }
        public string AttributeName { get; private set; }
        public string Comment { get; private set; }
        public string ValueType { get
            {
                return m_ValueType;
            }
            private set
            {
                var valid = Configuration.IsValueTypeValid(value);
                m_ValueType = valid ? value : string.Empty;
                if (!valid)
                {
                    Debugger.LogErrorFormat("value type '{0}' is not valid", value);
                }
            }
        }
        public string CollectionType { get
            {
                return m_CollectionType;
            }
            private set
            {
                var valid = Configuration.IsCollectionTypeValid(value);
                m_CollectionType = valid ? value : string.Empty;
                if (!valid)
                {
                    Debugger.LogErrorFormat("collection type '{0}' is not valid", value);
                }
            }
        }

        /// AttributeName & Comment can be read from config file (for example the first line and the second line of csv file)
        public ConfigAttributeInfo SetConfigFileInfo(int index, string attributeName, string comment)
        {
            Index = index;
            AttributeName = attributeName;
            Comment = comment;
            return this;
        }

        public bool SetJsonFileInfo(JsonData jsonData)
        {
            try
            {
                ValueType = jsonData[VALUE_TYPE_KEY].ToString();
                CollectionType = jsonData[COLLECTION_TYPE_KEY].ToString();
                foreach (var usage in jsonData[USAGE_KEY])
                {
                    var valid = Configuration.IsUsageValid(usage.ToString());
                    if (!valid)
                    {
                        Debugger.LogErrorFormat("usage '{0}' is not valid", usage.ToString());
                    }
                    else
                    {
                        m_Usage.Add(usage.ToString());
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public void WriteToJson(JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(ATTRIBUTE_NAME_KEY);
            writer.Write(AttributeName);
            writer.WritePropertyName(VALUE_TYPE_KEY);
            writer.Write(ValueType);
            writer.WritePropertyName(COLLECTION_TYPE_KEY);
            writer.Write(CollectionType);
            writer.WritePropertyName(USAGE_KEY);
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
