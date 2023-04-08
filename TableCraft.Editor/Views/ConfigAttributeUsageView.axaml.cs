#region FILE HEADER
// Filename: ThisIsJustAStupidViewHaveFun.axaml.cs
// Author: boming.chen
// Create: 2023-03-15
// Description:
// Design ->
// GPP ->
#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TableCraft.Editor.Views;

public partial class ConfigAttributeUsageView : UserControl
{
    public ConfigAttributeUsageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}