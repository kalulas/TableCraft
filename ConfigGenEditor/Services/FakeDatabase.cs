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
    public IEnumerable<ConfigFileElement> GetElements()
    {
        if (!File.Exists(m_ListJsonFilePath))
        {
            throw new FileNotFoundException(m_ListJsonFilePath + " not found");
        }
        
        var listJsonFileContent = File.ReadAllText(m_ListJsonFilePath, Encoding.UTF8);
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
}