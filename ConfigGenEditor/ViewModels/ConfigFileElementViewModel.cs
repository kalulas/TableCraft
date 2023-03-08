using ConfigGenEditor.Models;
using System.IO;

namespace ConfigGenEditor.ViewModels;

public class ConfigFileElementViewModel : ViewModelBase
{
    private readonly ConfigFileElement m_Element;

    public bool IsJsonDescriptionFound => File.Exists(JsonFilePath);

    public string ConfigFileRelativePath => m_Element.ConfigFileRelativePath;
    
    public string ConfigFilePath => m_Element.GetConfigFileFullPath();

    public string JsonFilePath => m_Element.GetJsonFileFullPath();

    public ConfigFileElementViewModel(ConfigFileElement element)
    {
        m_Element = element;
    }

    public ConfigFileElement GetElement()
    {
        return m_Element;
    }
}