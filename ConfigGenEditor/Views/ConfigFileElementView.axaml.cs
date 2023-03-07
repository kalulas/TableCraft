using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ConfigGenEditor.Views;

public partial class ConfigFileElementView : UserControl
{
    public ConfigFileElementView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}