using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;

namespace ConfigCodeGenLib.ConfigReader
{
    /// <summary>
    /// Information about a single attribute in config file
    /// </summary>
    public class ConfigAttributeInfo
    {
        #region Fields

        public const string ATTRIBUTE_NAME_KEY = "AttributeName";
        private const string COMMENT_KEY = "Comment";
        private const string VALUE_TYPE_KEY = "ValueType";
        private const string DEFAULT_VALUE_KEY = "DefaultValue";
        private const string COLLECTION_TYPE_KEY = "CollectionType";
        private const string USAGE_KEY = "Usages";
        private const string TAG_KEY = "Tag";

        private string m_ValueType;
        private string m_CollectionType;
        private readonly List<ConfigAttributeUsageInfo> m_UsageList;
        private readonly HashSet<string> m_TagList;

        #endregion

        #region Properties

        public int Index { get; private set; }
        public string AttributeName { get; private set; }
        public string Comment { get; set; }
        public string DefaultValue { get; set; }

        public string ValueType
        {
            get => m_ValueType;
            set
            {
                var valid = Configuration.IsValueTypeValid(value);
                if (!valid)
                {
                    throw new ArgumentException($"Illegal value type {value}");
                }
                
                m_ValueType = value;
            }
        }

        public string CollectionType
        {
            get => m_CollectionType;
            set
            {
                var valid = Configuration.IsCollectionTypeValid(value);
                if (!valid)
                {
                    throw new ArgumentException($"Illegal collection type {value}");
                }
                
                m_CollectionType = value;
            }
        }

        public ConfigAttributeUsageInfo[] AttributeUsageInfos => m_UsageList.ToArray();

        public string[] Tags => m_TagList.ToArray();

        #endregion

        public ConfigAttributeInfo()
        {
            m_ValueType = string.Empty;
            DefaultValue = string.Empty;
            m_CollectionType = string.Empty;
            m_UsageList = new List<ConfigAttributeUsageInfo>();
            m_TagList = new HashSet<string>();
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
            ValueType = jsonData[VALUE_TYPE_KEY].ToString();
            DefaultValue = jsonData[DEFAULT_VALUE_KEY].ToString();
            CollectionType = jsonData[COLLECTION_TYPE_KEY].ToString();
            Comment = jsonData[COMMENT_KEY].ToString();
            m_UsageList.Clear();
            if (jsonData[USAGE_KEY].IsArray)
            {
                foreach (var usage in jsonData[USAGE_KEY])
                {
                    if (!(usage is JsonData usageJsonData))
                    {
                        continue;
                    }

                    var newUsageInfo = new ConfigAttributeUsageInfo().ReadFromJson(usageJsonData);
                    if (newUsageInfo != null)
                    {
                        m_UsageList.Add(newUsageInfo);
                    }
                }
            }


            m_TagList.Clear();
            foreach (var tag in jsonData[TAG_KEY])
            {
                m_TagList.Add(tag.ToString());
            }

            return true;
        }

        public void WriteToJson(JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(ATTRIBUTE_NAME_KEY);
            writer.Write(AttributeName);
            writer.WritePropertyName(COMMENT_KEY);
            writer.Write(Comment);
            writer.WritePropertyName(VALUE_TYPE_KEY);
            writer.Write(ValueType);
            writer.WritePropertyName(DEFAULT_VALUE_KEY);
            writer.Write(DefaultValue);
            writer.WritePropertyName(COLLECTION_TYPE_KEY);
            writer.Write(CollectionType);
            writer.WritePropertyName(USAGE_KEY);
            writer.WriteArrayStart();
            foreach (var usage in m_UsageList)
            {
                usage.WriteToJson(writer);
            }

            writer.WriteArrayEnd();
            writer.WritePropertyName(TAG_KEY);
            writer.WriteArrayStart();
            foreach (var tag in m_TagList)
            {
                writer.Write(tag);
            }

            writer.WriteArrayEnd();
            writer.WriteObjectEnd();
        }

        #region Public API

        public bool HasTag(string tag)
        {
            return m_TagList.Contains(tag);
        }

        public bool HasUsage(string usage)
        {
            foreach (var usageInfo in m_UsageList)
            {
                if (usageInfo.Usage == usage)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetUsageFieldName(string usage)
        {
            var targetUsage = m_UsageList.Find(info => info.Usage == usage);
            if (targetUsage != null)
            {
                return targetUsage.FieldName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Valid only if <see cref="AttributeName"/> and <see cref="m_ValueType"/> not empty
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AttributeName) && !string.IsNullOrEmpty(m_ValueType);
        }

        /// <summary>
        /// Add new usage info into usage list
        /// </summary>
        /// <param name="usageInfo"></param>
        /// <returns> True if not duplicated </returns>
        public bool AddUsageInfo(ConfigAttributeUsageInfo usageInfo)
        {
            foreach (var attributeUsageInfo in m_UsageList)
            {
                if (attributeUsageInfo.Usage == usageInfo.Usage)
                {
                    return false;
                }
            }
            
            m_UsageList.Add(usageInfo);
            return true;
        }

        public bool RemoveUsageInfo(string usageType)
        {
            foreach (var attributeUsageInfo in m_UsageList)
            {
                if (attributeUsageInfo.Usage == usageType)
                {
                    m_UsageList.Remove(attributeUsageInfo);
                    return true;
                }
            }

            return false;
        }

        public bool AddTag(string tag)
        {
            return m_TagList.Add(tag);
        }

        #endregion
    }
}