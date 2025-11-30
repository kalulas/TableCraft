using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using TableCraft.Core.IO;
using TableCraft.Core.VersionControl;

namespace TableCraft.Editor.Services;

/// <summary>
/// Manages Perforce configuration stored in user directory instead of appsettings.json
/// </summary>
public class P4ConfigManager : IP4ConfigManager
{
    private static readonly string m_P4ConfigFilePath = Path.Combine(Program.AppDataDirectory, m_P4ConfigFileName);
    private const string m_P4ConfigFileName = "p4config.json";

    /// <summary>
    /// Gets the full path to the P4 configuration file
    /// </summary>
    /// <returns>Full path to p4config.json</returns>
    public string GetConfigFilePath()
    {
        return m_P4ConfigFilePath;
    }

    /// <summary>
    /// Checks if P4 configuration exists
    /// </summary>
    /// <returns>True if p4config.json exists</returns>
    public bool ConfigExists()
    {
        return File.Exists(m_P4ConfigFilePath);
    }

    /// <summary>
    /// Gets the Perforce configuration from user directory
    /// </summary>
    /// <returns>PerforceUserConfig object, empty if not found or invalid</returns>
    public PerforceUserConfig GetVersionControlConfig()
    {
        var emptyConfig = new PerforceUserConfig();

        if (!File.Exists(m_P4ConfigFilePath))
        {
            Log.Information("P4 config file not found: {FilePath}", m_P4ConfigFilePath);
            return emptyConfig;
        }

        try
        {
            var configContent = FileHelper.ReadAllText(m_P4ConfigFilePath);
            if (string.IsNullOrEmpty(configContent))
            {
                Log.Warning("P4 config file is empty: {FilePath}", m_P4ConfigFilePath);
                return emptyConfig;
            }

            var config = JsonSerializer.Deserialize<PerforceUserConfig>(configContent);
            if (config == null || string.IsNullOrEmpty(config.P4PORT))
            {
                Log.Warning("Invalid P4 configuration in file: {FilePath}", m_P4ConfigFilePath);
                return emptyConfig;
            }

            config.Decode();
            
            Log.Information("Successfully loaded P4 config from: {FilePath}", m_P4ConfigFilePath);
            return config;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error reading P4 config: {FilePath}",
                m_P4ConfigFilePath);
            return emptyConfig;
        }
    }

    /// <summary>
    /// Saves the Perforce configuration to user directory
    /// </summary>
    /// <param name="config">The PerforceUserConfig to save</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> UpdateVersionControlConfig(PerforceUserConfig config)
    {
        config.Encode(); // ensure password is encoded

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var configJson = JsonSerializer.Serialize(config, options);
            await FileHelper.WriteAsync(m_P4ConfigFilePath, configJson);

            Log.Information("Successfully saved P4 config to: {FilePath}", m_P4ConfigFilePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error saving P4 config: {FilePath}", m_P4ConfigFilePath);
            return false;
        }
    }
}