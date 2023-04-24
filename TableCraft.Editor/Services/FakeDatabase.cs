using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using Serilog;
using TableCraft.Core.IO;
using TableCraft.Editor.Models;

namespace TableCraft.Editor.Services;

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
    public IEnumerable<ConfigFileElement> ReadTableElements()
    {
        if (!File.Exists(m_ListJsonFilePath))
        {
            throw new FileNotFoundException(m_ListJsonFilePath + " not found");
        }

        var listJsonFileContent = FileHelper.ReadAllText(m_ListJsonFilePath);
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

        elements.Sort();
        return elements;
    }

    public async Task WriteTableElements(IEnumerable<ConfigFileElement> elements)
    {
        var builder = new StringBuilder();
        var writer = new JsonWriter(builder)
        {
            PrettyPrint = true
        };

        var elementList = elements.ToList();
        elementList.Sort();
        
        JsonMapper.ToJson(elementList, writer);
        await FileHelper.WriteAsync(m_ListJsonFilePath, builder.ToString());
        Log.Information("write ListJsonFile {Path} finished", m_ListJsonFilePath);
    }
}