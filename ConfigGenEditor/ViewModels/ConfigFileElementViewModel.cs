using ConfigGenEditor.Models;
using System.IO;

namespace ConfigGenEditor.ViewModels;

public class ConfigFileElementViewModel : ViewModelBase
{
    private readonly ConfigFileElement m_Element;

    public string Identifier => m_Element.Identifier;
    
    public string ConfigFilePath => m_Element.ConfigFilePath;

    public string JsonFilePath => m_Element.JsonFilePath;

    public ConfigFileElementViewModel(ConfigFileElement element)
    {
        m_Element = element;
    }
}