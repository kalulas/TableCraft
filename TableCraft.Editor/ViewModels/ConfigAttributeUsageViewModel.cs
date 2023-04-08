#region FILE HEADER
// Filename: ConfigAttributeUsageViewModel.cs
// Author: boming.chen
// Create: 2023-03-15
// Description:
#endregion

using System.Windows.Input;
using TableCraft.Core.ConfigReader;
using ReactiveUI;

namespace TableCraft.Editor.ViewModels;

public class ConfigAttributeUsageViewModel : ViewModelBase
{
    #region Fields

    private readonly ConfigAttributeUsageInfo m_AttributeUsageInfo;
    private readonly ConfigAttributeDetailsViewModel m_DetailsViewModel;

    #endregion

    #region Properties

    public string Usage => m_AttributeUsageInfo.Usage;
    
    public string FieldName
    {
        get => m_AttributeUsageInfo.FieldName;
        set => m_AttributeUsageInfo.FieldName = value;
    }
    
    public ICommand RemoveUsageCommand { get; private set; }

    #endregion

    #region Constructor

    public ConfigAttributeUsageViewModel(ConfigAttributeUsageInfo attributeUsageInfo, ConfigAttributeDetailsViewModel detailsViewModel)
    {
        m_AttributeUsageInfo = attributeUsageInfo;
        m_DetailsViewModel = detailsViewModel;
        RemoveUsageCommand = ReactiveCommand.Create(OnRemoveUsageBtnClicked);
    }

    #endregion

    #region Interaction

    private void OnRemoveUsageBtnClicked()
    {
        m_DetailsViewModel.OnUsageRemoved(this);
    }

    #endregion
}