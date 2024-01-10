using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableCraft.Editor.ViewModels;

namespace TableCraft.Editor.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowPerforceWindow.RegisterHandler(DoShowDialogAsync)));
    }

    private void SelectingTableItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ViewModel?.SelectedTableChangedEventHandler?.Invoke(sender, e);
    }

    private void SelectingAttributeItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ViewModel?.SelectedAttributeChangedEventHandler?.Invoke(sender, e);
    }
    
    private void TableListInputSearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel?.TableListSearchBoxTextChangedEventHandler?.Invoke(sender, e);
    }
    
    private async Task DoShowDialogAsync(InteractionContext<PerforceUserConfigViewModel, PerforceUserConfigViewModel?> interaction)
    {
        var dialog = new PerforceUserConfigWindow
        {
            DataContext = interaction.Input // this is the PerforceUserConfigViewModel, create in OnOpenPerforceWindowButtonClicked
        };

        var result = await dialog.ShowDialog<PerforceUserConfigViewModel?>(this);
        interaction.SetOutput(result);
    }
}