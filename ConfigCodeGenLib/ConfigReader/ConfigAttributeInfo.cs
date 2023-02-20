﻿using System;
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
        private const string COMMENT_KEY = "Comment";
        private const string VALUE_TYPE_KEY = "ValueType";
        private const string DEFAULT_VALUE_KEY = "DefaultValue";
        private const string COLLECTION_TYPE_KEY = "CollectionType";
        private const string USAGE_KEY = "Usage";
        private const string TAG_KEY = "Tag";

        private string m_ValueType;
        private string m_CollectionType;
        private string m_DefaultValue;
        // TODO To hashset?
        private readonly List<string> m_Usage;
        // TODO To hashset?
        private readonly List<string> m_TagList;

        public int Index { get; private set; }
        public string AttributeName { get; private set; }
        public string Comment { get; private set; }
        public string ValueType { 
            get
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
        public string CollectionType { 
            get
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

        public string DefaultValue { 
            get 
            { 
                return m_DefaultValue; 
            } 
            private set 
            { 
                m_DefaultValue = value; 
            } 
        }

        public ConfigAttributeInfo()
        {
            m_ValueType = string.Empty;
            m_DefaultValue = string.Empty;
            m_CollectionType = Configuration.DefaultCollectionType;
            m_Usage = new List<string>();
            m_TagList = new List<string>();
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
                m_DefaultValue = jsonData[DEFAULT_VALUE_KEY].ToString();
                CollectionType = jsonData[COLLECTION_TYPE_KEY].ToString();
                Comment = jsonData[COMMENT_KEY].ToString();
                m_Usage.Clear();
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

                m_TagList.Clear();
                foreach (var tag in jsonData[TAG_KEY])
                {
                    m_TagList.Add(tag.ToString());
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
            writer.WritePropertyName(COMMENT_KEY);
            writer.Write(Comment);
            writer.WritePropertyName(VALUE_TYPE_KEY);
            writer.Write(ValueType);
            writer.WritePropertyName(DEFAULT_VALUE_KEY);
            writer.Write(m_DefaultValue);
            writer.WritePropertyName(COLLECTION_TYPE_KEY);
            writer.Write(CollectionType);
            writer.WritePropertyName(USAGE_KEY);
            writer.WriteArrayStart();
            foreach (var usage in m_Usage)
            {
                writer.Write(usage);
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
            return m_Usage.Contains(usage);
        }

        /// <summary>
        /// Valid only if <see cref="AttributeName"/> and <see cref="m_ValueType"/> not empty
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AttributeName) && !string.IsNullOrEmpty(m_ValueType);
        }

        #endregion
    }
}
