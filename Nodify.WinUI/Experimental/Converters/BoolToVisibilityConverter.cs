using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Nodify.WinUI.Experimental.Converters
{
    /// <summary>
    /// Converts bool to Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                bool invert = parameter?.ToString()?.ToLower() == "invert";
                bool result = invert ? !boolValue : boolValue;
                return result ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                bool invert = parameter?.ToString()?.ToLower() == "invert";
                bool result = visibility == Visibility.Visible;
                return invert ? !result : result;
            }
            return false;
        }
    }
}
