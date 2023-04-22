#region File Header
// Filename: MessageBoxManager.cs
// Author: Kalulas
// Create: 2023-04-22
// Description:
#endregion

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace TableCraft.Editor.Services;

public static class MessageBoxManager
{
    public const string ErrorTitle = "Error";
    public const string SuccessTitle = "Success";
    public const float StandardPopupHeight = 180.0f;
    
    public static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
    
    public static async void ShowStandardMessageBox(string title, string message)
    {
        var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                MinHeight = StandardPopupHeight
            });
        
        await messageBox.Show();
    }
    
    public static async Task ShowMainWindowStandardMessageBoxDialog(string title, string message)
    {
        var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                MinHeight = StandardPopupHeight
            });

        await messageBox.ShowDialog(GetMainWindow());
    }
}