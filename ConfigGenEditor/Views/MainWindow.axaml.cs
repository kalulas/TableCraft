using Avalonia.Controls;
using ConfigGenEditor.ViewModels;

namespace ConfigGenEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SelectingTableItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        viewModel?.SelectedTableChangedEventHandler?.Invoke(sender, e);
    }

    private void SelectingAttributeItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        viewModel?.SelectedAttributeChangedEventHandler?.Invoke(sender, e);
    }
}