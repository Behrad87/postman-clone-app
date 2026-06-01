using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PostmanCloneWPFUI.Converters;

public class StringEqualityToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is string s && p is string param && s == param ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
