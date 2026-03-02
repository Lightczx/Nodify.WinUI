using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace Nodify.WinUI.Experimental.Converters;

public sealed partial class EmptyObjectToVisibilityInverseConverter : EmptyObjectToObjectConverter
{
    public EmptyObjectToVisibilityInverseConverter()
    {
        EmptyValue = Visibility.Visible;
        NotEmptyValue = Visibility.Collapsed;
    }
}