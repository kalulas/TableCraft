#region File Header
// Filename: ConfigAttributeDetailsViewModel.cs
// Author: Kalulas
// Create: 2023-03-11
// Description:
#endregion

using System.Collections.ObjectModel;
using System.Windows.Input;
using TableCraft.Core.ConfigElements;
using ReactiveUI;

namespace TableCraft.Editor.ViewModels;

/// <summary>
/// Another viewmodel wrapper of <see cref="TableCraft.Core.ConfigElements.ConfigAttributeInfo"/>, for attribute editor panel
/// </summary>
public class ConfigAttributeDetailsViewModel : ViewModelBase
{
    #region Fields

    private readonly ConfigAttributeInfo m_AttributeInfo;
    private string m_CollectionType;
    private string m_SelectedUsageType = string.Empty;
    private string m_SelectedAttributeTag = string.Empty;

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
        set
        {
            var previousValueType = m_AttributeInfo.ValueType;
            m_AttributeInfo.ValueType = value;
            OnValueTypeChanged(previousValueType, value);
        }
    }

    public string CollectionType
    {
        get => m_AttributeInfo.CollectionType;
        set
        {
            m_AttributeInfo.CollectionType = value;
            this.RaiseAndSetIfChanged(ref m_CollectionType, value);
        }
    }

    public string DefaultValue
    {
        get => m_AttributeInfo.DefaultValue;
        set => m_AttributeInfo.DefaultValue = value;
    }

    public string SelectedUsageType
    {
        get => m_SelectedUsageType;
        set => this.RaiseAndSetIfChanged(ref m_SelectedUsageType, value);
    }

    public string SelectedAttributeTag
    {
        get => m_SelectedAttributeTag;
        set => this.RaiseAndSetIfChanged(ref m_SelectedAttributeTag, value);
    }

    public ICommand AddUsageCommand { get; }
    
    public ICommand AddTagCommand { get; }
    
    public ObservableCollection<ConfigAttributeUsageViewModel> Usages { get; }
    
    public ObservableCollection<ConfigAttributeTagViewModel> Tags { get; }

    #endregion

    #region Constructors

    public ConfigAttributeDetailsViewModel(ConfigAttributeInfo attributeInfo)
    {
        m_AttributeInfo = attributeInfo;
        m_CollectionType = m_AttributeInfo.CollectionType;
        Usages = new ObservableCollection<ConfigAttributeUsageViewModel>();
        foreach (var usageInfo in m_AttributeInfo.AttributeUsageInfos)
        {
            Usages.Add(new ConfigAttributeUsageViewModel(usageInfo, this));
        }

        Tags = new ObservableCollection<ConfigAttributeTagViewModel>();
        foreach (var tag in m_AttributeInfo.Tags)
        {
            Tags.Add(new ConfigAttributeTagViewModel(tag, this));
        }
        
        AddUsageCommand = ReactiveCommand.Create(OnAddUsageBtnClicked);
        AddTagCommand = ReactiveCommand.Create(OnAddTagBtnClicked);
    }

    #endregion

    #region Interaction

    private void OnAddUsageBtnClicked()
    {
        foreach (var usage in Usages)
        {
            if (usage.Usage == m_SelectedUsageType)
            {
                // duplicated, do nothing
                return;
            }
        }

        var newUsageInfo = new ConfigAttributeUsageInfo
        {
            Usage = m_SelectedUsageType,
            FieldName = m_AttributeInfo.AttributeName
        };

        if (!m_AttributeInfo.AddUsageInfo(newUsageInfo))
        {
            return;
        }

        Usages.Add(new ConfigAttributeUsageViewModel(newUsageInfo, this));
    }

    private void OnAddTagBtnClicked()
    {
        foreach (var tag in Tags)
        {
            if (tag.Content == m_SelectedAttributeTag)
            {
                // duplicated, do nothing
                return;
            }
        }
        
        if (!m_AttributeInfo.AddTag(m_SelectedAttributeTag))
        {
            return;
        }
        
        Tags.Add(new ConfigAttributeTagViewModel(m_SelectedAttributeTag, this));
    }

    #endregion

    #region Private Methods

    private void OnValueTypeChanged(string previousValueType, string newValueType)
    {
        if (string.Equals(previousValueType, newValueType))
        {
            return;
        }

        // if already set, ignore
        if (!string.IsNullOrEmpty(m_CollectionType))
        {
            return;
        }

        var availableCollectionType = Core.Configuration.DataCollectionType;
        if (availableCollectionType.Length == 0)
        {
            return;
        }
        
        // auto select the first collection type for user
        CollectionType = availableCollectionType[0];
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
    
    public void OnTagRemoved(ConfigAttributeTagViewModel tagViewModel)
    {
        if (!m_AttributeInfo.RemoveTag(tagViewModel.Content))
        {
            return;
        }

        Tags.Remove(tagViewModel);
    }

    #endregion
}