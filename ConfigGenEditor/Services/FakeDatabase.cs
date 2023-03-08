using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConfigGenEditor.Models;
using LitJson;

namespace ConfigGenEditor.Services;

public class FakeDatabase
{
    private string m_ListJsonFilePath;

    public FakeDatabase(string listJsonFilePath)
    {
        m_ListJsonFilePath = listJsonFilePath;
    }
    
    /// <summary>
    /// Read relative directory information for all existed files
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ConfigFileElement> GetTableElements()
    {
        if (!File.Exists(m_ListJsonFilePath))
        {
            throw new FileNotFoundException(m_ListJsonFilePath + " not found");
        }

        var utf8NoBom = new UTF8Encoding(false);
        var listJsonFileContent = File.ReadAllText(m_ListJsonFilePath, utf8NoBom);
        var jsonData = JsonMapper.ToObject(listJsonFileContent);
        if (!jsonData.IsArray)
        {
            return Array.Empty<ConfigFileElement>();
        }

        var arraySize = jsonData.Count;
        var elements = new List<ConfigFileElement>();
        for (int i = 0; i < arraySize; i++)
        {
            var element = JsonMapper.ToObject<ConfigFileElement>(jsonData[i].ToJson());
            // invalid configuration
            if (string.IsNullOrEmpty(element.ConfigFileRelativePath))
            {
                continue;
            }
            
            elements.Add(element);
        }

        return elements;
    }

    public void WriteTableElements(IEnumerable<ConfigFileElement> elements)
    {
        var builder = new StringBuilder();
        var writer = new JsonWriter(builder)
        {
            PrettyPrint = true
        };

        JsonMapper.ToJson(elements, writer);
        var utf8NoBom = new UTF8Encoding(false);
        using var fs = File.Open(m_ListJsonFilePath, FileMode.OpenOrCreate);
        using var sw = new StreamWriter(fs, utf8NoBom);
        sw.Write(builder.ToString());
    }
}