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
        // before read
        await using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(fs, PreferredEncoding);
        var result = await sr.ReadToEndAsync();
        // after read
        return result;
    }
    
    public static string ReadAllText(string filePath)
    {
        // before read
        var content = File.ReadAllText(filePath, PreferredEncoding);
        // after read
        return content;
    }

    public static IEnumerable<string> ReadLines(string filePath)
    {
        // before read
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
        // before write
        await using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
        await using var sw = new StreamWriter(fs, PreferredEncoding);
        await sw.WriteAsync(content);
        // after write
    }
    
    /// <summary>
    /// Open file and write all with <see cref="System.IO.FileMode.Create"/> and <see cref="System.IO.FileAccess.Write"/>
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    public static void Write(string filePath, string content)
    {
        // before write
        using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
        using var sw = new StreamWriter(fs, PreferredEncoding);
        sw.Write(content);
        // after write
    }

    #endregion

}