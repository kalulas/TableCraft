#region File Header
// Filename: ConfigInfoViewModel.cs
// Author: Kalulas
// Create: 2023-03-11
// Description: 
#endregion

using System.Collections.ObjectModel;
using System.IO;
using TableCraft.Core;
using TableCraft.Core.ConfigReader;
using ReactiveUI;

namespace TableCraft.Editor.ViewModels;

/// <summary>
/// A viewmodel wrapper of <see cref="TableCraft.Core.ConfigReader.ConfigInfo"/>
/// </summary>
public class ConfigInfoViewModel : ViewModelBase
{
    #region Fields

    private readonly ConfigInfo m_ConfigInfo;
    private string m_SelectedUsage;
    private ConfigUsageInfo? m_PreviewConfigConfigUsageInfo;
    private readonly ObservableCollection<ConfigAttributeListItemViewModel> m_Attributes;

    #endregion

    #region Properties

    public ObservableCollection<ConfigAttributeListItemViewModel> Attributes => m_Attributes;

    public string ConfigName => m_ConfigInfo.ConfigName;

    public string PreviewInfoUsage
    {
        get => m_SelectedUsage;
        set
        {
            m_SelectedUsage = value;
            OnPreviewUsageChanged();
        }
    }

    public ConfigUsageInfo? PreviewConfigUsageInfo
    {
        get => m_PreviewConfigConfigUsageInfo;
        set => this.RaiseAndSetIfChanged(ref m_PreviewConfigConfigUsageInfo, value);
    }

    #endregion

    public ConfigInfoViewModel(ConfigInfo configInfo)
    {
        m_ConfigInfo = configInfo;
        m_Attributes = new ObservableCollection<ConfigAttributeListItemViewModel>();
        if (Configuration.ConfigUsageType.Length > 0)
        {
            m_SelectedUsage = Configuration.ConfigUsageType[0];
            m_ConfigInfo.TryGetUsageInfo(m_SelectedUsage, out var usageInfo);
            m_PreviewConfigConfigUsageInfo = usageInfo;
        }
        else
        {
            m_SelectedUsage = string.Empty;
            m_PreviewConfigConfigUsageInfo = null;
        }

        CreateAttributes();
    }

    #region Private Methods

    private void CreateAttributes()
    {
        foreach (var configAttributeInfo in m_ConfigInfo.AttributeInfos)
        {
            m_Attributes.Add(new ConfigAttributeListItemViewModel(configAttributeInfo));
        }
    }

    private void OnPreviewUsageChanged()
    {
        m_ConfigInfo.TryGetUsageInfo(m_SelectedUsage, out var usageInfo);
        PreviewConfigUsageInfo = usageInfo;
    }

    #endregion

    #region Public API

    public ConfigInfo GetConfigInfo()
    {
        return m_ConfigInfo;
    }

    #endregion
}