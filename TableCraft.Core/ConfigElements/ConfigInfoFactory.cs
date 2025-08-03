using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TableCraft.Core.Attributes;
using TableCraft.Core.Decorator;
using TableCraft.Core.Source;

namespace TableCraft.Core.ConfigElements
{
    internal static class ConfigInfoFactory
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

        internal static ConfigInfo CreateConfigInfo(string dataSourceFilePath, string[] dataDecoratorFilePaths)
        {
            var configName = Path.GetFileNameWithoutExtension(dataSourceFilePath);
            var source = CreateDataSource(dataSourceFilePath);
            
            var decorators = new List<IDataDecorator>();
            if (dataDecoratorFilePaths is {Length: > 0})
            {
                decorators.AddRange(dataDecoratorFilePaths.SkipWhile(string.IsNullOrEmpty).Select(CreateDataDecorator));
            }

            var configInfo = new ConfigInfo(configName).FillWith(source).DecorateWith(decorators);
            return configInfo;
        }

        internal static string[] GetDataSourceExtensions()
        {
            return m_RegisteredDataSources.Keys.ToArray();
        }
    }
}