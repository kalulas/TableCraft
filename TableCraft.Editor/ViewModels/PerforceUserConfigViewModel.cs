#region File Header
// Filename: PerforceUserConfigViewModel.cs
// Author: Kalulas
// Create: 2023-04-22
// Description:
#endregion

using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using TableCraft.Core.VersionControl;
using TableCraft.Editor.Services;

namespace TableCraft.Editor.ViewModels;

public enum PerforceConnectionState
{
    None,
    Connecting,
    Connected,
    Failed,
}

public class PerforceUserConfigViewModel : ViewModelBase
{
    #region Fields

    private readonly PerforceUserConfig m_PerforceUserConfig;
    private PerforceConnectionState m_ConnectionState;
    private bool m_PasswordShown;

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
    
    public PerforceConnectionState ConnectionState
    {
        get => m_ConnectionState;
        set
        {
            this.RaiseAndSetIfChanged(ref m_ConnectionState, value);
            this.RaisePropertyChanged(nameof(IsConnecting));
        }
    }

    public bool PasswordShown
    {
        get => m_PasswordShown;
        set => this.RaiseAndSetIfChanged(ref m_PasswordShown, value);
    }

    /// <summary>
    /// Binding on ConnectionState and check if it's connecting with Converter need more code, so we add this 'IsConnection' property for convenience.
    /// RaisePropertyChanged in <see cref="ConnectionState"/>
    /// </summary>
    public bool IsConnecting => m_ConnectionState == PerforceConnectionState.Connecting;

    public ReactiveCommand<Unit,Unit> ShowPasswdCommand { get; }
    public ReactiveCommand<Unit,Unit> SaveCommand { get; }
    
    public ReactiveCommand<Unit,Unit> TestConnectionCommand { get; }

    #endregion

    #region Constructors

    public PerforceUserConfigViewModel(PerforceUserConfig config)
    {
        m_PerforceUserConfig = config;
        m_ConnectionState = PerforceConnectionState.None;
        m_PasswordShown = false;
        SaveCommand = ReactiveCommand.CreateFromTask(OnSaveButtonClicked);
        SaveCommand.ThrownExceptions.Subscribe(Program.HandleException);
        TestConnectionCommand = ReactiveCommand.CreateFromTask(OnTestConnectionButtonClicked);
        TestConnectionCommand.ThrownExceptions.Subscribe(Program.HandleException);
        ShowPasswdCommand = ReactiveCommand.Create(OnShowPasswdButtonClicked);
        ShowPasswdCommand.ThrownExceptions.Subscribe(Program.HandleException);
    }

    private void OnShowPasswdButtonClicked()
    {
        PasswordShown = !PasswordShown;
    }

    #endregion

    #region Interactions

    private async Task OnSaveButtonClicked()
    {
        var result = await Program.UpdateVersionControlConfig(m_PerforceUserConfig);
        var popupTitle = result ? MessageBoxManager.SuccessTitle : MessageBoxManager.ErrorTitle;
        var popupContent = result ? "Perforce configuration saved!" : "Failed to save Perforce configuration";
        await MessageBoxManager.ShowStandardMessageBoxDialog(popupTitle, popupContent);
    }
    
    private async Task OnTestConnectionButtonClicked()
    {
        ConnectionState = PerforceConnectionState.None;
        if (!m_PerforceUserConfig.IsReady())
        {
            await MessageBoxManager.ShowStandardMessageBoxDialog(MessageBoxManager.ErrorTitle, "Please fill in all fields");
            return;
        }

        var versionControl = new Core.VersionControl.Perforce(m_PerforceUserConfig);
        ConnectionState = PerforceConnectionState.Connecting;
        await versionControl.TryConnectAndLoginAsync();
        ConnectionState = versionControl.Connected ? PerforceConnectionState.Connected : PerforceConnectionState.Failed;
    }
    
    #endregion
}