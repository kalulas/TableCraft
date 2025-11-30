using System;
using System.Collections.Generic;
using System.IO;

namespace TableCraft.Editor.Models;

/// <summary>
/// Strongly typed configuration model for appsettings.json
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Fallback export path when specific usage is not configured
    /// </summary>
    public static string FallbackCodeExportPath => Path.Combine(AppContext.BaseDirectory, "GeneratedCode");

    /// <summary>
    /// If true, the application will check for updates on startup
    /// </summary>
    public bool CheckForUpdates { get; set; } = true;

    /// <summary>
    /// Common root directory of configuration files for reading configuration files
    /// </summary>
    public string ConfigHomePath { get; set; } = string.Empty;

    /// <summary>
    /// Common root directory of configuration description files for saving description files
    /// </summary>
    public string JsonHomePath { get; set; } = string.Empty;

    /// <summary>
    /// Export directories for different usage types
    /// </summary>
    public Dictionary<string, string> ExportPath { get; set; } = new();

    /// <summary>
    /// Try get the export path for code generation, if not found, use fallback path
    /// </summary>
    /// <param name="usage">The usage type to get export path for</param>
    /// <returns>Export path for the usage, or fallback path if not configured</returns>
    public string GetCodeExportPath(string usage)
    {
        if (ExportPath.TryGetValue(usage, out var exportPath))
        {
            return exportPath;
        }

        return FallbackCodeExportPath;
    }
}