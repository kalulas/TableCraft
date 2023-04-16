#region File Header
// Filename: Perforce.cs
// Author: Kalulas
// Create: 2023-04-16
// Description:
#endregion

using System;
using Perforce.P4;
using TableCraft.Core.IO;

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

    public bool Connected => m_Connection.Status == ConnectionStatus.Connected;

    #endregion

    #region Constructor

    public Perforce(string uri, string user, string clientName, string passwd)
    {
        m_Server = new Server(new ServerAddress(uri));
        m_Repository = new Repository(m_Server);
        m_Password = passwd;
        m_Connection = m_Repository.Connection;
        m_Connection.UserName = user;
        m_Connection.Client = new Client
        {
            Name = clientName
        };
    }

    #endregion

    #region Public API

    public string GetLabel()
    {
        return Label;
    }

    public void BeforeRead(string filePath)
    {
        // Debugger.Log($"[Perforce.BeforeRead] {filePath}");
    }

    public void AfterRead(string filePath)
    {
        // Debugger.Log($"[Perforce.AfterRead] {filePath}");
    }

    public void BeforeWrite(string filePath)
    {
        // Debugger.Log($"[Perforce.BeforeWrite] {filePath}");
    }

    public void AfterWrite(string filePath)
    {
        // Debugger.Log($"[Perforce.AfterWrite] {filePath}");
    }

    public void OnRegistered()
    {
        var connected = m_Connection.Connect(null);
        if (!connected)
        {
            Debugger.LogError($"Connect to {m_Server.Address} failed!");
            return;
        }
        
        try
        {
            m_Connection.Login(m_Password);
        }
        catch (Exception e)
        {
            Debugger.LogError($"Login failed with {e.Message}");
            m_Connection.Disconnect();
        }

        var info = m_Repository.GetServerMetaData(null);
        Debugger.Log($"Connected to {info.Address.Uri}");
    }

    public void OnUnregistered()
    {
        if (!Connected)
        {
            return;
        }

        m_Connection.Disconnect();
    }

    #endregion
}