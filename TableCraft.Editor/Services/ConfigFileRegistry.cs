using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using TableCraft.Core.IO;
using TableCraft.Editor.Models;

namespace TableCraft.Editor.Services;

public class ConfigFileRegistry
{
    private readonly string m_ListJsonFilePath;

    public ConfigFileRegistry(string listJsonFilePath)
    {
        m_ListJsonFilePath = listJsonFilePath;
    }
    
    /// <summary>
    /// Read relative directory information for all existed files
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ConfigFileElement> ReadTableElements()
    {
        if (!File.Exists(m_ListJsonFilePath))
        {
            throw new FileNotFoundException(m_ListJsonFilePath + " not found");
        }

        var listJsonFileContent = FileHelper.ReadAllText(m_ListJsonFilePath);
        using var jsonDoc = JsonDocument.Parse(listJsonFileContent);
        var jsonData = jsonDoc.RootElement;
        
        if (jsonData.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ConfigFileElement>();
        }

        var elements = new List<ConfigFileElement>();
        
        foreach (var jsonElement in jsonData.EnumerateArray())
        {
            var element = JsonSerializer.Deserialize<ConfigFileElement>(
                jsonElement.GetRawText());
            
            // invalid configuration
            if (element == null || string.IsNullOrEmpty(element.ConfigFileRelativePath))
            {
                continue;
            }
            
            elements.Add(element);
        }

        elements.Sort();
        return elements;
    }

    public async Task WriteTableElements(IEnumerable<ConfigFileElement> elements)
    {
        var elementList = elements.ToList();
        elementList.Sort();
        
        var jsonString = JsonSerializer.Serialize(elementList, new JsonSerializerOptions 
        { 
            WriteIndented = true
        });
        
        await FileHelper.WriteAsync(m_ListJsonFilePath, jsonString);
        Log.Information("write list.json file '{Path}' finished", m_ListJsonFilePath);
    }
}