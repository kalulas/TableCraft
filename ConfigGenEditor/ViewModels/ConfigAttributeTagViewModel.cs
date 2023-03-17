#region FILE HEADER
// Filename: ConfigAttributeTagViewModel.cs
// Author: boming.chen
// Create: 2023-03-16
// Description:
#endregion

using System.Windows.Input;
using ReactiveUI;

namespace ConfigGenEditor.ViewModels;

public class ConfigAttributeTagViewModel : ViewModelBase
{
    #region Fields

    private readonly ConfigAttributeDetailsViewModel m_DetailsViewModel;

    #endregion

    #region Properties

    public string Content { get; }
    public ICommand RemoveTagCommand { get; }

    #endregion

    #region Constructors

    public ConfigAttributeTagViewModel(string content, ConfigAttributeDetailsViewModel detailsViewModel)
    {
        Content = content;
        m_DetailsViewModel = detailsViewModel;
        RemoveTagCommand = ReactiveCommand.Create(OnRemoveTagBtnClicked);
    }

    #endregion

    #region Interaction

    private void OnRemoveTagBtnClicked()
    {
        m_DetailsViewModel.OnTagRemoved(this);
    }

    #endregion
}