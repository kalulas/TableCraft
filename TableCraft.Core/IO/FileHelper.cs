#region File Header
// Filename: FileHelper.cs
// Author: Kalulas
// Create: 2023-04-16
// Description:
#endregion

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TableCraft.Core.IO;

/// <summary>
/// <para> FileHelper provides you some with useful read/write methods. These methods use the encoding in configuration,
/// work with some extra file processors(such as version control) that you can registered. </para>
/// 
/// <para> FileHelper is based on <see cref="System.IO.File"/>,
/// <see cref="System.IO.StreamReader"/> and <see cref="System.IO.StreamWriter"/>. </para>
/// </summary>
public static class FileHelper
{
    #region Fields

    private static readonly List<IFileEvent> FileEvents = new();

    #endregion
    
    #region Properties

    private static Encoding PreferredEncoding => new UTF8Encoding(Configuration.UseUTF8WithBOM);

    #endregion
    
    #region Public API

    /// <summary>
    /// Open file and read to end with <see cref="System.IO.FileMode.Open"/> and <see cref="System.IO.FileAccess.Read"/>
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<string> ReadToEnd(string filePath)
    {
        foreach (var fileEvent in FileEvents) fileEvent.BeforeRead(filePath);

        await using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(fs, PreferredEncoding);
        var result = await sr.ReadToEndAsync();
        
        foreach (var fileEvent in FileEvents) fileEvent.AfterRead(filePath);
        
        return result;
    }
    
    public static string ReadAllText(string filePath)
    {
        foreach (var fileEvent in FileEvents) fileEvent.BeforeRead(filePath);
        
        var content = File.ReadAllText(filePath, PreferredEncoding);
        
        foreach (var fileEvent in FileEvents) fileEvent.AfterRead(filePath);
        
        return content;
    }

    public static IEnumerable<string> ReadLines(string filePath)
    {
        foreach (var fileEvent in FileEvents) fileEvent.BeforeRead(filePath);
        
        var enumerator = File.ReadLines(filePath, PreferredEncoding);
        return enumerator;
    }

    /// <summary>
    /// Open file and write all with <see cref="System.IO.FileMode.Create"/> and <see cref="System.IO.FileAccess.Write"/>
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    public static async Task WriteAsync(string filePath, string content)
    {
        foreach (var fileEvent in FileEvents) fileEvent.BeforeWrite(filePath);
        
        await using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
        await using var sw = new StreamWriter(fs, PreferredEncoding);
        await sw.WriteAsync(content);
        
        foreach (var fileEvent in FileEvents) fileEvent.AfterWrite(filePath);
    }
    
    /// <summary>
    /// Open file and write all with <see cref="System.IO.FileMode.Create"/> and <see cref="System.IO.FileAccess.Write"/>
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    public static void Write(string filePath, string content)
    {
        foreach (var fileEvent in FileEvents) fileEvent.BeforeWrite(filePath);
        
        using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
        using var sw = new StreamWriter(fs, PreferredEncoding);
        sw.Write(content);
        
        foreach (var fileEvent in FileEvents) fileEvent.AfterWrite(filePath);
    }

    public static void RegisterFileEvent(IFileEvent fileEvent)
    {
        if (fileEvent == null)
        {
            return;
        }
        
        FileEvents.Add(fileEvent);
        fileEvent.OnRegistered();
    }

    public static bool UnregisterFileEvent(string label)
    {
        var removed = FileEvents.FindAll(fileEvent => fileEvent.GetLabel() == label);
        foreach (var fileEvent in removed)
        {
            fileEvent.OnUnregistered();
        }
        
        return removed.Count != 0;
    }
    
    #endregion

}