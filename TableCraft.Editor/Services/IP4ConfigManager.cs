using System.Threading.Tasks;
using TableCraft.Core.VersionControl;

namespace TableCraft.Editor.Services;

/// <summary>
/// Interface for managing Perforce configuration stored in user directory
/// </summary>
public interface IP4ConfigManager
{
    /// <summary>
    /// Gets the full path to the P4 configuration file
    /// </summary>
    /// <returns>Full path to p4config.json</returns>
    string GetConfigFilePath();

    /// <summary>
    /// Checks if P4 configuration exists
    /// </summary>
    /// <returns>True if p4config.json exists</returns>
    bool ConfigExists();

    /// <summary>
    /// Gets the Perforce configuration from user directory
    /// </summary>
    /// <returns>PerforceUserConfig object, empty if not found or invalid</returns>
    PerforceUserConfig GetVersionControlConfig();

    /// <summary>
    /// Saves the Perforce configuration to user directory
    /// </summary>
    /// <param name="config">The PerforceUserConfig to save</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateVersionControlConfig(PerforceUserConfig config);
}