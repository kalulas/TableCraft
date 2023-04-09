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
using TableCraft.Core.Attributes;
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

    private static ConfigAttributeInfo FillConfigAttribute(ConfigAttributeInfo attributeInfo, int index,
        string attributeName, string comment)
    {
        attributeInfo.Index = index;
        attributeInfo.AttributeName = attributeName;
        attributeInfo.Comment = comment;
        return attributeInfo;
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

        configInfo.ConfigAttributeDict.Clear();
        for (int index = 0; index < headers.Length; index++)
        {
            var header = headers[index];
            var comment = ConfigManager.singleton.ReadComment && index < comments.Length
                ? comments[index]
                : string.Empty;

            var newAttributeInfo = FillConfigAttribute(new ConfigAttributeInfo(), index, header, comment);
            var success = configInfo.ConfigAttributeDict.TryAdd(header, newAttributeInfo);
            if (!success)
            {
                Debugger.LogWarning(
                    $"[CsvDataSource.Fill] Add attribute with '{header}' into '{configInfo.ConfigName}' failed!");
            }
        }

        return configInfo;
    }

    public string GetFilePath()
    {
        return m_FilePath;
    }

    #endregion
}