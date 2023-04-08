#region File Header
// Filename: ConfigInfoAttributeViewModel.cs
// Author: Kalulas
// Create: 2023-03-11
// Description:
#endregion

using TableCraft.Core.ConfigReader;

namespace TableCraft.Editor.ViewModels;

/// <summary>
/// A viewmodel wrapper of <see cref="TableCraft.Core.ConfigReader.ConfigAttributeInfo"/>
/// </summary>
public class ConfigAttributeListItemViewModel : ViewModelBase
{
    #region Fields

    private readonly ConfigAttributeInfo m_AttributeInfo;

    #endregion

    #region Properties

    public string Name => m_AttributeInfo.AttributeName;

    public string Comment
    {
        get => m_AttributeInfo.Comment;
        set => m_AttributeInfo.Comment = value;
    }

    #endregion
    
    public ConfigAttributeListItemViewModel(ConfigAttributeInfo attributeInfo)
    {
        m_AttributeInfo = attributeInfo;
    }

    #region Public API

    public ConfigAttributeInfo GetAttributeInfo()
    {
        return m_AttributeInfo;
    }

    #endregion
}