using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TableCraft.Core.ConfigReader
{
    /// <summary>
    /// Comma-Separated Values file (.csv)
    /// </summary>
    public class CSVConfigInfo : ConfigInfo
    {
        private readonly Regex m_Pattern = new Regex("\"([^\"]*)\"");
        
        public CSVConfigInfo(EConfigType configType, string configName, string configFilePath, string relatedJsonFilePath) : base(configType, configName, configFilePath, relatedJsonFilePath) { }

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

        public override ConfigInfo ReadConfigFileAttributes()
        {
            if (!File.Exists(m_ConfigFilePath))
            {
                Debugger.LogError($"[ConfigInfo.ReadConfigFileAttributes] '{m_ConfigFilePath}' not found!");
                return null;
            }
            
            var count = 0;
            string[] headers = new string[1];
            string[] comments = new string[1];
            var encoding = new UTF8Encoding(Configuration.UseUTF8WithBOM);
            foreach (var line in File.ReadAllLines(m_ConfigFilePath, encoding))
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

            ConfigAttributeDict.Clear();
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                var comment = ConfigManager.singleton.ReadComment && i < comments.Length 
                    ? comments[i] : string.Empty;
                if (ConfigAttributeDict.ContainsKey(header))
                {
                    Debugger.Log("[CSVConfigInfo.ReadConfigFileAttributes] duplicate key \'{0}\', skip", header);
                    continue;
                }

                ConfigAttributeDict.Add(header, new ConfigAttributeInfo().SetConfigFileInfo(i, header, comment));
            }

            return this;
        }
    }
}
