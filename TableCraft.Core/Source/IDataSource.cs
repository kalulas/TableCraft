#region File Header
// Filename: IDataSource.cs
// Author: Kalulas
// Create: 2023-04-08
// Description:
#endregion

using TableCraft.Core.ConfigElements;

namespace TableCraft.Core.Source;

/// <summary>
/// IDataSource provides fundamental information while creating <see cref="ConfigInfo"/>
/// </summary>
public interface IDataSource
{
    ConfigInfo Fill(ConfigInfo configInfo);
    string GetFilePath();
}