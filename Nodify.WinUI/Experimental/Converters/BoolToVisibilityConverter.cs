using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace Nodify.WinUI.Experimental.Converters;

public sealed partial class BoolToVisibilityInverseConverter : BoolToObjectConverter
{
    public BoolToVisibilityInverseConverter()
    {
        TrueValue = Visibility.Collapsed;
        FalseValue = Visibility.Visible;
    }
}