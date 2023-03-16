#region FILE HEADER
// Filename: ConfigAttributeTagView.axaml.cs
// Author: boming.chen
// Create: 2023-03-16
// Description:
#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ConfigGenEditor.Views;

public partial class ConfigAttributeTagView : UserControl
{
    public ConfigAttributeTagView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}