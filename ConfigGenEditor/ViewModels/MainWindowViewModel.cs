using System.ComponentModel;
using ConfigGenEditor.Models;

namespace ConfigGenEditor.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        if (!ConfigCodeGenLib.Configuration.IsInited)
        {
            System.Diagnostics.Trace.TraceError("Initialize ConfigCodeGenLib failed!");
            // TODO show error popup window
        }
    }
    
    public string ListJsonFilename => Program.ListJsonFilename;

    public string ConfigHomePath => Program.GetConfigHomePath();

    public string JsonHomePath => Program.GetJsonHomePath();

    public ConfigFileElement[] SavedConfigFileElements => ConfigFileContext.GetSavedElements();

    public event PropertyChangedEventHandler? PropertyChanged;

    public void AddConfigFileElement()
    {
        
    }
}