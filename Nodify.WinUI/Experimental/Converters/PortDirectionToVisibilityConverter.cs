using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Nodify.WinUI.Experimental.Model;
using System;

namespace Nodify.WinUI.Experimental.Converters;

/// <summary>
/// Returns Visible when the bound PortDirection matches the expected direction (passed as ConverterParameter).
/// ConverterParameter: "Input" or "Output"
/// </summary>
public sealed class PortDirectionToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PortDirection direction && parameter is string expected)
        {
            return direction.ToString() == expected ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
