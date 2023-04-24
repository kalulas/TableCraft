#region File Header
// Filename: PerforceConnectionStateConverter.cs
// Author: Kalulas
// Create: 2023-04-24
// Description:
#endregion

using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using TableCraft.Editor.ViewModels;

namespace TableCraft.Editor.Converters;

/// <summary>
/// BoolConverter: Return true if two <see cref="PerforceConnectionState"/> are the same
/// </summary>
public class PerforceConnectionStateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PerforceConnectionState currentState && parameter is PerforceConnectionState comparerState &&
            targetType == typeof(bool))
        {
            return currentState == comparerState;
        }

        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}