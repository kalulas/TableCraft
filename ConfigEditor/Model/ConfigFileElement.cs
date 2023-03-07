using Path = System.IO.Path;

namespace ConfigEditor.Model;

/// <summary>
/// <para> File information for an existed config file(data source) </para>
/// <para> For LitJson reflection used </para>
/// </summary>
public class ConfigFileElement
{
    // CamelCase for LitJson reflection
    public readonly string ConfigFileRelativePath;
    public readonly string JsonFileRelativePath;

    public string ConfigFilePath => Path.Combine(Program.GetConfigHomePath(), ConfigFileRelativePath);

    public string JsonFilePath => Path.Combine(Program.GetJsonHomePath(), JsonFileRelativePath);

    public ConfigFileElement()
    {
        ConfigFileRelativePath = string.Empty;
        JsonFileRelativePath = string.Empty;
    }
}