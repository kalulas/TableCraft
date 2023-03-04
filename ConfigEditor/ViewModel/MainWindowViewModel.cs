using System;
using System.ComponentModel;
namespace ConfigEditor.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        if (Program.ServiceProvider == null)
        {
            return;
        }
        
        if (!ConfigCodeGenLib.Configuration.IsInited)
        {
            System.Diagnostics.Trace.TraceError("Initialize ConfigCodeGenLib failed!");
            // TODO show error popup window
        }
    }

    public string LibEnvFilePath
    {
        get
        {
            return AppContext.BaseDirectory + "libenv.json";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}