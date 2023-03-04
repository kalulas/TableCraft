using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    public string ConfigHomePath
    {
        get
        {
            if (Program.ServiceProvider == null)
            {
                return string.Empty;
            }

            var config = Program.ServiceProvider.Services.GetRequiredService<IConfiguration>();
            var path = config.GetValue<string>("ConfigHomePath");
            return path ?? string.Empty;
        }
    }
    
    public string JsonHomePath
    {
        get
        {
            if (Program.ServiceProvider == null)
            {
                return string.Empty;
            }

            var config = Program.ServiceProvider.Services.GetRequiredService<IConfiguration>();
            var path = config.GetValue<string>("JsonHomePath");
            return path ?? string.Empty;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}