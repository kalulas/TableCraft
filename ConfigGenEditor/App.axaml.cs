using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ConfigGenEditor.Services;
using ConfigGenEditor.ViewModels;
using ConfigGenEditor.Views;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Serilog;

namespace ConfigGenEditor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static Window? GetMainWindow()
    {
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (!ConfigCodeGenLib.Configuration.IsInited)
            {
                const string errorMessage = $"library's initialization failed, please check '{Program.LibEnvJsonFilename}'";
                var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                    new MessageBoxStandardParams
                    {
                        ButtonDefinitions = ButtonEnum.Ok,
                        ContentTitle = "Error",
                        ContentMessage = errorMessage,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        MinHeight = 180.0
                    });
                
                messageBox.Show();
                Log.Error(errorMessage);
                // return here so main window will not be opened
                return;
            }

            var listJsonFilePath = AppContext.BaseDirectory + Program.ListJsonFilename;
            if (!File.Exists(listJsonFilePath))
            {
                throw new FileNotFoundException(listJsonFilePath + " not found, construct database failed");
            }

            var db = new FakeDatabase(listJsonFilePath);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(db),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}