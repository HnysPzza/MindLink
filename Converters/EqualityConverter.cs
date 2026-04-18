using System.Globalization;

namespace M1ndLink.Converters;

/// <summary>
/// Returns true when the binding value's ToString() equals ConverterParameter.
/// Useful for showing/hiding views based on enum phase values.
/// </summary>
public class EqualityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
