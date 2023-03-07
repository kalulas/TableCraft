using System.ComponentModel;

namespace ConfigEditor.ViewModel;

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

    public string ConfigHomePath => Program.GetConfigHomePath();

    public string JsonHomePath => Program.GetJsonHomePath();

    public event PropertyChangedEventHandler? PropertyChanged;
}