#region File Header
// Filename: ConfigAttributeDetailsView.axaml.cs
// Author: Kalulas
// Create: 2023-03-11
// Description:
#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TableCraft.Editor.Views;

public partial class ConfigAttributeDetailsView : UserControl
{
    public ConfigAttributeDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}