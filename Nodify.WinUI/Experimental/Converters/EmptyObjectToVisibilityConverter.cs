using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace Nodify.WinUI.Experimental.Converters;

public sealed partial class EmptyObjectToVisibilityConverter : EmptyObjectToObjectConverter
{
    public EmptyObjectToVisibilityConverter()
    {
        EmptyValue = Visibility.Collapsed;
        NotEmptyValue = Visibility.Visible;
    }
}
