﻿#region File Header
// Filename: ConfigInfoView.axaml.cs
// Author: Kalulas
// Create: 2023-04-05
// Description:
#endregion

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TableCraft.Editor.Views;

public partial class ConfigInfoView : UserControl
{
    public ConfigInfoView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}