#region File Header
// Filename: PathExtend.cs
// Author: Kalulas
// Create: 2023-03-26
// Description:
#endregion

using System;
using System.IO;

namespace ConfigGenEditor.Services;

public static class PathExtend
{
    // This is from https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
    // For in .NET Framework 4.8, we don't have a Path.GetRelativePath
    /// <summary>
    /// Creates a relative path from one file or folder to another.
    /// </summary>
    /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
    /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
    /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="UriFormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static string MakeRelativePath(string fromPath, string toPath)
    {
        if (string.IsNullOrEmpty(fromPath))
        {
            throw new ArgumentNullException(nameof(fromPath));
        }

        if (string.IsNullOrEmpty(toPath))
        {
            throw new ArgumentNullException(nameof(toPath));
        }

        var fromUri = new Uri(fromPath);
        var toUri = new Uri(toPath);

        if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
        {
            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        return relativePath;
    }
}