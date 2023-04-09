using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TableCraft.Core.Attributes;
using TableCraft.Core.Decorator;
using TableCraft.Core.Source;

namespace TableCraft.Core.ConfigReader
{
    public static class ConfigInfoFactory
    {
        static ConfigInfoFactory()
        {
            LoadAllDefaultRegistration();
        }
        
        private static readonly Dictionary<string, Type> m_RegisteredDataSources = new();
        private static readonly Dictionary<string, Type> m_RegisteredDataDecorators = new();

        private static IDataSource CreateDataSource(string filepath)
        {
            var extension = Path.GetExtension(filepath).Replace(".", "");
            if (m_RegisteredDataSources.TryGetValue(extension, out var dataSourceType))
            {
                if (Activator.CreateInstance(dataSourceType, filepath) is not IDataSource dataSource)
                {
                    throw new Exception(
                        $"[ConfigInfoFactory.CreateDataSource] Create DataSource instance for extension {extension} failed!");
                }

                return dataSource;
            }

            throw new Exception($"[ConfigInfoFactory.CreateDataSource] not supported extension: {extension}");
        }

        internal static IDataDecorator CreateDataDecorator(string filepath)
        {
            var extension = Path.GetExtension(filepath).Replace(".", "");
            if (m_RegisteredDataDecorators.TryGetValue(extension, out var dataDecoratorType))
            {
                if (Activator.CreateInstance(dataDecoratorType, filepath) is not IDataDecorator dataDecorator)
                {
                    throw new Exception(
                        $"[ConfigInfoFactory.CreateDataDecorator] Create DataDecorator instance for extension {extension} failed!");
                }

                return dataDecorator;
            }

            throw new Exception($"[ConfigInfoFactory.CreateDataDecorator] not supported extension: {extension}");
        }

        private static void LoadAllDefaultRegistration()
        {
            m_RegisteredDataSources.Clear();
            // get all types that implement IDataSource in current assembly
            var dataSources = Assembly.GetAssembly(typeof(ConfigManager))?.GetTypes()
                .Where(type => typeof(IDataSource).IsAssignableFrom(type));
            if (dataSources != null)
            {
                foreach (var dataSource in dataSources)
                {
                    var extensionAttributes = dataSource.GetCustomAttributes(typeof(WithExtensionAttribute), false);
                    if (extensionAttributes.Length <= 0) continue;
                    if (extensionAttributes[0] is not WithExtensionAttribute extensionAttribute) continue;
                    var extension = extensionAttribute.Extension;
                    m_RegisteredDataSources.TryAdd(extension, dataSource);
                }
            }
            
            m_RegisteredDataDecorators.Clear();
            var dataDecorators = Assembly.GetAssembly(typeof(ConfigManager))?.GetTypes()
                .Where(type => typeof(IDataDecorator).IsAssignableFrom(type));
            if (dataDecorators != null)
            {
                foreach (var dataDecorator in dataDecorators)
                {
                    var extensionAttributes = dataDecorator.GetCustomAttributes(typeof(WithExtensionAttribute), false);
                    if (extensionAttributes.Length <= 0) continue;
                    if (extensionAttributes[0] is not WithExtensionAttribute extensionAttribute) continue;
                    var extension = extensionAttribute.Extension;
                    m_RegisteredDataDecorators.TryAdd(extension, dataDecorator);
                }
            }
        }
        
        public static ConfigInfo CreateConfigInfo(EConfigType configType, string configFilePath)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            var relatedJsonPath = string.Empty;
            switch (configType)
            {
                case EConfigType.CSV:
                    ConfigInfo configInfo = new CSVConfigInfo(configType, configName, configFilePath, relatedJsonPath);
                    return configInfo.ReadConfigFileAttributes()?.ReadJsonFileAttributes();
                default:
                    Debugger.Log("[ConfigInfoFactory.CreateConfigInfo] not supported config type!");
                    return null;
            }
        }

        public static ConfigInfo CreateConfigInfo(EConfigType configType, string configFilePath, string jsonFilePath)
        {
            var configName = Path.GetFileNameWithoutExtension(configFilePath);
            switch (configType)
            {
                case EConfigType.CSV:
                    ConfigInfo configInfo = new CSVConfigInfo(configType, configName, configFilePath, jsonFilePath);
                    return configInfo.ReadConfigFileAttributes()?.ReadJsonFileAttributes();
                default:
                    Debugger.Log("[ConfigInfoFactory.CreateConfigInfo] not supported config type!");
                    return null;
            }
        }
        
        public static ConfigInfo CreateConfigInfo(string dataSourceFilePath, string[] dataDecoratorFilePaths)
        {
            var configName = Path.GetFileNameWithoutExtension(dataSourceFilePath);
            var source = CreateDataSource(dataSourceFilePath);
            
            string decoratorFilePath;
            var decorators = new List<IDataDecorator>();
            if (dataDecoratorFilePaths != null && dataDecoratorFilePaths.Length > 0)
            {
                decoratorFilePath = dataDecoratorFilePaths[0];
                decorators.AddRange(dataDecoratorFilePaths.Select(CreateDataDecorator));
            }
            else
            {
                decoratorFilePath = string.Empty;
            }

            var configInfo = new ConfigInfo(configName, dataSourceFilePath, decoratorFilePath).FillWith(source)
                .DecorateWith(decorators);
            return configInfo;
        }
    }
}