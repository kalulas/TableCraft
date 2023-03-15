#region File Header
// Filename: ConfigAttributeDetailsViewModel.cs
// Author: Kalulas
// Create: 2023-03-11
// Description:
#endregion

using ConfigCodeGenLib.ConfigReader;

namespace ConfigGenEditor.ViewModels;

/// <summary>
/// Another viewmodel wrapper of <see cref="ConfigCodeGenLib.ConfigReader.ConfigAttributeInfo"/>, for attribute editor panel
/// </summary>
public class ConfigAttributeDetailsViewModel : ViewModelBase
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

    public string ValueType
    {
        get => m_AttributeInfo.ValueType;
        set => m_AttributeInfo.ValueType = value;
    }

    public string CollectionType
    {
        get => m_AttributeInfo.CollectionType;
        set => m_AttributeInfo.CollectionType = value;
    }

    public string DefaultValue
    {
        get => m_AttributeInfo.DefaultValue;
        set => m_AttributeInfo.DefaultValue = value;
    }

    #endregion
    
    public ConfigAttributeDetailsViewModel(ConfigAttributeInfo attributeInfo)
    {
        m_AttributeInfo = attributeInfo;
    }
}