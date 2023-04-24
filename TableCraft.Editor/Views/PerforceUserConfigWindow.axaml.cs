#region File Header
// Filename: PerforceUserConfigWindow.axaml.cs
// Author: Kalulas
// Create: 2023-04-22
// Description:
#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TableCraft.Editor.Views;

public partial class PerforceUserConfigWindow : Window
{
    public PerforceUserConfigWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}