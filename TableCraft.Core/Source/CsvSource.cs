#region File Header

// Filename: CsvDataSource.cs
// Author: Kalulas
// Create: 2023-04-08
// Description:

#endregion

using System;
using System.IO;
using System.Text.RegularExpressions;
using TableCraft.Core.Attributes;
using TableCraft.Core.ConfigElements;
using TableCraft.Core.IO;

namespace TableCraft.Core.Source;

[WithExtension("csv")]
public class CsvSource : IDataSource
{
    private class Configuration
    {
        /// <summary>
        /// Read Attribute name from header line, this is a must-have
        /// </summary>
        public int HeaderLineIndex;
        /// <summary>
        /// -1 if no comment line
        /// </summary>
        public int CommentLineIndex = -1;

        public Configuration Fix()
        {
            if (HeaderLineIndex == CommentLineIndex)
            {
                HeaderLineIndex = -1;
                CommentLineIndex = -1;
                Debugger.LogError("[Configuration.Fix] HeaderLineIndex and CommentLineIndex can't be the same, all set to -1");
            }

            return this;
        }
    }
    
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

        var curLineIndex = 0;
        var headers = Array.Empty<string>();
        var comments = Array.Empty<string>();
        
        var dataSourceConfig = Core.Configuration.GetDataSourceConfiguration<Configuration>(typeof(CsvSource)).Fix();
        var maxLineIndex = Math.Max(dataSourceConfig.HeaderLineIndex, dataSourceConfig.CommentLineIndex);
        // When you are working with very large files, ReadLines can be more efficient.
        // (https://learn.microsoft.com/en-us/dotnet/api/System.IO.File.ReadLines?view=net-6.0)
        foreach(var line in FileHelper.ReadLines(m_FilePath))
        {
            if (curLineIndex > maxLineIndex)
            {
                break;
            }

            var contentList = ReadLineAttributes(line);
            if (curLineIndex == dataSourceConfig.HeaderLineIndex)
            {
                headers = contentList;
            }

            if (curLineIndex == dataSourceConfig.CommentLineIndex)
            {
                comments = contentList;
            }

            curLineIndex++;
        }

        if (dataSourceConfig.CommentLineIndex != -1 && headers.Length != comments.Length)
        {
            Debugger.LogWarning(
                $"[CsvDataSource.Fill] Header count '{headers.Length}' not equal to comment count '{comments.Length}'");
        }
        
        configInfo.ConfigAttributeDict.Clear();
        for (int index = 0; index < headers.Length; index++)
        {
            var header = headers[index];
            var comment = index < comments.Length ? comments[index] : string.Empty;

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