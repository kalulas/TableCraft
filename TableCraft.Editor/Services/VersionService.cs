using System;
using System.Reflection;

namespace TableCraft.Editor.Services;

public class VersionService : IVersionService
{
    public Version GetCurrentVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetName().Version;
        return version ?? new Version(1, 0, 0);
    }

    public bool IsNewerVersion(Version? current, Version? latest)
    {
        if (current == null || latest == null)
        {
            return false;
        }

        return latest.CompareTo(current) > 0;
    }

    public Version? ParseVersion(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return null;
        }

        // Remove 'v' prefix if present
        var cleanVersion = versionString.TrimStart('v', 'V');
        if (Version.TryParse(cleanVersion, out var version))
        {
            return version;
        }

        return null;
    }
}