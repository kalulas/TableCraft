using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConfigCodeGenLib.ConfigReader
{
    /// <summary>
    /// Comma-Separated Values file (.csv)
    /// </summary>
    public class CSVConfigInfo : ConfigInfo
    {
        public CSVConfigInfo(EConfigType configType, string configName, string configFilePath, string relatedJsonFilePath) : base(configType, configName, configFilePath, relatedJsonFilePath) { }

        protected override void ReadConfigFileAttributes()
        {
            var count = 0;
            string[] headers = new string[1];
            string[] comments = new string[1];
            foreach (var line in File.ReadAllLines(m_ConfigFilePath, Encoding.UTF8))
            {
                if (count++ >= 2)
                {
                    break;
                }

                var contentList = line.Split(',');
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

            m_Attribtues.Clear();
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                var comment = ConfigManager.singleton.ReadComment && i < comments.Length ? comments[i] : string.Empty;
                if (m_Attribtues.ContainsKey(header))
                {
                    Console.WriteLine("[CSVConfigInfo.ReadConfigFileAttributes] duplicate key \'{0}\', skip", header);
                    continue;
                }

                m_Attribtues.Add(header, new ConfigAttributeInfo().SetConfigFileInfo(i, header, comment));
            }
        }
    }
}
