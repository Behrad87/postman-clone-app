using System.Globalization;
using System.Windows.Data;

namespace PostmanCloneWPFUI.Converters;

public class ElapsedTimeConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is long ms)
            return ms >= 1000 ? $"{ms / 1000.0:F2}s" : $"{ms}ms";
        return "–";
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}