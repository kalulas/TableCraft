#region File Header
// Filename: IDataDecorator.cs
// Author: Kalulas
// Create: 2023-04-08
// Description:
#endregion

using TableCraft.Core.ConfigElements;

namespace TableCraft.Core.Decorator;

/// <summary>
/// IDataDecorator provides additional information while creating <see cref="ConfigInfo"/>
/// </summary>
public interface IDataDecorator
{
    ConfigInfo Decorate(ConfigInfo configInfo);
    bool SaveToFile(ConfigInfo configInfo);
    string GetFilePath();
}