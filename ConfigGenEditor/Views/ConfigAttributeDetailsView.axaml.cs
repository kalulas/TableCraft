#region File Header
// Filename: ConfigAttributeDetailsView.axaml.cs
// Author: Kalulas
// Create: 2023-03-11
// Description:
#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ConfigGenEditor.ViewModels;

namespace ConfigGenEditor.Views;

public partial class ConfigAttributeDetailsView : UserControl
{
    public ConfigAttributeDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        var valueTypeComboBox = this.Find<ComboBox>("ValueTypeComboBox");
        // TODO get these from configCodeGenLib, current not accessible
        valueTypeComboBox.Items = new[] {"bool", "int", "uint", "long", "ulong", "string", "float", "double"};
        valueTypeComboBox.SelectedIndex = 0;
    }
}