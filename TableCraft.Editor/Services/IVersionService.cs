using System;

namespace TableCraft.Editor.Services;

public interface IVersionService
{
    Version GetCurrentVersion();
    bool IsNewerVersion(Version current, Version latest);
    Version? ParseVersion(string versionString);
}