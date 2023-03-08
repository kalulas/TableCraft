using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Logging;
using ConfigGenEditor.Models;
using ConfigGenEditor.Services;
using Microsoft.Extensions.Hosting.Internal;
using ReactiveUI;
using Path = System.IO.Path;

namespace ConfigGenEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string ListJsonFilename => Program.ListJsonFilename;

    public string ConfigHomePath => Program.GetConfigHomePath();

    public string JsonHomePath => Program.GetJsonHomePath();

    private ConfigFileElementViewModel? m_SelectedTable;
    private List<FileDialogFilter>? m_NewTableFileFilters;

    public ConfigFileElementViewModel? SelectedTable
    {
        get => m_SelectedTable;
        set => this.RaiseAndSetIfChanged(ref m_SelectedTable, value);
    }
    
    public ObservableCollection<ConfigFileElementViewModel>? TableList { get; private set; }

    private FakeDatabase m_Database;

    public MainWindowViewModel(FakeDatabase db)
    {
        m_Database = db;
        if (!EnvironmentCheck())
        {
            // System.Diagnostics.Trace.TraceError("Initialize ConfigCodeGenLib failed!");
            // TODO show error popup window
            return;
        }

        CreateSubViewModels(db);
        CreateCommands();
    }

    #region Commands

    public ICommand? AddNewTableFileCommand { get; private set; }

    #endregion

    #region Private Methods

    private bool EnvironmentCheck()
    {
        return ConfigCodeGenLib.Configuration.IsInited;
    }

    private void CreateSubViewModels(FakeDatabase fakeDatabase)
    {
        TableList = new ObservableCollection<ConfigFileElementViewModel>();
        foreach (var tableElement in fakeDatabase.GetTableElements())
        {
            TableList.Add(new ConfigFileElementViewModel(tableElement));
        }
    }

    private void CreateCommands()
    {
        AddNewTableFileCommand = ReactiveCommand.CreateFromTask(OnAddNewTableButtonClicked);
    }

    private List<FileDialogFilter> GetNewTableFileFilterList()
    {
        if (m_NewTableFileFilters != null)
        {
            return m_NewTableFileFilters;
        }
        
        var extensions = new List<string>
        {
            "csv",
        };
        
        var fileFilter = new FileDialogFilter
        {
            Extensions = extensions,
            Name = "Table Files",
        };

        m_NewTableFileFilters = new List<FileDialogFilter> {fileFilter};
        return m_NewTableFileFilters;
    }

    private void AddNewSelectedTableFile(string newTableFilePath)
    {
        if (TableList == null)
        {
            return;
        }
        
        var tableFileRelative = Path.GetRelativePath(ConfigHomePath, newTableFilePath);
        // imported table file, without json file
        var tableElement = new ConfigFileElement(tableFileRelative);
        var tableViewModel = new ConfigFileElementViewModel(tableElement);
        TableList.Add(tableViewModel);

        var updatedTableList = TableList.Select(viewModel => viewModel.GetElement()).ToList();
        m_Database.WriteTableElements(updatedTableList);
    }

    #endregion

    #region Interactions

    private async Task OnAddNewTableButtonClicked()
    {
        var dialog = new OpenFileDialog
        {
            Directory = ConfigHomePath,
            Filters = GetNewTableFileFilterList(),
            AllowMultiple = false
        };
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var result = await dialog.ShowAsync(desktop.MainWindow);
            if (result != null)
            {
                var selected = result[0];
                Logger.TryGet(LogEventLevel.Debug, LogArea.Control)?.Log(this, selected);
                AddNewSelectedTableFile(selected);
            }
        }
    }

    #endregion
}