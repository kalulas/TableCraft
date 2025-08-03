#region File Header
// Filename: PerforceUserConfig.cs
// Author: Kalulas
// Create: 2023-04-22
// Description:
#endregion

using System;

namespace TableCraft.Core.VersionControl;

public class PerforceUserConfig
{
    public string P4PORT { get; set; }
    public string P4USER { get; set; }
    public string P4CLIENT { get; set; }
    public string P4PASSWDBASE64 { get; set; }

    public string P4Passwd { get; private set; } = string.Empty;

    public bool IsReady()
    {
        return !string.IsNullOrEmpty(P4PORT) && !string.IsNullOrEmpty(P4USER) && !string.IsNullOrEmpty(P4CLIENT) &&
               !string.IsNullOrEmpty(P4Passwd);
    }

    public void Decode()
    {
        P4Passwd = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(P4PASSWDBASE64));
    }

    /// <summary>
    /// Encode password to base64, make sure this is called before saving to file
    /// </summary>
    public void Encode()
    {
        P4PASSWDBASE64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(P4Passwd));
    }

    public void UpdatePasswd(string password)
    {
        P4Passwd = password;
    }

    public override string ToString()
    {
        return $"P4PORT: {P4PORT}, P4USER: {P4USER}, P4CLIENT: {P4CLIENT}";
    }
}