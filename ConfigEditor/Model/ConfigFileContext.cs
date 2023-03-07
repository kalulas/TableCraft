using System.IO;
using System.Text;
using LitJson;

namespace ConfigEditor.Model;

/// <summary>
/// Store all path information for user-created config files
/// </summary>
public static class ConfigFileContext
{
    private static ConfigFileElement[]? m_Elements;
    
    /// <summary>
    /// Called once, read relative directory information for all existed files
    /// </summary>
    /// <param name="listJsonFilePath"></param>
    /// <returns></returns>
    internal static bool Load(string listJsonFilePath)
    {
        var listJsonFileContent = File.ReadAllText(listJsonFilePath, Encoding.UTF8);
        var jsonData = JsonMapper.ToObject(listJsonFileContent);
        if (!jsonData.IsArray)
        {
            return false;
        }

        var arraySize = jsonData.Count;
        m_Elements = new ConfigFileElement[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            var element = JsonMapper.ToObject<ConfigFileElement>(jsonData[i].ToJson());
            m_Elements[i] = element;
        }

        return true;
    }
}