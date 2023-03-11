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

    public string Comment => m_AttributeInfo.Comment;
    
    public string ValueType
    {
        get => m_AttributeInfo.ValueType;
        // TODO make it accessible 
        // set => m_AttributeInfo.ValueType = value;
    }

    public string CollectionType
    {
        get => m_AttributeInfo.CollectionType;
    }

    #endregion
    
    public ConfigAttributeDetailsViewModel(ConfigAttributeInfo attributeInfo)
    {
        m_AttributeInfo = attributeInfo;
    }
}