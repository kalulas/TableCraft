using System;
using ConfigGenEditor.Models;
using System.IO;
using ConfigCodeGenLib.ConfigReader;

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

    public EConfigType GetConfigType()
    {
        var extension = Path.GetExtension(m_Element.ConfigFileRelativePath);
        switch (extension)
        {
            case ".csv":
                return EConfigType.CSV;
            default:
                throw new Exception($"Unknown config type with path:{m_Element.ConfigFileRelativePath}");
        }
    }
}