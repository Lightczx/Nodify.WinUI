using Microsoft.UI.Xaml.Data;
using System;

namespace Nodify.WinUI.Experimental.Converters
{
    /// <summary>
    /// Converts null to false and non-null to true (or inverted)
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isNull = value == null;
            
            // Check if we should invert the result
            bool invert = parameter is string str && 
                         (str.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                          str.Equals("invert", StringComparison.OrdinalIgnoreCase));
            
            return invert ? isNull : !isNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
