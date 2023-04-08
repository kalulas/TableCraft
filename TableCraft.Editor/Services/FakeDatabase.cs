using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Logging;
using LitJson;
using Serilog;
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

        var encoding = new UTF8Encoding(Core.Configuration.UseUTF8WithBOM);
        var listJsonFileContent = File.ReadAllText(m_ListJsonFilePath, encoding);
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
        var encoding = new UTF8Encoding(Core.Configuration.UseUTF8WithBOM);
        using var fs = File.Open(m_ListJsonFilePath, FileMode.Create);
        using var sw = new StreamWriter(fs, encoding);
        await sw.WriteAsync(builder.ToString());
        Log.Information("write ListJsonFile {Path} finished", m_ListJsonFilePath);
    }
}