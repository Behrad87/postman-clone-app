using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PostmanCloneWPFUI.Converters;

public class EmptyStringToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var empty = string.IsNullOrWhiteSpace(value as string);
        if (Invert) empty = !empty;
        return empty ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
