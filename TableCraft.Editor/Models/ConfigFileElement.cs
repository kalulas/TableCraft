using System;
using Microsoft.Extensions.DependencyInjection;
using Path = System.IO.Path;

namespace TableCraft.Editor.Models;

/// <summary>
/// <para> File information for an existed config file(data source) </para>
/// <para> Compatible with System.Text.Json serialization </para>
/// </summary>
public class ConfigFileElement : IComparable<ConfigFileElement>
{
    public string ConfigFileRelativePath { get; init; }
    public string JsonFileRelativePath { get; set; }

    public ConfigFileElement(string configFileRelativePath, string jsonFileRelativePath)
    {
        ConfigFileRelativePath = configFileRelativePath;
        JsonFileRelativePath = jsonFileRelativePath;
    }

    public string GetConfigFileFullPath()
    {
        var appSettings = Program.Host.Services.GetRequiredService<AppSettings>();
        return Path.Combine(appSettings.ConfigHomePath, ConfigFileRelativePath);
    }

    public string GetJsonFileFullPath()
    {
        if (string.IsNullOrEmpty(JsonFileRelativePath))
        {
            return string.Empty;
        }

        var appSettings = Program.Host.Services.GetRequiredService<AppSettings>();
        return Path.Combine(appSettings.JsonHomePath, JsonFileRelativePath);
    }

    public int CompareTo(ConfigFileElement? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return string.Compare(ConfigFileRelativePath, other.ConfigFileRelativePath, StringComparison.Ordinal);
    }

    public void SetJsonFileRelativePath(string relativePath)
    {
        JsonFileRelativePath = relativePath;
    }
}