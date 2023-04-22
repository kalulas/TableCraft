#region File Header
// Filename: PerforceUserConfigViewModel.cs
// Author: Kalulas
// Create: 2023-04-22
// Description:
#endregion

using TableCraft.Core.VersionControl;

namespace TableCraft.Editor.ViewModels;

public class PerforceUserConfigViewModel : ViewModelBase
{
    #region Fields

    private readonly PerforceUserConfig m_PerforceUserConfig;

    #endregion

    #region Properties

    public string Port
    {
        get => m_PerforceUserConfig.P4PORT;
        set => m_PerforceUserConfig.P4PORT = value;
    }
    
    public string User
    {
        get => m_PerforceUserConfig.P4USER;
        set => m_PerforceUserConfig.P4USER = value;
    }
    
    public string Client
    {
        get => m_PerforceUserConfig.P4CLIENT;
        set => m_PerforceUserConfig.P4CLIENT = value;
    }
    
    public string Password
    {
        get => m_PerforceUserConfig.P4Passwd;
        set => m_PerforceUserConfig.UpdatePasswd(value);
    }

    #endregion
    
    public PerforceUserConfigViewModel(PerforceUserConfig config)
    {
        m_PerforceUserConfig = config;
        // TODO show password button, test connection button, save button
    }
}