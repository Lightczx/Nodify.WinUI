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

    protected override bool CheckValueIsEmpty(object value)
    {
        return base.CheckValueIsEmpty(value);
    }
}