using System.Collections.ObjectModel;
using ConfigGenEditor.Services;
using ReactiveUI;

namespace ConfigGenEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(FakeDatabase db)
    {
        if (!ConfigCodeGenLib.Configuration.IsInited)
        {
            System.Diagnostics.Trace.TraceError("Initialize ConfigCodeGenLib failed!");
            // TODO show error popup window
        }

        ElementList = new ObservableCollection<ConfigFileElementViewModel>();
        foreach (var configFileElement in db.GetElements())
        {
            ElementList.Add(new ConfigFileElementViewModel(configFileElement));
        }
    }
    
    public string ListJsonFilename => Program.ListJsonFilename;

    public string ConfigHomePath => Program.GetConfigHomePath();

    public string JsonHomePath => Program.GetJsonHomePath();

    private ConfigFileElementViewModel m_SelectedConfigFileElement;

    public ConfigFileElementViewModel SelectedConfigFileElement
    {
        get => m_SelectedConfigFileElement;
        set => this.RaiseAndSetIfChanged(ref m_SelectedConfigFileElement, value);
    }
    
    public ObservableCollection<ConfigFileElementViewModel> ElementList { get; }

    public void AddConfigFileElement()
    {
        
    }
}