#region File Header
// Filename: JsonDecorator.cs
// Author: Kalulas
// Create: 2023-04-09
// Description:
#endregion

using TableCraft.Core.ConfigReader;

namespace TableCraft.Core.Decorator;

[WithExtension("json")]
public class JsonDecorator : IDataDecorator
{
    #region Fields

    private readonly string m_Filepath;

    #endregion

    #region Constructors

    public JsonDecorator(string filepath)
    {
        m_Filepath = filepath;
    }

    #endregion

    #region Private Methods

    

    #endregion

    #region Public API

    public ConfigInfo Decorate(ConfigInfo configInfo)
    {
        return configInfo;
    }

    #endregion
}