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
    private readonly FakeDatabase m_Database;
    private readonly List<FileDialogFilter> m_NewTableFileFilters = new();
    private readonly Dictionary<string, ConfigInfoViewModel> m_EditorRuntimeConfigInfo = new();

    #region Propreties

    public string ListJsonFilename => Program.ListJsonFilename;

    public string ConfigHomePath => Program.GetConfigHomePath();

    public string JsonHomePath => Program.GetJsonHomePath();

    private ConfigFileElementViewModel? m_SelectedTable;

    /// <summary>
    /// For table list select use
    /// </summary>
    public ConfigFileElementViewModel? SelectedTable
    {
        get => m_SelectedTable;
        set => this.RaiseAndSetIfChanged(ref m_SelectedTable, value);
    }
    
    public ObservableCollection<ConfigFileElementViewModel>? TableList { get; private set; }

    private ConfigInfoViewModel? m_SelectedConfigInfo;

    /// <summary>
    /// For attributes display use
    /// </summary>
    public ConfigInfoViewModel? SelectedConfigInfo
    {
        get => m_SelectedConfigInfo;
        private set => this.RaiseAndSetIfChanged(ref m_SelectedConfigInfo, value);
    }

    private ConfigAttributeDetailsViewModel? m_SelectedAttribute;

    public ConfigAttributeDetailsViewModel? SelectedAttribute
    {
        get => m_SelectedAttribute;
        set => this.RaiseAndSetIfChanged(ref m_SelectedAttribute, value);
    }

    #endregion

    #region Commands

    public ICommand? AddNewTableFileCommand { get; private set; }
    public EventHandler<SelectionChangedEventArgs>? SelectedTableChangedEventHandler { get; private set; }
    public EventHandler<SelectionChangedEventArgs>? SelectedAttributeChangedEventHandler { get; private set; }

    #endregion

    #region Private Methods

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

        AppendNewTableFileFilter();
        CreateSubViewModels(db);
        CreateCommands();
    }

    private bool EnvironmentCheck()
    {
        return ConfigCodeGenLib.Configuration.IsInited;
    }
    
    private void AppendNewTableFileFilter()
    {
        // TODO get extensions from lib?
        var extensions = new List<string>
        {
            "csv",
        };
        
        var fileFilter = new FileDialogFilter
        {
            Extensions = extensions,
            Name = "Table Files",
        };

        m_NewTableFileFilters.Add(fileFilter);
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
        SelectedTableChangedEventHandler = OnSelectedTableChanged;
        SelectedAttributeChangedEventHandler = OnSelectedAttributeChanged;
    }

    private void CancelSelectedAttribute()
    {
        SelectedAttribute = null;
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
        var tableElement = new ConfigFileElement(tableFileRelative, string.Empty);
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
        
        // This is implement in SelectedTable setter
        // SelectedConfigInfo = new ConfigInfoViewModel(createdTableViewModel.ConfigFilePath,
        //     createdTableViewModel.JsonFilePath, createdTableViewModel.GetConfigType());
        
        // write to local file, we make it async
        var updatedTableList = TableList.Select(viewModel => viewModel.GetElement()).ToList();
#pragma warning disable CS4014
        m_Database.WriteTableElements(updatedTableList);
#pragma warning restore CS4014
    }

    private void UpdateSelectedConfigInfoWithTable(ConfigFileElementViewModel? selectedTable)
    {
        if (selectedTable == null)
        {
            SelectedConfigInfo = null;
            return;
        }

        var identifier = selectedTable.ConfigFileRelativePath;
        var existed = m_EditorRuntimeConfigInfo.TryGetValue(identifier, out var createdConfigInfo);
        if (existed)
        {
            SelectedConfigInfo = createdConfigInfo;
            return;
        }

        var newConfigInfo = new ConfigInfoViewModel(selectedTable.ConfigFilePath, selectedTable.JsonFilePath,
            selectedTable.GetConfigType());
        m_EditorRuntimeConfigInfo.Add(identifier, newConfigInfo);
        SelectedConfigInfo = newConfigInfo;

        // reset selected attribute
        CancelSelectedAttribute();
    }

    private void UpdateSelectedAttributeWithListItem(ConfigAttributeListItemViewModel? listItemViewModel)
    {
        if (listItemViewModel == null)
        {
            CancelSelectedAttribute();
            return;
        }

        var configInfo = listItemViewModel.GetAttributeInfo();
        SelectedAttribute = new ConfigAttributeDetailsViewModel(configInfo);
    }

    #endregion

    #region Interactions

    private async Task OnAddNewTableButtonClicked()
    {
        var dialog = new OpenFileDialog
        {
            Directory = ConfigHomePath,
            Filters = m_NewTableFileFilters,
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

    private void OnSelectedTableChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
        var selectedTable = selected as ConfigFileElementViewModel;
        UpdateSelectedConfigInfoWithTable(selectedTable);
    }
    
    private void OnSelectedAttributeChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
        var selectedListItem = selected as ConfigAttributeListItemViewModel;
        UpdateSelectedAttributeWithListItem(selectedListItem);
    }

    #endregion
}