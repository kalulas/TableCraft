using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using ConfigGenEditor.Models;
using ConfigGenEditor.Services;
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

    private readonly FakeDatabase m_Database;

    public MainWindowViewModel(FakeDatabase db)
    {
        m_Database = db;
        if (!EnvironmentCheck())
        {
            if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime desktop)
            {
                // TODO show error popup window, shutdown on confirm
                desktop.Shutdown();
            }
            
            // This is not supposed to happen ...
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
        foreach (var tableElement in fakeDatabase.ReadTableElements())
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
        
        // TODO maybe move this to ConfigCodeGenLib ...
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
        
        // TODO uniqueness check, popup message if duplicate
        var tableFileRelative = Path.GetRelativePath(ConfigHomePath, newTableFilePath);
        // imported table file, without json file
        var tableElement = new ConfigFileElement(tableFileRelative);
        var createdTableViewModel = new ConfigFileElementViewModel(tableElement);

        var insertAt = TableList.Count;
        for (var index = 0; index < TableList.Count; index++)
        {
            var tableVm = TableList[index];
            if (string.Compare(tableVm.ConfigFileRelativePath, createdTableViewModel.ConfigFileRelativePath, 
                    StringComparison.Ordinal) <= 0) continue;
            insertAt = index;
            break;
        }
        
        TableList.Insert(insertAt, createdTableViewModel);
        
        // select new added
        SelectedTable = createdTableViewModel;
        
        // TODO create ConfigCodeGenLib.ConfigReader.ConfigInfo and related ViewModel
        
        // write to local file, we make it async
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
                Logger.TryGet(LogEventLevel.Debug, LogArea.Control)?.Log(this, $"Selected file from dialog: {selected}");
                AddNewSelectedTableFile(selected);
            }
        }
    }

    #endregion
}