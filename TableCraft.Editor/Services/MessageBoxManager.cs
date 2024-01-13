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
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace TableCraft.Editor.Services;

public static class MessageBoxManager
{
    public const string ErrorTitle = "Error";
    public const string SuccessTitle = "Success";
    private const float m_StandardPopupWidth = 300.0f;
    private const float m_StandardPopupHeight = 180.0f;
    
    public static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.Windows[^1];
        }

        return null;
    }
    
    public static async void ShowStandardMessageBox(string title, string message)
    {
        var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                MinWidth = m_StandardPopupWidth,
                MinHeight = m_StandardPopupHeight
            });

        await messageBox.ShowAsync();
    }
    
    public static async Task ShowStandardMessageBoxDialog(string title, string message)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null)
        {
            return;
        }
        
        var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                MinWidth = m_StandardPopupWidth,
                MinHeight = m_StandardPopupHeight
            });

        await messageBox.ShowWindowDialogAsync(mainWindow);
    }

    public static async Task ShowCustomMarkdownMessageBoxDialog(string title, string message)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null)
        {
            return;
        }
        
        var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                Markdown = true,
                ContentTitle = title,
                ContentMessage = message,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                MinWidth = m_StandardPopupWidth,
                MinHeight = m_StandardPopupHeight
            });

        await messageBox.ShowWindowDialogAsync(mainWindow);
    }
}