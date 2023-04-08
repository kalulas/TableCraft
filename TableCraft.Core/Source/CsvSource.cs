#region File Header
// Filename: CsvDataSource.cs
// Author: Kalulas
// Create: 2023-04-08
// Description:
#endregion

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TableCraft.Core.ConfigReader;

namespace TableCraft.Core.Source;

[WithExtension("csv")]
public class CsvSource : IDataSource
{
    #region Fields

    private readonly string m_FilePath;
    private readonly Regex m_Pattern = new("\"([^\"]*)\"");

    #endregion

    #region Constructors

    public CsvSource(string filePath)
    {
        m_FilePath = filePath;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Dealing with the situation that the attribute(in csv file) contains a comma
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private string[] ReadLineAttributes(string line)
    {
        const string commaPlaceholder = "#comma#";
        var processedLine = m_Pattern.Replace(line, match => match.Value.Replace(",", commaPlaceholder)); 
        var attributes = processedLine.Split(',');
        for (int i = 0; i < attributes.Length; i++)
        {
            if (attributes[i].StartsWith("\""))
            {
                // remove leading and tailing quote
                var result = attributes[i].Remove(0, 1);
                result = result.Remove(result.Length - 1, 1);
                result = result.Replace(commaPlaceholder, ",");
                // replace inner quote
                attributes[i] = result.Replace("\"\"", "\"");
            }

        }

        return attributes;
    }

    #endregion

    #region Public API

    public ConfigInfo Fill(ConfigInfo configInfo)
    {
        if (!File.Exists(m_FilePath))
        {
            Debugger.LogError($"[CsvDataSource.Fill] '{m_FilePath}' not found!");
            return null;
        }
            
        var count = 0;
        var headers = Array.Empty<string>();
        var comments = Array.Empty<string>();
        var encoding = new UTF8Encoding(Configuration.UseUTF8WithBOM);
        foreach (var line in File.ReadAllLines(m_FilePath, encoding))
        {
            if (count++ >= 2)
            {
                break;
            }

            var contentList = ReadLineAttributes(line);
            if (count == 1)
            {
                headers = contentList;
                continue;
            }

            if (count == 2 && ConfigManager.singleton.ReadComment)
            {
                comments = contentList;
                continue;
            }
        }

        configInfo.ClearAttributes();
        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i];
            var comment = ConfigManager.singleton.ReadComment && i < comments.Length 
                ? comments[i] : string.Empty;

            var newAttributeInfo = new ConfigAttributeInfo().SetConfigFileInfo(i, header, comment);
            var success = configInfo.TryAddAttribute(header, newAttributeInfo);
            if (!success)
            {
                Debugger.LogWarning($"[CsvDataSource.Fill] Add attribute with '{header}' into '{configInfo.ConfigName}' failed!");
            }
        }
        
        return configInfo;
    }

    #endregion
}