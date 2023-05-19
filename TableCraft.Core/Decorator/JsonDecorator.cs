#region File Header

// Filename: JsonDecorator.cs
// Author: Kalulas
// Create: 2023-04-09
// Description:

#endregion

using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using TableCraft.Core.Attributes;
using TableCraft.Core.ConfigElements;
using TableCraft.Core.IO;

namespace TableCraft.Core.Decorator;

[WithExtension("json")]
public class JsonDecorator : IDataDecorator
{
    #region Fields

    private const string ATTRIBUTES_KEY = "Attributes";
    private const string CONFIG_NAME_KEY = "ConfigName";
    private const string USAGE_KEY = "Usage";

    private const string ATTRIBUTE_NAME_KEY = "AttributeName";
    private const string ATTRIBUTE_COMMENT_KEY = "Comment";
    private const string ATTRIBUTE_VALUE_TYPE_KEY = "ValueType";
    private const string ATTRIBUTE_DEFAULT_VALUE_KEY = "DefaultValue";
    private const string ATTRIBUTE_COLLECTION_TYPE_KEY = "CollectionType";
    private const string ATTRIBUTE_USAGE_KEY = "Usages";
    private const string ATTRIBUTE_TAG_KEY = "Tag";
    
    private const string ATTRIBUTE_USAGE_FIELD_NAME_KEY = "FieldName";

    private readonly string m_FilePath;

    #endregion

    #region Constructors

    public JsonDecorator(string filePath)
    {
        m_FilePath = filePath;
    }

    #endregion

    #region Private Methods
    
    private static void DecorateConfigUsage(ConfigInfo configInfo, JsonData usageJsonData)
    {
        foreach (var usageConfig in usageJsonData)
        {
            var (usage, usageInfoJsonData) = (KeyValuePair<string, JsonData>) usageConfig;
            var usageInfo = JsonMapper.ToObject<ConfigUsageInfo>(usageInfoJsonData.ToJson());
            if (!configInfo.TryGetUsageInfo(usage, out var existedUsageInfo))
            {
                Debugger.LogWarning($"[JsonDecorator.DecorateConfigUsage] Usage '{usage}' is not available under current configuration, ignore");
                continue;
            }

            existedUsageInfo.ExportName = usageInfo.ExportName;
        }
    }

    private static (ConfigAttributeUsageInfo, bool) DecorateConfigAttributeUsage(ConfigAttributeUsageInfo attributeUsageInfo,
        JsonData attributeUsageJsonData)
    {
        attributeUsageInfo.Usage = attributeUsageJsonData[USAGE_KEY].ToString();
        if (!Configuration.IsUsageValid(attributeUsageInfo.Usage))
        {
            Debugger.LogError("[JsonDecorator.DecorateConfigAttributeUsage] usage '{0}' is not valid",
                attributeUsageInfo.Usage);
            return (attributeUsageInfo, false);
        }

        attributeUsageInfo.FieldName = attributeUsageJsonData[ATTRIBUTE_USAGE_FIELD_NAME_KEY].ToString();
        return (attributeUsageInfo, true);
    }

    private static bool DecorateConfigAttribute(ConfigInfo configInfo, JsonData attributeJsonData)
    {
        var name = attributeJsonData[ATTRIBUTE_NAME_KEY].ToString();
        if (!configInfo.TryGetAttribute(name, out var attributeInfo))
        {
            Debugger.LogError($"[JsonDecorator.DecorateConfigAttribute] attribute '{name}' not found in config file {configInfo.ConfigName}");
            return false;
        }
        
        attributeInfo.ValueType = attributeJsonData[ATTRIBUTE_VALUE_TYPE_KEY].ToString();
        attributeInfo.DefaultValue = attributeJsonData[ATTRIBUTE_DEFAULT_VALUE_KEY].ToString();
        attributeInfo.CollectionType = attributeJsonData[ATTRIBUTE_COLLECTION_TYPE_KEY].ToString();
        // overwriting previous comment from data source
        attributeInfo.Comment = attributeJsonData[ATTRIBUTE_COMMENT_KEY].ToString();
        
        attributeInfo.UsageList.Clear();
        foreach (var usage in attributeJsonData[ATTRIBUTE_USAGE_KEY])
        {
            var (newUsageInfo, success) = DecorateConfigAttributeUsage(new ConfigAttributeUsageInfo(), (JsonData)usage);
            if (success)
            {
                attributeInfo.AddUsageInfo(newUsageInfo);
            }
        }
        
        attributeInfo.TagList.Clear();
        foreach (var tag in attributeJsonData[ATTRIBUTE_TAG_KEY])
        {
            attributeInfo.AddTag(tag.ToString());
        }

        return true;
    }

    private static void WriteConfigAttributeToJson(ConfigAttributeInfo attributeInfo, JsonWriter writer)
    {
        writer.WriteObjectStart();
        writer.WritePropertyName(ATTRIBUTE_NAME_KEY);
        writer.Write(attributeInfo.AttributeName);
        writer.WritePropertyName(ATTRIBUTE_COMMENT_KEY);
        writer.Write(attributeInfo.Comment);
        writer.WritePropertyName(ATTRIBUTE_VALUE_TYPE_KEY);
        writer.Write(attributeInfo.ValueType);
        writer.WritePropertyName(ATTRIBUTE_DEFAULT_VALUE_KEY);
        writer.Write(attributeInfo.DefaultValue);
        writer.WritePropertyName(ATTRIBUTE_COLLECTION_TYPE_KEY);
        writer.Write(attributeInfo.CollectionType);
        writer.WritePropertyName(ATTRIBUTE_USAGE_KEY);
        JsonMapper.ToJson(attributeInfo.UsageList, writer);
        // writer.WriteArrayStart();
        // foreach (var usage in attributeInfo.UsageList)
        // {
        //     JsonMapper.ToJson(usage, writer);
        // }
        //
        // writer.WriteArrayEnd();
        writer.WritePropertyName(ATTRIBUTE_TAG_KEY);
        // for HastSet, ToJson will save information(Count / Comparer) we don't need
        // JsonMapper.ToJson(attributeInfo.TagList, writer);
        writer.WriteArrayStart();
        foreach (var tag in attributeInfo.TagList)
        {
            writer.Write(tag);
        }
        
        writer.WriteArrayEnd();
        writer.WriteObjectEnd();
    }

    #endregion

    #region Public API

    public ConfigInfo Decorate(ConfigInfo configInfo)
    {
        if (!File.Exists(m_FilePath))
        {
            Debugger.LogError($"[JsonDecorator.Decorate] '{m_FilePath}' not found!");
            return null;
        }

        var jsonContent = FileHelper.ReadAllText(m_FilePath);
        var jsonData = JsonMapper.ToObject(jsonContent);
        if (!jsonData.ContainsKey(CONFIG_NAME_KEY))
        {
            Debugger.LogError($"[JsonDecorator.Decorate] '{CONFIG_NAME_KEY}' not found in json file {m_FilePath}");
            return configInfo;
        }

        var configName = jsonData[CONFIG_NAME_KEY].ToString();
        if (configName != configInfo.ConfigName)
        {
            Debugger.LogWarning($"[JsonDecorator.Decorate] ConfigName doesn't match: json({configName}), runtime({configInfo.ConfigName})");
        }

        if (!jsonData.ContainsKey(USAGE_KEY))
        {
            Debugger.LogError($"[JsonDecorator.Decorate] '{USAGE_KEY}' not found in json file {m_FilePath}");
            return configInfo;
        }

        var usageJsonData = jsonData[USAGE_KEY];
        DecorateConfigUsage(configInfo, usageJsonData);

        if (!jsonData.ContainsKey(ATTRIBUTES_KEY))
        {
            Debugger.LogError($"[JsonDecorator.Decorate] '{ATTRIBUTES_KEY}' not found in json file {m_FilePath}");
            return configInfo;
        }

        var attributesJsonData = jsonData[ATTRIBUTES_KEY];
        foreach (var attributeJson in attributesJsonData)
        {
            if (attributeJson is not JsonData attributeJsonData)
            {
                continue;
            }
            
            var success = DecorateConfigAttribute(configInfo, attributeJsonData);
            if (!success)
            {
                Debugger.LogError(
                    $"[JsonDecorator.Decorate] failed to decorate attribute {attributeJsonData[ATTRIBUTE_NAME_KEY]}");
            }
        }

        return configInfo;
    }

    public bool SaveToFile(ConfigInfo configInfo)
    {
        var builder = new StringBuilder();
        var writer = new JsonWriter(builder)
        {
            PrettyPrint = true
        };

        writer.WriteObjectStart();
        writer.WritePropertyName(CONFIG_NAME_KEY);
        writer.Write(configInfo.ConfigName);
            
        writer.WritePropertyName(USAGE_KEY);
        JsonMapper.ToJson(configInfo.ConfigUsageDict, writer);

        writer.WritePropertyName(ATTRIBUTES_KEY);
        writer.WriteArrayStart();
        foreach (var attribute in configInfo.ConfigAttributeDict)
        {
            WriteConfigAttributeToJson(attribute.Value, writer);
        }

        writer.WriteArrayEnd();
        writer.WriteObjectEnd();

        // make sure the directory existed
        var directoryName = Path.GetDirectoryName(m_FilePath);
        if (directoryName != null)
        {
            Directory.CreateDirectory(directoryName);
        }
        
        FileHelper.Write(m_FilePath, builder.ToString());
        return true;
    }

    public string GetFilePath()
    {
        return m_FilePath;
    }

    #endregion
}