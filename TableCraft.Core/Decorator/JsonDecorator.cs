#region File Header

// Filename: JsonDecorator.cs
// Author: Kalulas
// Create: 2023-04-09
// Description:

#endregion

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    
    private static void DecorateConfigUsage(ConfigInfo configInfo, JsonObject usageJsonObject)
    {
        foreach (var usageConfig in usageJsonObject)
        {
            if (usageConfig.Value == null) continue;
            
            var configUsageInfoContent = usageConfig.Value.ToString();
            var usageInfo = JsonSerializer.Deserialize<ConfigUsageInfo>(configUsageInfoContent);
            if (usageInfo == null)
            {
                Debugger.LogWarning(
                    $"[JsonDecorator.DecorateConfigUsage] Failed to deserialize ConfigUsageInfo from key '{usageConfig.Key}' value '{configUsageInfoContent}'");
                continue;
            }
            
            if (!configInfo.TryGetUsageInfo(usageConfig.Key, out var existedUsageInfo))
            {
                Debugger.LogWarning($"[JsonDecorator.DecorateConfigUsage] Usage '{usageConfig.Key}' is not available under current configuration, ignore");
                continue;
            }

            existedUsageInfo.ExportName = usageInfo.ExportName;
        }
    }

    private static (ConfigAttributeUsageInfo, bool) DecorateConfigAttributeUsage(ConfigAttributeUsageInfo attributeUsageInfo,
        JsonObject attributeUsageJsonObject)
    {
        if (!attributeUsageJsonObject.TryGetPropertyValue(USAGE_KEY, out var usageNode))
        {
            return (attributeUsageInfo, false);
        }
        
        attributeUsageInfo.Usage = usageNode?.ToString() ?? string.Empty;
        if (!Configuration.IsUsageValid(attributeUsageInfo.Usage))
        {
            Debugger.LogError("[JsonDecorator.DecorateConfigAttributeUsage] usage '{0}' is not valid",
                attributeUsageInfo.Usage);
            return (attributeUsageInfo, false);
        }

        if (attributeUsageJsonObject.TryGetPropertyValue(ATTRIBUTE_USAGE_FIELD_NAME_KEY, out var fieldNameNode))
        {
            attributeUsageInfo.FieldName = fieldNameNode?.ToString() ?? string.Empty;
        }
        
        return (attributeUsageInfo, true);
    }

    private static bool DecorateConfigAttribute(ConfigInfo configInfo, JsonObject attributeJsonObject)
    {
        if (!attributeJsonObject.TryGetPropertyValue(ATTRIBUTE_NAME_KEY, out var nameNode))
        {
            return false;
        }
        
        var name = nameNode?.ToString() ?? string.Empty;
        if (!configInfo.TryGetAttribute(name, out var attributeInfo))
        {
            Debugger.LogError($"[JsonDecorator.DecorateConfigAttribute] attribute '{name}' not found in config file {configInfo.ConfigName}");
            return false;
        }

        attributeInfo.ValueType = attributeJsonObject[ATTRIBUTE_VALUE_TYPE_KEY].ToString();
        attributeInfo.DefaultValue = attributeJsonObject[ATTRIBUTE_DEFAULT_VALUE_KEY].ToString();
        attributeInfo.CollectionType = attributeJsonObject[ATTRIBUTE_COLLECTION_TYPE_KEY].ToString();
        attributeInfo.Comment = attributeJsonObject[ATTRIBUTE_COMMENT_KEY].ToString();
        
        attributeInfo.UsageList.Clear();
        if (attributeJsonObject.TryGetPropertyValue(ATTRIBUTE_USAGE_KEY, out var usageArrayNode) &&
            usageArrayNode is JsonArray usageArray)
        {
            foreach (var usage in usageArray)
            {
                if (usage is JsonObject usageObject)
                {
                    var (newUsageInfo, success) =
                        DecorateConfigAttributeUsage(new ConfigAttributeUsageInfo(), usageObject);
                    if (success)
                    {
                        attributeInfo.AddUsageInfo(newUsageInfo);
                    }
                }
            }
        }
        
        attributeInfo.TagList.Clear();
        if (attributeJsonObject.TryGetPropertyValue(ATTRIBUTE_TAG_KEY, out var tagArrayNode) &&
            tagArrayNode is JsonArray tagArray)
        {
            foreach (var tag in tagArray)
            {
                if (tag != null)
                {
                    attributeInfo.AddTag(tag.ToString());
                }
            }
        }

        return true;
    }

    private static JsonObject WriteConfigAttributeToJson(ConfigAttributeInfo attributeInfo)
    {
        var attributeObject = new JsonObject
        {
            [ATTRIBUTE_NAME_KEY] = attributeInfo.AttributeName,
            [ATTRIBUTE_COMMENT_KEY] = attributeInfo.Comment,
            [ATTRIBUTE_VALUE_TYPE_KEY] = attributeInfo.ValueType,
            [ATTRIBUTE_DEFAULT_VALUE_KEY] = attributeInfo.DefaultValue,
            [ATTRIBUTE_COLLECTION_TYPE_KEY] = attributeInfo.CollectionType
        };
        
        var usageArray = new JsonArray();
        foreach (var usage in attributeInfo.UsageList)
        {
            var usageJson = JsonSerializer.Serialize(usage);
            var usageNode = JsonNode.Parse(usageJson);
            if (usageNode != null)
            {
                usageArray.Add(usageNode);
            }
        }

        attributeObject[ATTRIBUTE_USAGE_KEY] = usageArray;
        
        var tagArray = new JsonArray();
        foreach (var tag in attributeInfo.TagList)
        {
            tagArray.Add(tag);
        }

        attributeObject[ATTRIBUTE_TAG_KEY] = tagArray;
        return attributeObject;
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
        if (JsonNode.Parse(jsonContent) is not JsonObject jsonObject)
        {
            Debugger.LogError($"[JsonDecorator.Decorate] Failed to parse JSON as object from: {m_FilePath}");
            return configInfo;
        }
        
        if (!jsonObject.TryGetPropertyValue(CONFIG_NAME_KEY, out var configNameNode))
        {
            Debugger.LogError($"[JsonDecorator.Decorate] '{CONFIG_NAME_KEY}' not found in json file: {m_FilePath}");
            return configInfo;
        }

        var configName = configNameNode?.ToString() ?? string.Empty;
        if (configName != configInfo.ConfigName)
        {
            Debugger.LogWarning($"[JsonDecorator.Decorate] ConfigName doesn't match: json({configName}), runtime({configInfo.ConfigName})");
        }

        if (!jsonObject.TryGetPropertyValue(USAGE_KEY, out var usageNode) || usageNode is not JsonObject usageObject)
        {
            Debugger.LogError($"[JsonDecorator.Decorate] '{USAGE_KEY}' not found in json file {m_FilePath}");
            return configInfo;
        }

        DecorateConfigUsage(configInfo, usageObject);

        if (!jsonObject.TryGetPropertyValue(ATTRIBUTES_KEY, out var attributesNode) || attributesNode is not JsonArray attributesArray)
        {
            Debugger.LogError($"[JsonDecorator.Decorate] '{ATTRIBUTES_KEY}' not found in json file {m_FilePath}");
            return configInfo;
        }

        foreach (var attributeNode in attributesArray)
        {
            if (attributeNode is JsonObject attributeObject)
            {
                var success = DecorateConfigAttribute(configInfo, attributeObject);
                if (!success)
                {
                    if (attributeObject.TryGetPropertyValue(ATTRIBUTE_NAME_KEY, out var nameNode) && nameNode != null)
                    {
                        Debugger.LogError($"[JsonDecorator.Decorate] failed to decorate attribute '{nameNode}' in config file {configName}");
                    }
                }
            }
        }

        return configInfo;
    }

    public bool SaveToFile(ConfigInfo configInfo)
    {
        var rootObject = new JsonObject
        {
            [CONFIG_NAME_KEY] = configInfo.ConfigName
        };

        var usageJson = JsonSerializer.Serialize(configInfo.ConfigUsageDict);
        var usageNode = JsonNode.Parse(usageJson);
        if (usageNode != null)
        {
            rootObject[USAGE_KEY] = usageNode;
        }

        var attributesArray = new JsonArray();
        foreach (var attribute in configInfo.ConfigAttributeDict)
        {
            var attributeObject = WriteConfigAttributeToJson(attribute.Value);
            attributesArray.Add(attributeObject);
        }

        rootObject[ATTRIBUTES_KEY] = attributesArray;

        var directoryName = Path.GetDirectoryName(m_FilePath);
        if (directoryName != null)
        {
            Directory.CreateDirectory(directoryName);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var jsonString = rootObject.ToJsonString(options);
        FileHelper.Write(m_FilePath, jsonString);
        return true;
    }

    public string GetFilePath()
    {
        return m_FilePath;
    }

    #endregion
}