using Microsoft.UI.Xaml.Data;
using System;

namespace Nodify.WinUI.Experimental.Converters;

/// <summary>
/// Converts a double value to percentage string (e.g., 1.0 -> "100%")
/// </summary>
public sealed partial class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is double doubleValue ? $"{doubleValue:P0}" : "0%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}