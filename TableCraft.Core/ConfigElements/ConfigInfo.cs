using System;
using System.Collections.Generic;
using System.Linq;
using TableCraft.Core.Decorator;
using TableCraft.Core.Source;

namespace TableCraft.Core.ConfigElements
{
    /// <summary>
    /// Information about a single config file, for example: SomeTable.csv
    /// </summary>
    public class ConfigInfo
    {
        #region Fields

        private IDataSource m_Source;
        private readonly List<IDataDecorator> m_Decorators;
        
        /// <summary>
        /// AttributeName -> AttributeInfo instance
        /// </summary>
        internal readonly Dictionary<string, ConfigAttributeInfo> ConfigAttributeDict;

        /// <summary>
        /// UsageName -> UsageInfo
        /// </summary>
        internal readonly Dictionary<string, ConfigUsageInfo> ConfigUsageDict;

        #endregion

        #region Properties

        public string ConfigName { get; }
        
        public string DataSourceFilePath => m_Source?.GetFilePath() ?? string.Empty;
        
        public IEnumerable<string> DataDecoratorFilePaths => m_Decorators.Select(decorator => decorator.GetFilePath());

        public IEnumerable<ConfigAttributeInfo> AttributeInfos => ConfigAttributeDict.Values;

        #endregion

        public ConfigInfo(string configName)
        {
            ConfigName = configName;
            m_Source = null;
            m_Decorators = new List<IDataDecorator>();
            ConfigAttributeDict = new Dictionary<string, ConfigAttributeInfo>();
            ConfigUsageDict = new Dictionary<string, ConfigUsageInfo>();
            PrepareUsageDict();
        }

        #region Private / Internal Methods

        private void PrepareUsageDict()
        {
            foreach (var usage in Configuration.ConfigUsageType)
            {
                ConfigUsageDict[usage] = new ConfigUsageInfo
                {
                    // default value
                    ExportName = ConfigName
                };
            }
        }
        
        internal ConfigInfo FillWith(IDataSource dataSource)
        {
            if (m_Source != null)
            {
                throw new Exception("ConfigInfo already filled with data source, multiple data source is not allowed");
            }
            
            m_Source = dataSource;
            return dataSource.Fill(this);
        }
        
        internal ConfigInfo DecorateWith(IDataDecorator decorator)
        {
            m_Decorators.Add(decorator);
            return decorator.Decorate(this);
        }

        internal ConfigInfo DecorateWith(IEnumerable<IDataDecorator> decorators)
        {
            foreach (var decorator in decorators)
            {
                decorator.Decorate(this);
                m_Decorators.Add(decorator);
            }
            
            return this;
        }

        /// <summary>
        /// Save all decorators to file after modification, every used decorator is saved in <see cref="m_Decorators"/>
        /// </summary>
        internal void SaveDecoratorsToFile()
        {
            if (m_Decorators.Count == 0)
            {
                Debugger.LogWarning("[ConfigInfo.SaveDecoratorsToFile] No decorator found");
                return;
            }

            foreach (var decorator in m_Decorators)
            {
                decorator.SaveToFile(this);
            }
        }

        internal bool SaveWith(IDataDecorator decorator)
        {
            return decorator.SaveToFile(this);
        }

        #endregion

        #region Public API

        public bool TryGetAttribute(string attributeName, out ConfigAttributeInfo attributeInfo)
        {
            return ConfigAttributeDict.TryGetValue(attributeName, out attributeInfo);
        }

        public bool TryGetUsageInfo(string usage, out ConfigUsageInfo usageInfo)
        {
            return ConfigUsageDict.TryGetValue(usage, out usageInfo);
        }

        /// <summary>
        /// Return ExportName if <paramref name="usage"/> was found, else return original <see cref="ConfigName"/>
        /// </summary>
        /// <param name="usage"></param>
        /// <returns></returns>
        public string GetExportName(string usage)
        {
            if (ConfigUsageDict.TryGetValue(usage, out var usageInfo))
            {
                return usageInfo.ExportName;
            }

            return ConfigName;
        }

        #endregion
    }
}