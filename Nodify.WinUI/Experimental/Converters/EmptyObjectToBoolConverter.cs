using CommunityToolkit.WinUI.Converters;

namespace Nodify.WinUI.Experimental.Converters;

/// <summary>
/// Converts null to false and non-null to true (or inverted)
/// </summary>
public sealed partial class EmptyObjectToBoolConverter : EmptyObjectToObjectConverter
{
    public EmptyObjectToBoolConverter()
    {
        EmptyValue = false;
        NotEmptyValue = true;
    }
}