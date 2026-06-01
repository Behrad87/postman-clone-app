using System.Globalization;
using System.Windows.Data;

namespace PostmanCloneWPFUI.Converters;

public class StringEqualityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is string s && p is string param && s == param;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
