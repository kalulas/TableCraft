#region File Header
// Filename: ConfigInfoViewModel.cs
// Author: Kalulas
// Create: 2023-03-11
// Description: 
#endregion

using System.Collections.ObjectModel;
using System.IO;
using ConfigCodeGenLib;
using ConfigCodeGenLib.ConfigReader;

namespace ConfigGenEditor.ViewModels;

/// <summary>
/// A viewmodel wrapper of <see cref="ConfigCodeGenLib.ConfigReader.ConfigInfo"/>
/// </summary>
public class ConfigInfoViewModel : ViewModelBase
{
    #region Fields

    private readonly ConfigInfo m_ConfigInfo;
    private readonly ObservableCollection<ConfigAttributeListItemViewModel> m_Attributes;

    #endregion

    #region Properties

    public ObservableCollection<ConfigAttributeListItemViewModel> Attributes => m_Attributes;

    #endregion

    public ConfigInfoViewModel(ConfigInfo configInfo)
    {
        m_ConfigInfo = configInfo;
        m_Attributes = new ObservableCollection<ConfigAttributeListItemViewModel>();
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

    #endregion

    #region Public API

    public ConfigInfo GetConfigInfo()
    {
        return m_ConfigInfo;
    }

    #endregion
}