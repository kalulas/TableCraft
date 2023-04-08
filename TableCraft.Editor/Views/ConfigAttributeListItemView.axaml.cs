#region File Header
// Filename: ConfigAttributeListItemView.axaml.cs
// Author: Kalulas
// Create: 2023-03-11
// Description:
#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TableCraft.Editor.Views;

public partial class ConfigAttributeListItemView : UserControl
{
    public ConfigAttributeListItemView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}