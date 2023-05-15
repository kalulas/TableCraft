#region File Header
// Filename: Perforce.cs
// Author: Kalulas
// Create: 2023-04-16
// Description:
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Perforce.P4;
using TableCraft.Core.IO;
using File = System.IO.File;

namespace TableCraft.Core.VersionControl;

public class Perforce : IFileEvent
{
    #region Fields

    public const string Label = "Perforce";

    private readonly Server m_Server;
    private readonly Repository m_Repository;
    private readonly Connection m_Connection;
    private readonly string m_Password;

    #endregion

    #region Properties

    public bool Connected => m_Connection.connectionEstablished();

    #endregion

    #region Constructor

    public Perforce(string uri, string user, string clientName, string passwd)
    {
        m_Server = new Server(new ServerAddress(uri));
        m_Password = passwd;
        m_Connection = new Repository(m_Server).Connection;
        m_Connection.UserName = user;
        m_Connection.Client = new Client
        {
            Name = clientName
        };
    }

    public Perforce(PerforceUserConfig config)
    {
        m_Server = new Server(new ServerAddress(config.P4PORT));
        m_Password = config.P4Passwd;
        m_Repository = new Repository(m_Server);
        m_Connection = m_Repository.Connection;
        m_Connection.UserName = config.P4USER;
        m_Connection.Client = new Client
        {
            Name = config.P4CLIENT
        };
    }

    #endregion

    #region Private Methods

    private void TryConnectAndLogin()
    {
        try
        {
            if (Connected)
            {
                Debugger.Log($"Already connected to perforce server {m_Server.Address.Uri}");
                return;
            }
            
            var connected = m_Connection.Connect(null);
            if (!connected)
            {
                Debugger.LogError($"Connect to {m_Server.Address.Uri} failed!");
                return;
            }

            var cred = m_Connection.Login(m_Password);
            Debugger.Log($"Connected to {m_Server.Address.Uri}, login success with credential {cred}");
        }
        catch (Exception e)
        {
            Debugger.LogError($"Login '{m_Server.Address.Uri}' failed with {e.Message}");
            m_Connection.Disconnect();
        }
    }

    #endregion

    #region Public API

    public string GetLabel()
    {
        return Label;
    }

    public void BeforeRead(string filePath)
    {
        if (!Connected)
        {
            return;
        }
        
        // Debugger.Log($"[Perforce.BeforeRead] {filePath}");
    }

    public void AfterRead(string filePath)
    {
        if (!Connected)
        {
            return;
        }
        
        // Debugger.Log($"[Perforce.AfterRead] {filePath}");
    }

    public void BeforeWrite(string filePath)
    {
        if (!Connected)
        {
            return;
        }

        var rootPath = m_Connection.Client.Root;
        if (string.IsNullOrEmpty(rootPath))
        {
            return;
        }
        
        var relativePath = Path.GetRelativePath(rootPath, filePath);
        if (relativePath == filePath)
        {
            // Debugger.LogWarning($"[Perforce.BeforeWrite] filePath {filePath} is not under client root,skip");
            return;
        }
        
        if (!File.Exists(filePath))
        {
            return;
        }
        
        var fileSpec = new FileSpec(new LocalPath(filePath), null);
        var options = new EditCmdOptions(EditFilesCmdFlags.None, 0, null);
        m_Connection.Client.EditFiles(options, fileSpec);
        Debugger.Log($"[Perforce.BeforeWrite] Edit file: {filePath}");
    }

    public void AfterWrite(string filePath)
    {
        if (!Connected)
        {
            return;
        }
        
        var rootPath = m_Connection.Client.Root;
        if (string.IsNullOrEmpty(rootPath))
        {
            return;
        }

        var relativePath = Path.GetRelativePath(rootPath, filePath);
        if (relativePath == filePath)
        {
            // Debugger.LogWarning($"[Perforce.AfterWrite] filePath {filePath} is not under client root,skip");
            return;
        }

        var opts = new ChangesCmdOptions(ChangesCmdFlags.None, null, 0, ChangeListStatus.None, null);
        var fileSpec = new FileSpec(new LocalPath(filePath), null);
        var changes = m_Repository.GetChangelists(opts, fileSpec);
        // already in other changelist, ignore
        if (changes != null && changes.Any())
        {
            return;
        }
        
        var options = new AddFilesCmdOptions(AddFilesCmdFlags.None, 0, null);
        m_Connection.Client.AddFiles(options, fileSpec);
        Debugger.Log($"[Perforce.AfterWrite] Add file: {filePath}");
    }

    public void OnRegistered()
    {
        // Synchronous call will block the main thread
        Task.Run(TryConnectAndLogin);
    }

    public void OnUnregistered()
    {
        if (!Connected)
        {
            return;
        }

        // Synchronous call will block the main thread
        Task.Run(() =>
        {
            m_Connection.Disconnect();
            Debugger.Log("[Perforce.OnUnregistered] Disconnected");
        });
    }
    
    public async Task TryConnectAndLoginAsync()
    {
        await Task.Run(TryConnectAndLogin);
    }

    #endregion
}