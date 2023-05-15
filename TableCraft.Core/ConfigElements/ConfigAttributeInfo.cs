using System;
using System.Collections.Generic;
using System.Linq;

namespace TableCraft.Core.ConfigElements
{
    /// <summary>
    /// Information about a single attribute in config file
    /// </summary>
    public class ConfigAttributeInfo
    {
        #region Fields
        
        private string m_ValueType;
        private string m_CollectionType;
        internal readonly List<ConfigAttributeUsageInfo> UsageList;
        internal readonly HashSet<string> TagList;

        #endregion

        #region Properties

        public int Index { get; internal set; }
        public string AttributeName { get; internal set; }
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
                    throw new ArgumentException($"Illegal value type '{value}' for attribute '{AttributeName}'");
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
                    throw new ArgumentException($"Illegal collection type '{value}' for attribute '{AttributeName}'");
                }
                
                m_CollectionType = value;
            }
        }

        public IEnumerable<ConfigAttributeUsageInfo> AttributeUsageInfos => UsageList.ToArray();

        public IEnumerable<string> Tags => TagList.ToArray();

        #endregion

        public ConfigAttributeInfo()
        {
            m_ValueType = string.Empty;
            DefaultValue = string.Empty;
            m_CollectionType = string.Empty;
            UsageList = new List<ConfigAttributeUsageInfo>();
            TagList = new HashSet<string>();
        }

        #region Public API
        
        /// <summary>
        /// Valid only if <see cref="AttributeName"/> and <see cref="m_ValueType"/> not empty
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AttributeName) && !string.IsNullOrEmpty(m_ValueType);
        }

        public bool HasUsage(string usage)
        {
            foreach (var usageInfo in UsageList)
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
            var targetUsage = UsageList.Find(info => info.Usage == usage);
            if (targetUsage != null)
            {
                return targetUsage.FieldName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Add new usage info into usage list
        /// </summary>
        /// <param name="usageInfo"></param>
        /// <returns> True if not duplicated </returns>
        public bool AddUsageInfo(ConfigAttributeUsageInfo usageInfo)
        {
            foreach (var attributeUsageInfo in UsageList)
            {
                if (attributeUsageInfo.Usage == usageInfo.Usage)
                {
                    return false;
                }
            }
            
            UsageList.Add(usageInfo);
            return true;
        }

        /// <summary>
        /// Remove usageInfo from <see cref="UsageList"/>
        /// </summary>
        /// <param name="usageType"></param>
        /// <returns> True if removed </returns>
        public bool RemoveUsageInfo(string usageType)
        {
            foreach (var attributeUsageInfo in UsageList)
            {
                if (attributeUsageInfo.Usage == usageType)
                {
                    UsageList.Remove(attributeUsageInfo);
                    return true;
                }
            }

            return false;
        }
        
        public bool HasTag(string tag)
        {
            return TagList.Contains(tag);
        }

        public bool AddTag(string tag)
        {
            return TagList.Add(tag);
        }
        
        /// <summary>
        /// Remove tag from <see cref="TagList"/>
        /// </summary>
        /// <param name="tagContent"></param>
        /// <returns> True if removed </returns>
        public bool RemoveTag(string tagContent)
        {
            return TagList.Remove(tagContent);
        }

        #endregion
    }
}