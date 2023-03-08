using System;
using Path = System.IO.Path;

namespace ConfigGenEditor.Models;

/// <summary>
/// <para> File information for an existed config file(data source) </para>
/// <para> For LitJson reflection used </para>
/// </summary>
public class ConfigFileElement : IComparable<ConfigFileElement>
{
    // CamelCase for LitJson reflection
    public readonly string ConfigFileRelativePath;
    public readonly string JsonFileRelativePath;

    public ConfigFileElement()
    {
        ConfigFileRelativePath = string.Empty;
        JsonFileRelativePath = string.Empty;
    }

    public ConfigFileElement(string configFileRelativePath)
    {
        ConfigFileRelativePath = configFileRelativePath;
        JsonFileRelativePath = string.Empty;
    }

    public string GetConfigFileFullPath()
    {
        return Path.Combine(Program.GetConfigHomePath(), ConfigFileRelativePath);
    }

    public string GetJsonFileFullPath()
    {
        if (string.IsNullOrEmpty(JsonFileRelativePath))
        {
            return string.Empty;
        }

        return Path.Combine(Program.GetJsonHomePath(), JsonFileRelativePath);
    }

    public int CompareTo(ConfigFileElement? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return string.Compare(ConfigFileRelativePath, other.ConfigFileRelativePath, StringComparison.Ordinal);
    }
}