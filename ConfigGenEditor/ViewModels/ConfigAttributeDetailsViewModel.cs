#region File Header
// Filename: ConfigAttributeDetailsViewModel.cs
// Author: Kalulas
// Create: 2023-03-11
// Description:
#endregion

using System.Collections.ObjectModel;
using System.Windows.Input;
using ConfigCodeGenLib.ConfigReader;
using ReactiveUI;

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

    public string SelectedUsageType { get; set; } = string.Empty;
    
    public ICommand AddUsageCommand { get; private set; }
    
    public ObservableCollection<ConfigAttributeUsageViewModel> Usages { get; }

    #endregion

    #region Constructors

    public ConfigAttributeDetailsViewModel(ConfigAttributeInfo attributeInfo)
    {
        m_AttributeInfo = attributeInfo;
        Usages = new ObservableCollection<ConfigAttributeUsageViewModel>();
        foreach (var usageInfo in m_AttributeInfo.AttributeUsageInfos)
        {
            Usages.Add(new ConfigAttributeUsageViewModel(usageInfo, this));
        }
        
        AddUsageCommand = ReactiveCommand.Create(OnAddUsageBtnClicked);
    }

    #endregion

    #region Interaction

    private void OnAddUsageBtnClicked()
    {
        foreach (var usage in Usages)
        {
            if (usage.Usage == SelectedUsageType)
            {
                // duplicated, do nothing
                return;
            }
        }

        var newUsageInfo = new ConfigAttributeUsageInfo
        {
            Usage = SelectedUsageType,
            FieldName = m_AttributeInfo.AttributeName
        };

        if (!m_AttributeInfo.AddUsageInfo(newUsageInfo))
        {
            return;
        }
        
        var newUsage = new ConfigAttributeUsageViewModel(newUsageInfo, this);
        Usages.Add(newUsage);
    }

    #endregion

    #region Public API

    public void OnUsageRemoved(ConfigAttributeUsageViewModel usageViewModel)
    {
        if (!m_AttributeInfo.RemoveUsageInfo(usageViewModel.Usage))
        {
            return;
        }

        Usages.Remove(usageViewModel);
    }

    #endregion
}