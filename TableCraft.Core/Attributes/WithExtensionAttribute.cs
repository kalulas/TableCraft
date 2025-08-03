#region File Header

// Filename: DataSourceExtensionAttribute.cs
// Author: Kalulas
// Create: 2023-04-08
// Description:

#endregion

using System;

namespace TableCraft.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class WithExtensionAttribute : Attribute
{
    public string Extension { get; private set; }
    public WithExtensionAttribute(string extension)
    {
        Extension = extension;
    }
}