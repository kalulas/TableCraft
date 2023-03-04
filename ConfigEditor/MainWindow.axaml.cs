using Avalonia.Controls;
using ConfigEditor.ViewModel;

namespace ConfigEditor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}