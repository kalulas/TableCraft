#region File Header
// Filename: Perforce.cs
// Author: Kalulas
// Create: 2023-04-16
// Description:
#endregion

using TableCraft.Core.IO;

namespace TableCraft.Core.VersionControl;

public class Perforce : IFileEvent
{
    public const string Label = "Perforce";

    public string GetLabel()
    {
        return Label;
    }

    public void BeforeRead(string filePath)
    {
        // Debugger.Log($"[Perforce.BeforeRead] {filePath}");
    }

    public void AfterRead(string filePath)
    {
        // Debugger.Log($"[Perforce.AfterRead] {filePath}");
    }

    public void BeforeWrite(string filePath)
    {
        // Debugger.Log($"[Perforce.BeforeWrite] {filePath}");
    }

    public void AfterWrite(string filePath)
    {
        // Debugger.Log($"[Perforce.AfterWrite] {filePath}");
    }
}