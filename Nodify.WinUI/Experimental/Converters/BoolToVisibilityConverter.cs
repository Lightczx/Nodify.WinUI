using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Nodify.WinUI.Experimental.Converters
{
    /// <summary>
    /// 将布尔值转换为 Visibility 的转换器
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 获取或设置是否反转转换逻辑（默认 false）
        /// true 时将显示和隐藏的逻辑反转
        /// </summary>
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolValue = false;

            if (value is bool b)
            {
                boolValue = b;
            }
            else if (value != null)
            {
                // 对于非布尔值，null 为 false，非 null 为 true
                boolValue = true;
            }

            // 检查参数是否要求反转
            bool shouldInvert = IsInverted;
            if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                shouldInvert = true;
            }

            // 如果启用反转，则翻转布尔值
            if (shouldInvert)
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;
                return IsInverted ? !result : result;
            }

            return false;
        }
    }
}
