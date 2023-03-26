using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ConfigCodeGenLib;
using ConfigGenEditor.Models;
using ConfigGenEditor.Services;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using Serilog;
using Path = System.IO.Path;

namespace ConfigGenEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FakeDatabase m_Database;
    private readonly List<FileDialogFilter> m_NewTableFileFilters = new();
    /// <summary>
    /// ConfigFileRelativeFilePath -> ConfigInfoViewModel, create on selected
    /// </summary>
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
    public ICommand? SaveJsonFileCommand { get; private set; }
    public ICommand? GenerateCodeCommand { get; private set; }
    public EventHandler<SelectionChangedEventArgs>? SelectedTableChangedEventHandler { get; private set; }
    public EventHandler<SelectionChangedEventArgs>? SelectedAttributeChangedEventHandler { get; private set; }

    #endregion

    #region Private Methods

    public MainWindowViewModel(FakeDatabase db)
    {
        m_Database = db;
        AppendNewTableFileFilter();
        CreateSubViewModels(db);
        CreateCommands();
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
        SaveJsonFileCommand = ReactiveCommand.Create(OnSaveJsonButtonClicked);
        GenerateCodeCommand = ReactiveCommand.Create(OnGenerateCodeButtonClicked);
        SelectedTableChangedEventHandler = OnSelectedTableChanged;
        SelectedAttributeChangedEventHandler = OnSelectedAttributeChanged;
    }

    private void CancelSelectedAttribute()
    {
        SelectedAttribute = null;
    }
    
    private async Task FlushTableListToDatabase()
    {
        if (TableList == null)
        {
            return;
        }
        
        var updatedTableList = TableList.Select(viewModel => viewModel.GetElement()).ToList();
        await m_Database.WriteTableElements(updatedTableList);
    }

    private async Task AddNewSelectedTableFile(string newTableFilePath)
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
        await FlushTableListToDatabase();
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

        var configInfo = ConfigManager.singleton.AddNewConfigInfo(selectedTable.ConfigFilePath,
            selectedTable.JsonFilePath, selectedTable.GetConfigType());
        if (configInfo == null)
        {
            Log.Error("Failed to create config info for '{identifier}' with config home path '{HomePath}'",
                selectedTable.ConfigFileRelativePath, Program.GetConfigHomePath());
            SelectedConfigInfo = null;
            return;
        }
        
        var configInfoViewModel = new ConfigInfoViewModel(configInfo);
        m_EditorRuntimeConfigInfo.Add(identifier, configInfoViewModel);
        SelectedConfigInfo = configInfoViewModel;

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

    private async Task SaveJsonFileWithCurrentSelected()
    {
        if (m_SelectedConfigInfo == null || m_SelectedTable == null)
        {
            Log.Error("No selected config info, cannot save json file");
            return;
        }

        // step1 save json file
        var configInfo = m_SelectedConfigInfo.GetConfigInfo();
        var jsonFileName = m_SelectedTable.GetTargetJsonFileName();
        var jsonFileFullPath = Path.Combine(JsonHomePath, jsonFileName);
        var success = ConfigManager.singleton.SaveConfigInfoJsonFile(configInfo, jsonFileFullPath);
        if (!success)
        {
            Log.Error("Failed to save json file '{JsonFilePath}'", jsonFileFullPath);
            return;
        }
        
        Log.Information("Saved json file '{JsonFilePath}'", jsonFileFullPath);
        
        // step2 update and save list.json
        m_SelectedTable.SetJsonFileRelativePath(jsonFileName);
        await FlushTableListToDatabase();
        Log.Information("Save table list to list.json finished");
        
        // step3 update 'json file found' status
        m_SelectedTable.NotifyJsonFileStatusChanged();
        
        // step4 success message popup
        var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentTitle = "Success",
            ContentMessage = $"Json file saved: {jsonFileFullPath}",
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            MinHeight = 180.0,
            CanResize = true
        });

        await messageBox.ShowDialog(App.GetMainWindow());
    }

    private void GenerateCodeWithCurrentUsage()
    {
        // TODO to appsettings.json with different usage
        var outputDir = AppContext.BaseDirectory;
        var configInfo = m_SelectedConfigInfo?.GetConfigInfo();
        if (configInfo == null)
        {
            Log.Error("[MainWindowViewModel.GenerateCodeWithCurrentUsage] selected configInfo is null, exit");
            return;
        }

        var success = ConfigManager.singleton.GenerateCodeForUsage(Configuration.ConfigUsageType[0], configInfo, outputDir);
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
                Log.Information("Selected file from dialog: '{SelectedFile}'", selected);
#pragma warning disable CS4014
                AddNewSelectedTableFile(selected);
#pragma warning restore CS4014
            }
        }
    }
    
    private void OnSaveJsonButtonClicked()
    {
#pragma warning disable CS4014
        SaveJsonFileWithCurrentSelected();
#pragma warning restore CS4014
    }

    private void OnGenerateCodeButtonClicked()
    {
        GenerateCodeWithCurrentUsage();
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