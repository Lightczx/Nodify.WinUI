using Microsoft.UI.Xaml.Data;
using System;

namespace Nodify.WinUI.Experimental.Converters
{
    /// <summary>
    /// Converts a double value to percentage string (e.g., 1.0 -> "100%")
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                return $"{doubleValue:P0}";
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
